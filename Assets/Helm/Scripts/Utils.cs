// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Reflection;

namespace Helm
{
    public enum CommonParam
    {
        kNone = 0,

        // Filter
        kCutoff = Param.kCutoff,
        kResonance = Param.kResonance,
        kFilterAttack = Param.kFilterAttack,
        kFilterRelease = Param.kFilterRelease,
        kFilterEnvelopeDepth = Param.kFilterEnvelopeDepth,

        // Formant
        kFormantX = Param.kFormantX,
        kFormantY = Param.kFormantY,

        // Amplitude
        kAmplitudeAttack = Param.kAmplitudeAttack,
        kAmplitudeRelease = Param.kAmplitudeRelease,

        // Oscillators
        kCrossMod = Param.kCrossMod,
        kOsc1Tune = Param.kOsc1Tune,
        kOsc2Tune = Param.kOsc2Tune,
        kOsc1Transpose = Param.kOsc1Transpose,
        kOsc2Transpose = Param.kOsc2Transpose,
        kOsc1UnisonDetune = Param.kOsc1UnisonDetune,
        kOsc2UnisonDetune = Param.kOsc2UnisonDetune,

        // Delay
        kDelayFrequency = Param.kDelayFrequency,
        kDelayTempo = Param.kDelayTempo,
    }

    public enum Param
    {
        kNone = 0,
        kAmplitudeAttack = 1,
        kAmplitudeDecay,
        kAmplitudeSustain,
        kAmplitudeRelease,
        kArpFrequency,
        kArpGate,
        kArpOctaves,
        kArpOn,
        kArpPattern,
        kArpSync,
        kArpTempo,
        kCrossMod = 13,
        kCutoff,
        kDelayDryWet,
        kDelayFeedback,
        kDelayFrequency,
        kDelayOn,
        kDelaySync,
        kDelayTempo,
        kFilterAttack,
        kFilterDecay,
        kFilterEnvelopeDepth,
        kFilterRelease,
        kFilterSustain,
        kSaturation,
        kFilterType,
        kFormantOn,
        kFormantX,
        kFormantY,
        kKeytrack,
        kLegato,
        kModAttack,
        kModDecay,
        kModRelease,
        kModSustain,
        kMonoLfo1Amplitude,
        kMonoLfo1Frequency,
        kMonoLfo1Retrigger,
        kMonoLfo1Sync,
        kMonoLfo1Tempo,
        kMonoLfo1Waveform,
        kMonoLfo2Amplitude,
        kMonoLfo2Frequency,
        kMonoLfo2Retrigger,
        kMonoLfo2Sync,
        kMonoLfo2Tempo,
        kMonoLfo2Waveform,
        kNoiseVolume,
        kNumSteps,
        kOsc1Transpose,
        kOsc1Tune,
        kOsc1UnisonDetune,
        kOsc1UnisonVoices,
        kOsc1Volume,
        kOsc1Waveform,
        kOsc2Transpose,
        kOsc2Tune,
        kOsc2UnisonDetune,
        kOsc2UnisonVoices,
        kOsc2Volume,
        kOsc2Waveform,
        kOscFeedbackAmount,
        kOscFeedbackTranspose,
        kOscFeedbackTune,
        kPitchBendRange = 67,
        kPolyLfoAmplitude,
        kPolyLfoFrequency,
        kPolyLfoSync,
        kPolyLfoTempo,
        kPolyLfoWaveform,
        kPolyphony,
        kPortamento,
        kPortamentoType,
        kResonance,
        kReverbDamping,
        kReverbDryWet,
        kReverbFeedback,
        kReverbOn,
        kStepFrequency,
        kStepSequencerRetrigger = 114,
        kStepSequencerSync,
        kStepSequencerTempo,
        kStepSmoothing,
        kStutterFrequency,
        kStutterOn,
        kStutterResampleFrequency,
        kStutterResampleSync,
        kStutterResampleTempo,
        kStutterSoftness,
        kStutterSync,
        kStutterTempo,
        kSubShuffle,
        kSubVolume = 129,
        kSubWaveform,
        kOsc1UnisonHarmonize,
        kOsc2UnisonHarmonize,
        kVelocityTrack,
        kVolume
    }

    public class Utils : MonoBehaviour
    {
        public const int kMidiSize = 128;
        public const int kNotesPerOctave = 12;
        public const int kMaxChannels = 16;
        public const float kBpmToSixteenths = 4.0f / 60.0f;
        public const int kMinOctave = -2;
        public const int kMiddleC = 60;

        static bool[] blackKeys = new bool[kNotesPerOctave] { false, true, false, true,
                                                              false, false, true, false,
                                                              true, false, true, false };

        public static bool IsBlackKey(int key)
        {
            return blackKeys[key % kNotesPerOctave];
        }

        public static bool IsC(int key)
        {
            return key % kNotesPerOctave == 0;
        }

        public static int GetOctave(int key)
        {
            return key / kNotesPerOctave + kMinOctave;
        }

        public static float MidiChangeToRatio(int midi)
        {
            return Mathf.Pow(2, (1.0f * midi) / kNotesPerOctave);
        }

        public static void InitAudioSource(AudioSource audio)
        {
            AudioClip one = AudioClip.Create("helm", 1, 1, AudioSettings.outputSampleRate, false);
            one.SetData(new float[] { 1 }, 0);

            audio.clip = one;
            audio.loop = true;
            if (Application.isPlaying)
                audio.Play();
        }

        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                 BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] properties = type.GetProperties(flags);
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite && !property.IsDefined(typeof(ObsoleteAttribute), true))
                {
                    try
                    {
                        property.SetValue(copy, property.GetValue(original, null), null);
                    }
                    catch { }
                }
            }
            FieldInfo[] fields = type.GetFields(flags);
            foreach (FieldInfo field in fields)
                field.SetValue(copy, field.GetValue(original));

            return copy as T;
        }
    }
}
