#include "AudioPluginUtil.h"
#include "helm_engine.h"

namespace Helm {

  const int MAX_BUFFER_SAMPLES = 256;

  enum Param {
    kInstance,
    kNumParams
  };

  struct EffectData {
    float* parameters;
    mopo::Value** value_lookup;
    int instance_id;
    mopo::HelmEngine synth_engine;
    Mutex mutex;
  };

  Mutex instance_mutex;
  int instance_counter = 0;
  std::map<int, EffectData*> instance_map;

  int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition) {
    std::map<std::string, mopo::ValueDetails> parameters = mopo::Parameters::lookup_.getAllDetails();
    int num_synth_parameters = parameters.size();
    int num_plugin_params = kNumParams;

    definition.paramdefs = new UnityAudioParameterDefinition[num_synth_parameters + num_plugin_params];
    RegisterParameter(definition, "Instance", "", 0.0f, 1000.0f, 0.0f, 1.0f, 1.0f, kInstance);

    int index = kNumParams;
    for (auto parameter : parameters) {
      mopo::ValueDetails& details = parameter.second;
      RegisterParameter(definition, details.display_name.c_str(), details.display_units.c_str(),
                        details.min, details.max, details.default_value,
                        1.0f, 1.0f, index);
      index++;
    }

    return index;
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
    int num_parameters = mopo::Parameters::lookup_.getAllDetails().size() + kNumParams;

    effect_data->parameters = new float[num_parameters];
    InitParametersFromDefinitions(InternalRegisterEffectDefinition, effect_data->parameters);

    effect_data->value_lookup = new mopo::Value*[num_parameters];
    mopo::control_map controls = effect_data->synth_engine.getControls();
    initializeValueLookup(effect_data->value_lookup, controls);

    effect_data->synth_engine.setSampleRate(state->samplerate);

    state->effectdata = effect_data;
    MutexScopeLock mutex_lock(instance_mutex);
    effect_data->instance_id = instance_counter;
    effect_data->parameters[kInstance] = instance_counter;
    instance_map[instance_counter] = effect_data;
    instance_counter++;
    return UNITY_AUDIODSP_OK;
  }

  void clearInstance(int id) {
    instance_map.erase(id);
  }

  UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state) {
    EffectData* data = state->GetEffectData<EffectData>();
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

  extern "C" UNITY_AUDIODSP_EXPORT_API void HelmNoteOn(int instance, int note) {
    if (instance_map.count(instance)) {
      MutexScopeLock mutex_lock(instance_map[instance]->mutex);
      instance_map[instance]->synth_engine.noteOn(note);
    }
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void HelmNoteOff(int instance, int note) {

    if (instance_map.count(instance)) {
      MutexScopeLock mutex_lock(instance_map[instance]->mutex);
      instance_map[instance]->synth_engine.noteOff(note);
    }
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void HelmAllNotesOff(int instance) {

    if (instance_map.count(instance)) {
      MutexScopeLock mutex_lock(instance_map[instance]->mutex);
      instance_map[instance]->synth_engine.allNotesOff();
    }
  }
}
