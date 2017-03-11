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

        static bool[] blackKeys = new bool[kNotesPerOctave] { false, true, false, true,
                                                              false, false, true, false,
                                                              true, false, true, false };

        public static bool IsBlackKey(int key)
        {
            return blackKeys[key % kNotesPerOctave];
        }
    }
}
