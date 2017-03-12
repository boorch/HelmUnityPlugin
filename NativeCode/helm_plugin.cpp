/* Copyright 2017 Matt Tytel */

#include "helm_engine.h"
#include "helm_sequencer.h"
#include "AudioPluginUtil.h"

namespace Helm {

  const int MAX_BUFFER_SAMPLES = 256;
  const int MAX_CHARACTERS = 15;
  const int MAX_CHANNELS = 16;
  const int MAX_NOTES = 128;
  const int MAX_MODULATIONS = 16;
  const int VALUES_PER_MODULATION = 3;
  const float MODULATION_RANGE = 1000000.0f;
  const double BPM_TO_SIXTEENTH = 4.0 / 60.0;

  const std::map<std::string, std::string> REPLACE_STRINGS = {
    {"stutter_resample", "stutter_resamp"}
  };

  enum Param {
    kChannel,
    kNumParams
  };

  struct EffectData {
    int num_parameters;
    int num_synth_parameters;
    HelmSequencer::Note* note_events[MAX_NOTES];
    mopo::ModulationConnection* modulations[MAX_MODULATIONS];
    float* parameters;
    mopo::Value** value_lookup;
    int instance_id;
    mopo::HelmEngine synth_engine;
    Mutex mutex;
  };

  Mutex instance_mutex;
  int instance_counter = 0;
  double bpm = 120.0;
  std::map<int, EffectData*> instance_map;

  Mutex sequencer_mutex;
  std::map<HelmSequencer*, bool> sequencer_lookup;

  std::string getValueName(std::string full_name) {
    std::string name = full_name;
    for (auto replace : REPLACE_STRINGS) {
      size_t index = name.find(replace.first);
      if (index != std::string::npos)
        name = name.substr(0, index) + replace.second + name.substr(index + replace.first.length());
    }
    name.erase(std::remove(name.begin(), name.end(), '_'), name.end());
    return name.substr(0, MAX_CHARACTERS);
  }

