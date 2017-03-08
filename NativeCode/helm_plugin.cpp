/* Copyright 2017 Matt Tytel */

#include "helm_engine.h"
#include "helm_sequencer.h"
#include "AudioPluginUtil.h"

namespace Helm {

  const int MAX_BUFFER_SAMPLES = 256;
  const int MAX_CHARACTERS = 15;
  const int MAX_CHANNELS = 16;
  const int MAX_NOTES = 128;

  enum Param {
    kChannel,
    kNumParams
  };

  struct EffectData {
    int num_parameters;
    int note_events[MAX_NOTES];
    float* parameters;
    mopo::Value** value_lookup;
    int instance_id;
    mopo::HelmEngine synth_engine;
    Mutex mutex;
  };

  Mutex instance_mutex;
  int instance_counter = 0;
  std::map<int, EffectData*> instance_map;

  Mutex sequencer_mutex;
  std::map<HelmSequencer*, bool> sequencer_lookup;

  int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition) {
    std::map<std::string, mopo::ValueDetails> parameters = mopo::Parameters::lookup_.getAllDetails();
    int num_synth_parameters = parameters.size();
    int num_plugin_params = kNumParams;

    definition.paramdefs = new UnityAudioParameterDefinition[num_synth_parameters + num_plugin_params];
    RegisterParameter(definition, "Channel", "", 0.0f, MAX_CHANNELS, 0.0f, 1.0f, 1.0f, kChannel);

    int index = kNumParams;
    for (auto parameter : parameters) {
      mopo::ValueDetails& details = parameter.second;
      std::string name = details.name.substr(0, MAX_CHARACTERS);
      std::string units = details.display_units.substr(0, MAX_CHARACTERS);
      RegisterParameter(definition, name.c_str(), units.c_str(),
                        details.min, details.max, details.default_value,
                        1.0f, 1.0f, index);
      index++;
    }

    return num_synth_parameters + kNumParams;
  }

  void initializeValueLookup(mopo::Value** lookup, mopo::control_map& controls) {
    std::map<std::string, mopo::ValueDetails> parameters = mopo::Parameters::lookup_.getAllDetails();

    int index = 0;
    for (; index < kNumParams; ++index)
      lookup[index] = 0;

    for (auto parameter : parameters) {
      mopo::ValueDetails& details = parameter.second;
      lookup[index] = controls[details.name];
      index++;
    }
  }

  UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state) {
    EffectData* effect_data = new EffectData;
    MutexScopeLock mutex_lock(effect_data->mutex);
    int num_parameters = mopo::Parameters::lookup_.getAllDetails().size() + kNumParams;
    memset(effect_data->note_events, 0, sizeof(int) * MAX_NOTES);
    effect_data->num_parameters = num_parameters;

    effect_data->parameters = new float[num_parameters];
    InitParametersFromDefinitions(InternalRegisterEffectDefinition, effect_data->parameters);

    effect_data->value_lookup = new mopo::Value*[num_parameters];
    mopo::control_map controls = effect_data->synth_engine.getControls();
    initializeValueLookup(effect_data->value_lookup, controls);

    effect_data->synth_engine.setSampleRate(state->samplerate);

    state->effectdata = effect_data;
    MutexScopeLock mutex_instance_lock(instance_mutex);
    effect_data->instance_id = instance_counter;
    instance_map[instance_counter] = effect_data;
    instance_counter++;
    return UNITY_AUDIODSP_OK;
  }

  void clearInstance(int id) {
    instance_map.erase(id);
  }

  UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state) {
    EffectData* data = state->GetEffectData<EffectData>();
    MutexScopeLock mutex_lock(data->mutex);
    MutexScopeLock mutex_instance_lock(instance_mutex);
    data->synth_engine.allNotesOff();
    clearInstance(data->instance_id);
    delete[] data->parameters;
    delete[] data->value_lookup;
    delete data;
    return UNITY_AUDIODSP_OK;
  }

  UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(
      UnityAudioEffectState* state, int index, float value) {
    EffectData* data = state->GetEffectData<EffectData>();

    if (index < 0 || index >= data->num_parameters)
      return UNITY_AUDIODSP_ERR_UNSUPPORTED;

    data->parameters[index] = value;

    if (data->value_lookup[index]) {
      MutexScopeLock mutex_lock(data->mutex);
      data->value_lookup[index]->set(value);
    }
    return UNITY_AUDIODSP_OK;
  }

  UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(
      UnityAudioEffectState* state, int index, float* value, char *valuestr) {
    EffectData* data = state->GetEffectData<EffectData>();
    if (index < 0 || index >= data->num_parameters)
      return UNITY_AUDIODSP_ERR_UNSUPPORTED;
    if (value != NULL)
      *value = data->parameters[index];
    if (valuestr != NULL)
      valuestr[0] = 0;
    return UNITY_AUDIODSP_OK;
  }

  int UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback(UnityAudioEffectState* state, const char* name,
                                                     float* buffer, int numsamples) {
    return UNITY_AUDIODSP_OK;
  }

  void processSequencerNotes(EffectData* data, int current_sample, int samples) {
    HelmSequencer* active_sequencer = NULL;

    sequencer_mutex.Lock();
    for (auto sequencer : sequencer_lookup) {
      if (sequencer.first->channel() == data->parameters[kChannel])
        active_sequencer = sequencer.first;
    }

    sequencer_mutex.Unlock();
  }

  void processAudio(mopo::HelmEngine& engine, float* buffer, int channels, int samples, int offset) {
    if (engine.getBufferSize() != samples)
      engine.setBufferSize(samples);

    engine.process();

    const mopo::mopo_float* engine_output_left = engine.output(0)->buffer;
    const mopo::mopo_float* engine_output_right = engine.output(1)->buffer;
    for (int channel = 0; channel < channels; ++channel) {
      const mopo::mopo_float* synth_output = (channel % 2) ? engine_output_right : engine_output_left;

      for (int i = 0; i < samples; ++i) {
        buffer[(i + offset) * channels + channel] = synth_output[i];
      }
    }
  }

  UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(
      UnityAudioEffectState* state,
      float* in_buffer, float* out_buffer, unsigned int num_samples,
      int in_channels, int out_channels) {
    EffectData* data = state->GetEffectData<EffectData>();

    MutexScopeLock mutex_lock(data->mutex);
    int synth_samples = num_samples > MAX_BUFFER_SAMPLES ? MAX_BUFFER_SAMPLES : num_samples;

    for (int b = 0; b < num_samples; b += synth_samples) {
      int current_samples = std::min<int>(synth_samples, num_samples - b);

      processAudio(data->synth_engine, out_buffer, out_channels, current_samples, b);
    }

    return UNITY_AUDIODSP_OK;
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void HelmNoteOn(int channel, int note) {
    for (auto synth : instance_map) {
      if (((int)synth.second->parameters[kChannel]) == channel) {
        MutexScopeLock mutex_lock(synth.second->mutex);
        synth.second->synth_engine.noteOn(note);
      }
    }
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void HelmNoteOff(int channel, int note) {
    for (auto synth : instance_map) {
      if (((int)synth.second->parameters[kChannel]) == channel) {
        MutexScopeLock mutex_lock(synth.second->mutex);
        synth.second->synth_engine.noteOff(note);
      }
    }
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void HelmAllNotesOff(int channel) {
    for (auto synth : instance_map) {
      if (((int)synth.second->parameters[kChannel]) == channel) {
        MutexScopeLock mutex_lock(synth.second->mutex);
        synth.second->synth_engine.allNotesOff();
      }
    }
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API HelmSequencer* CreateSequencer() {
    HelmSequencer* sequencer = new HelmSequencer();
    MutexScopeLock mutex_lock(sequencer_mutex);
    sequencer_lookup[sequencer] = false;
    return sequencer;
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void DeleteSequencer(HelmSequencer* sequencer) {
    sequencer_mutex.Lock();
    sequencer_lookup.erase(sequencer);
    sequencer_mutex.Unlock();
    delete sequencer;
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void EnableSequencer(HelmSequencer* sequencer, bool enable) {
    MutexScopeLock mutex_lock(sequencer_mutex);
    sequencer_lookup[sequencer] = enable;
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API HelmSequencer::Note* CreateNote(
      HelmSequencer* sequencer, int note, float start, float end) {
    return sequencer->addNote(note, start, end);
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void DeleteNote(
      HelmSequencer* sequencer, HelmSequencer::Note* note) {
    sequencer->deleteNote(note);
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void ChangeNoteStart(
      HelmSequencer* sequencer, HelmSequencer::Note* note, float new_start) {
    sequencer->changeNoteStart(note, new_start);
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void ChangeNoteEnd(
      HelmSequencer* sequencer, HelmSequencer::Note* note, float new_end) {
    sequencer->changeNoteEnd(note, new_end);
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API bool ChangeSequencerChannel(
      HelmSequencer* sequencer, int channel) {
    sequencer->setChannel(channel);

    for (auto sequencer : sequencer_lookup) {
      if (sequencer.first->channel() == channel)
        return false;
    }
    return true;
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void ChangeSequencerLength(
      HelmSequencer* sequencer, float length) {
    sequencer->setLength(length);
  }
}
