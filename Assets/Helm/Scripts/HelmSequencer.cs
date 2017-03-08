using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Tytel
{
    public class HelmSequencer : MonoBehaviour
    {
        [DllImport("AudioPluginHelm")]
        private static extern IntPtr CreateSequencer();

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr DeleteSequencer(IntPtr sequencer);

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr EnableSequencer(IntPtr sequencer, bool enable);

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr CreateNote(IntPtr sequencer, int note, float start, float end);

        IntPtr sequencer = IntPtr.Zero;

        void CreateNativeSequencer()
        {
            if (sequencer == IntPtr.Zero)
                sequencer = CreateSequencer();
        }

        void DeleteNativeSequencer()
        {
            if (sequencer != IntPtr.Zero)
                DeleteSequencer(sequencer);
            sequencer = IntPtr.Zero;
        }

        void Awake()
        {
            sequencer = CreateSequencer();
        }

        void OnDestroy()
        {
            DeleteNativeSequencer();
        }

        void OnEnable()
        {
            if (sequencer != IntPtr.Zero)
                EnableSequencer(sequencer, true);
        }

        void OnDisable()
        {
            if (sequencer != IntPtr.Zero)
                EnableSequencer(sequencer, false);
        }

        void Start()
        {
            if (sequencer != IntPtr.Zero)
            {
                IntPtr note = CreateNote(sequencer, 50, 1, 5);
                IntPtr note2 = CreateNote(sequencer, 55, 6, 10);
            }
        }
    }
}