  int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition) {
    std::map<std::string, mopo::ValueDetails> parameters = mopo::Parameters::lookup_.getAllDetails();

    int num_synth_params = parameters.size();
    int num_modulation_params = MAX_MODULATIONS * VALUES_PER_MODULATION;
    int num_plugin_params = kNumParams;
    int total_params = num_synth_params + num_plugin_params + num_modulation_params;

    definition.paramdefs = new UnityAudioParameterDefinition[total_params];
    RegisterParameter(definition, "Channel", "", 0.0f, MAX_CHANNELS, 0.0f, 1.0f, 1.0f, kChannel);

    int index = kNumParams;
    for (auto parameter : parameters) {
      mopo::ValueDetails& details = parameter.second;
      std::string name = getValueName(details.name);
      std::string units = details.display_units.substr(0, MAX_CHARACTERS);
      RegisterParameter(definition, name.c_str(), units.c_str(),
                        details.min, details.max, details.default_value,
                        1.0f, 1.0f, index);
      index++;
    }

    for (int m = 0; m < MAX_MODULATIONS; ++m) {
      std::string name = std::string("mod") + std::to_string(m);
      std::string source_name = name + "source";
      std::string dest_name = name + "dest";
      std::string value_name = name + "value";
      RegisterParameter(definition, source_name.c_str(), "", 0.0f, MODULATION_RANGE, 0.0f,
                        1.0f, 1.0f, index++);
      RegisterParameter(definition, dest_name.c_str(), "", 0.0f, MODULATION_RANGE, 0.0f,
                        1.0f, 1.0f, index++);
      RegisterParameter(definition, value_name.c_str(), "", -MODULATION_RANGE, MODULATION_RANGE, 0.0f,
                        1.0f, 1.0f, index++);
    }

    return num_synth_params + kNumParams + num_modulation_params;
  }

  void initializeValueLookup(mopo::Value** lookup, mopo::control_map& controls, int num_params) {
    std::map<std::string, mopo::ValueDetails> parameters = mopo::Parameters::lookup_.getAllDetails();

    for (int i = 0; i < num_params; ++i)
      lookup[i] = 0;

    int index = kNumParams;
    for (auto parameter : parameters) {
      mopo::ValueDetails& details = parameter.second;
      lookup[index] = controls[details.name];
      index++;
    }
  }

  UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state) {
    EffectData* effect_data = new EffectData;
    MutexScopeLock mutex_lock(effect_data->mutex);
    memset(effect_data->note_events, 0, sizeof(HelmSequencer::Note*) * MAX_NOTES);

    effect_data->num_synth_parameters = mopo::Parameters::lookup_.getAllDetails().size();
    int num_params = effect_data->num_synth_parameters + kNumParams + MAX_MODULATIONS * VALUES_PER_MODULATION;
    effect_data->num_parameters = num_params;

    effect_data->parameters = new float[num_params];
    InitParametersFromDefinitions(InternalRegisterEffectDefinition, effect_data->parameters);

    effect_data->value_lookup = new mopo::Value*[num_params];
    mopo::control_map controls = effect_data->synth_engine.getControls();
    initializeValueLookup(effect_data->value_lookup, controls, num_params);

    for (int i = 0; i < MAX_MODULATIONS; ++i)
      effect_data->modulations[i] = new mopo::ModulationConnection();

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

    for (int i = 0; i < MAX_MODULATIONS; ++i) {
      if (data->synth_engine.isModulationActive(data->modulations[i]))
        data->synth_engine.disconnectModulation(data->modulations[i]);
      delete data->modulations[i];
    }
    
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

    int modulation_start = kNumParams + data->num_synth_parameters;
    if (index >= modulation_start) {
      int mod_param = index - modulation_start;
      int mod_index = mod_param / VALUES_PER_MODULATION;
      int mod_type = mod_param % VALUES_PER_MODULATION;

      mopo::ModulationConnection* connection = data->modulations[mod_index];

      if (mod_type == 0) {
        if (data->synth_engine.isModulationActive(connection))
          data->synth_engine.disconnectModulation(connection);

        int source_index = value;
        mopo::output_map sources = data->synth_engine.getModulationSources();
        auto source = sources.begin();
        std::advance(source, source_index);
        connection->source = source->first;
      }
      else if (mod_type == 1) {
        if (data->synth_engine.isModulationActive(connection))
          data->synth_engine.disconnectModulation(connection);

        mopo::output_map monoMods = data->synth_engine.getMonoModulations();
        int dest_index = value;
        if (dest_index < monoMods.size()) {
          auto mod = monoMods.begin();
          std::advance(mod, dest_index);
          connection->destination = mod->first;
        }
        else {
          dest_index -= monoMods.size();
          mopo::output_map polyMods = data->synth_engine.getPolyModulations();
          auto mod = polyMods.begin();
          std::advance(mod, dest_index);
          connection->destination = mod->first;
        }
      }
      else {
        if (value == 0.0f) {
          if (data->synth_engine.isModulationActive(connection))
            data->synth_engine.disconnectModulation(connection);
        }
        else {
          connection->amount.set(value);
          if (!data->synth_engine.isModulationActive(connection))
            data->synth_engine.connectModulation(connection);
        }
      }
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

  double timeToSixteenth(UInt64 sample, int sample_rate) {
    return (bpm * BPM_TO_SIXTEENTH * sample) / sample_rate;
  }

  double wrap(double value, double length) {
    int wrap = value / length;
    return value - wrap * length;
  }

  void processNotes(EffectData* data, HelmSequencer* sequencer, UInt64 current_sample, UInt64 end_sample) {
    double start = timeToSixteenth(current_sample, data->synth_engine.getSampleRate());
    start = wrap(start, sequencer->length());
    double end = timeToSixteenth(end_sample, data->synth_engine.getSampleRate());
    end = wrap(end, sequencer->length());

    sequencer->getNoteOffs(data->note_events, start, end);

    data->mutex.Lock();
    for (int i = 0; i < MAX_NOTES && data->note_events[i]; ++i)
      data->synth_engine.noteOff(data->note_events[i]->midi_note);

    data->mutex.Unlock();

    sequencer->getNoteOns(data->note_events, start, end);

    data->mutex.Lock();
    for (int i = 0; i < MAX_NOTES && data->note_events[i]; ++i)
      data->synth_engine.noteOn(data->note_events[i]->midi_note, data->note_events[i]->velocity);
    data->mutex.Unlock();
  }

  void processSequencerNotes(EffectData* data, UInt64 current_sample, UInt64 end_sample) {
    MutexScopeLock sequencer_lock(sequencer_mutex);
    for (auto sequencer : sequencer_lookup) {
      if (sequencer.second && sequencer.first->channel() == data->parameters[kChannel])
        processNotes(data, sequencer.first, current_sample, end_sample);
    }
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

      processSequencerNotes(data, state->currdsptick + b, state->currdsptick + b + current_samples + 1);
      processAudio(data->synth_engine, out_buffer, out_channels, current_samples, b);
    }

    return UNITY_AUDIODSP_OK;
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void HelmNoteOn(int channel, int note, float velocity) {
    for (auto synth : instance_map) {
      if (((int)synth.second->parameters[kChannel]) == channel) {
        MutexScopeLock mutex_lock(synth.second->mutex);
        synth.second->synth_engine.noteOn(note, velocity);
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
      HelmSequencer* sequencer, int note, float velocity, float start, float end) {
    return sequencer->addNote(note, velocity, start, end);
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void DeleteNote(
      HelmSequencer* sequencer, HelmSequencer::Note* note) {
    HelmNoteOff(sequencer->channel(), note->midi_note);
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

  extern "C" UNITY_AUDIODSP_EXPORT_API void ChangeNoteVelocity(HelmSequencer::Note* note, float new_velocity) {
    note->velocity = new_velocity;
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

  extern "C" UNITY_AUDIODSP_EXPORT_API void SetBpm(float new_bpm) {
    bpm = new_bpm;
  }
}
