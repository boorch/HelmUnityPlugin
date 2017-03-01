#include "AudioPluginUtil.h"
#include "helm_engine.h"

namespace Helm {

  const int MAX_BUFFER_SAMPLES = 256;

  enum Param {
    kInstance,
    kPolyphony,

    kOsc1Transpose,
    kOsc1Tune,
    kOsc1Waveform,
    kOsc2Transpose,
    kOsc2Tune,
    kOsc2Waveform,
    kCrossMod,
    kOscMix,

    kOscFeedbackAmount,
    kOscFeedbackTranspose,
    kOscFeedbackTune,

    kFilterType,
    kFilterCutoff,
    kFilterResonance,

    kFilterAttack,
    kFilterDecay,
    kFilterSustain,
    kFilterRelease,
    kFilterEnvelopeDepth,
    kFilterSaturation,
    kFilterKeyTrack,

    kAmpAttack,
    kAmpDecay,
    kAmpSustain,
    kAmpRelease,
    kVelocityTrack,

    kArpOn,
    kArpFrequency,
    kArpGate,
    kArpOctaves,
    kArpPattern,

    kDelayFrequency,
    kDelayFeedback,
    kDelayDryWet,

    kPortamento,
    kPortamentoType,
    kLegato,
    kNumParams
  };

  struct EffectData {
    float parameters[kNumParams];
    mopo::Value* value_lookup[kNumParams];
    int instance_id;
    mopo::HelmEngine synth_engine;
    Mutex mutex;
  };

  Mutex instance_mutex;
  int instance_counter = 0;
  std::map<int, EffectData*> instance_map;

