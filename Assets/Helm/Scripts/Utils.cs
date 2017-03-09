using UnityEngine;

namespace Tytel
{
    public class Utils : MonoBehaviour
    {
        public const int kMidiSize = 128;
        public const int kNotesPerOctave = 12;
        public const int kMaxChannels = 16;

        static bool[] blackKeys = new bool[kNotesPerOctave] { false, true, false, true,
                                                              false, false, true, false,
                                                              true, false, true, false };

        public static bool IsBlackKey(int key)
        {
            return blackKeys[key % kNotesPerOctave];
        }
    }
}
