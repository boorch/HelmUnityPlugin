// Copyright 2017 Matt Tytel

using UnityEngine;

namespace Tytel
{
    public class Utils : MonoBehaviour
    {
        public const int kMidiSize = 128;
        public const int kNotesPerOctave = 12;
        public const int kMaxChannels = 16;
        public const float kBpmToSixteenths = 4.0f / 60.0f;
        public const int kMinOctave = -2;

        static bool[] blackKeys = new bool[kNotesPerOctave] { false, true, false, true,
                                                              false, false, true, false,
                                                              true, false, true, false };

        public static bool IsBlackKey(int key)
        {
            return blackKeys[key % kNotesPerOctave];
        }

        public static int GetOctave(int key)
        {
            return key / kNotesPerOctave + kMinOctave;
        }

        public static void InitAudioSource(AudioSource audio)
        {
            AudioClip one = AudioClip.Create("one", 1, 1, AudioSettings.outputSampleRate, false);
            one.SetData(new float[] { 1 }, 0);

            audio.clip = one;
            audio.loop = true;
            if (Application.isPlaying)
                audio.Play();
        }
    }
}