  int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition) {
    int num_params = kNumParams;
    definition.paramdefs = new UnityAudioParameterDefinition[num_params];
    RegisterParameter(definition, "Instance", "", 0.0f, 1000.0f, 0.0f, 1.0f, 1.0f, kInstance);
    RegisterParameter(definition, "Polyphony", "", 1.0f, 32.0f, 1.0f, 1.0f, 1.0f, kPolyphony);

    RegisterParameter(definition, "Osc 1 Transpose", "Semitones", -48.0f, 48.0f, 0.0f, 1.0f, 1.0f, kOsc1Transpose);
    RegisterParameter(definition, "Osc 1 Tune", "Cents", -1.0f, 1.0f, 0.0f, 100.0f, 1.0f, kOsc1Tune);
    RegisterParameter(definition, "Osc 1 Waveform", "", 0.0f, 9.0f, 3.0f, 1.0f, 1.0f, kOsc1Waveform);
    RegisterParameter(definition, "Osc 2 Transpose", "Semitones", -48.0f, 48.0f, 0.0f, 1.0f, 1.0f, kOsc2Transpose);
    RegisterParameter(definition, "Osc 2 Tune", "Cents", -1.0f, 1.0f, 0.0f, 100.0f, 1.0f, kOsc2Tune);
    RegisterParameter(definition, "Osc 2 Waveform", "", 0.0f, 9.0f, 3.0f, 1.0f, 1.0f, kOsc2Waveform);

    RegisterParameter(definition, "Osc Cross Mod", "", 0.0f, 1.0f, 0.3f, 1.0f, 1.0f, kCrossMod);
    RegisterParameter(definition, "Osc Mix", "", 0.0f, 1.0f, 0.5f, 1.0f, 1.0f, kOscMix);

    RegisterParameter(definition, "Osc Feedback Transopose", "Semitones", -48.0f, 48.0f, -12.0f, 1.0f, 1.0f, kOscFeedbackTranspose);
    RegisterParameter(definition, "Osc Feedback Tune", "Cents", -1.0f, 1.0f, 0.0f, 100.0f, 1.0f, kOscFeedbackTune);
    RegisterParameter(definition, "Osc Feedback Amount", "", -1.0f, 1.0f, 0.0f, 1.0f, 1.0f, kOscFeedbackAmount);

    RegisterParameter(definition, "Filter Type", "", 0.0f, 5.0f, 0.0f, 1.0f, 1.0f, kFilterType);
    RegisterParameter(definition, "Filter Cutoff", "midi note", 0.0f, 128.0f, 64.0f, 1.0f, 1.0f, kFilterCutoff);
    RegisterParameter(definition, "Filter Resonance", "", 0.0f, 1.0f, 0.3f, 1.0f, 1.0f, kFilterResonance);

    RegisterParameter(definition, "Filter Attack", "secs", 0.0f, 4.0f, 0.3f, 1.0f, 1.0f, kFilterAttack);
    RegisterParameter(definition, "Filter Decay", "secs", 0.0f, 4.0f, 0.3f, 1.0f, 1.0f, kFilterDecay);
    RegisterParameter(definition, "Filter Sustain", "", 0.0f, 1.0f, 0.5f, 1.0f, 1.0f, kFilterSustain);
    RegisterParameter(definition, "Filter Release", "secs", 0.0f, 4.0f, 0.3f, 1.0f, 1.0f, kFilterRelease);
    RegisterParameter(definition, "Filter Envelope Depth", "midi note", -128.0f, 128.0f, 64.0f, 1.0f, 1.0f, kFilterEnvelopeDepth);
    RegisterParameter(definition, "Filter Saturation", "dB", 0.0f, 60.0f, 0.0f, 1.0f, 1.0f, kFilterSaturation);
    RegisterParameter(definition, "Filter Keytrack", "", -1.0f, 1.0f, 0.0f, 1.0f, 1.0f, kFilterKeyTrack);

    RegisterParameter(definition, "Amplitude Attack", "secs", 0.0f, 4.0f, 0.3f, 1.0f, 1.0f, kAmpAttack);
    RegisterParameter(definition, "Amplitude Decay", "secs", 0.0f, 4.0f, 0.3f, 1.0f, 1.0f, kAmpDecay);
    RegisterParameter(definition, "Amplitude Sustain", "", 0.0f, 1.0f, 0.5f, 1.0f, 1.0f, kAmpSustain);
    RegisterParameter(definition, "Amplitude Release", "secs", 0.0f, 4.0f, 0.3f, 1.0f, 1.0f, kAmpRelease);
    RegisterParameter(definition, "Velocity Track", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, kVelocityTrack);

    RegisterParameter(definition, "Arp On", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, kArpOn);
    RegisterParameter(definition, "Arp Gate", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, kArpGate);
    RegisterParameter(definition, "Arp Frequency", "", -1.0f, 4.0f, 0.0f, 1.0f, 1.0f, kArpFrequency);
    RegisterParameter(definition, "Arp Octaves", "", 1.0f, 4.0f, 1.0f, 1.0f, 1.0f, kArpOctaves);
    RegisterParameter(definition, "Arp Pattern", "", 1.0f, 4.0f, 1.0f, 1.0f, 1.0f, kArpPattern);

    RegisterParameter(definition, "Delay Frequency", "", -2.0f, 5.0f, 0.0f, 1.0f, 1.0f, kDelayFrequency);
    RegisterParameter(definition, "Delay Feedback", "", -1.0f, 1.0f, 0.0f, 1.0f, 1.0f, kDelayFeedback);
    RegisterParameter(definition, "Delay Dry/Wet", "", 0.0f, 1.0f, 0.3f, 1.0f, 1.0f, kDelayDryWet);

    RegisterParameter(definition, "Portamento", "", -9.0f, -1.0f, -8.0f, 1.0f, 1.0f, kPortamento);
    RegisterParameter(definition, "Portamento Type", "", 0.0f, 2.0f, 0.0f, 1.0f, 1.0f, kPortamentoType);
    RegisterParameter(definition, "Legato", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, kLegato);
    return num_params;
  }

  void initializeValueLookup(mopo::Value** lookup, mopo::control_map& controls) {
    lookup[kPolyphony] = controls["polyphony"];
    lookup[kOsc1Transpose] = controls["osc_1_transpose"];
    lookup[kOsc1Tune] = controls["osc_1_tune"];
    lookup[kOsc1Waveform] = controls["osc_1_waveform"];
    lookup[kOsc2Transpose] = controls["osc_2_transpose"];
    lookup[kOsc2Tune] = controls["osc_2_tune"];
    lookup[kOsc2Waveform] = controls["osc_2_waveform"];
    lookup[kCrossMod] = controls["cross_modulation"];
    lookup[kOscMix] = controls["osc_mix"];
    lookup[kOscFeedbackAmount] = controls["osc_feedback_amount"];
    lookup[kOscFeedbackTranspose] = controls["osc_feedback_transpose"];
    lookup[kOscFeedbackTune] = controls["osc_feedback_tune"];
    lookup[kFilterType] = controls["filter_type"];
    lookup[kFilterCutoff] = controls["cutoff"];
    lookup[kFilterResonance] = controls["resonance"];
    lookup[kFilterAttack] = controls["fil_attack"];
    lookup[kFilterDecay] = controls["fil_decay"];
    lookup[kFilterSustain] = controls["fil_sustain"];
    lookup[kFilterRelease] = controls["fil_release"];
    lookup[kFilterEnvelopeDepth] = controls["fil_env_depth"];
    lookup[kFilterSaturation] = controls["filter_saturation"];
    lookup[kFilterKeyTrack] = controls["keytrack"];
    lookup[kAmpAttack] = controls["amp_attack"];
    lookup[kAmpDecay] = controls["amp_decay"];
    lookup[kAmpSustain] = controls["amp_sustain"];
    lookup[kAmpRelease] = controls["amp_release"];
    lookup[kVelocityTrack] = controls["velocity_track"];
    lookup[kArpOn] = controls["arp_on"];
    lookup[kArpFrequency] = controls["arp_frequency"];
    lookup[kArpGate] = controls["arp_gate"];
    lookup[kArpOctaves] = controls["arp_octaves"];
    lookup[kArpPattern] = controls["arp_pattern"];
    lookup[kDelayFrequency] = controls["delay_frequency"];
    lookup[kDelayFeedback] = controls["delay_feedback"];
    lookup[kDelayDryWet] = controls["delay_dry_wet"];
    lookup[kPortamento] = controls["portamento"];
    lookup[kPortamentoType] = controls["portamento_type"];
    lookup[kLegato] = controls["legato"];
  }


  UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state) {
    EffectData* effect_data = new EffectData;
    InitParametersFromDefinitions(InternalRegisterEffectDefinition, effect_data->parameters);
    mopo::control_map controls = effect_data->synth_engine.getControls();
    initializeValueLookup(effect_data->value_lookup, controls);
    effect_data->synth_engine.setSampleRate(state->samplerate);

    state->effectdata = effect_data;

    effect_data->instance_id = instance_counter;
    instance_map[instance_counter] = effect_data;
    effect_data->parameters[kInstance] = instance_counter;
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
    delete data;
    return UNITY_AUDIODSP_OK;
  }

  UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(
      UnityAudioEffectState* state, int index, float value) {
    EffectData* data = state->GetEffectData<EffectData>();
    if (index >= kNumParams)
      return UNITY_AUDIODSP_ERR_UNSUPPORTED;
    data->parameters[index] = value;
    return UNITY_AUDIODSP_OK;
  }

  UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(
      UnityAudioEffectState* state, int index, float* value, char *valuestr) {
    EffectData* data = state->GetEffectData<EffectData>();
    if (index >= kNumParams)
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
    if (instance_map.count(instance))
      instance_map[instance]->synth_engine.noteOn(note);
  }

  extern "C" UNITY_AUDIODSP_EXPORT_API void HelmNoteOff(int instance, int note) {
    if (instance_map.count(instance))
      instance_map[instance]->synth_engine.noteOff(note);
  }
}
