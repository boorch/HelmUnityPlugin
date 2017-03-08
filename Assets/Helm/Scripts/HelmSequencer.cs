using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Tytel
{
    [ExecuteInEditMode]
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

        IntPtr sequencer = null;

        void CreateNativeSequencer()
        {
            if (sequencer == null)
                sequencer = CreateSequencer();
        }

        void DeleteNativeSequencer()
        {
            if (sequencer != null)
                DeleteSequencer(sequencer);
            sequencer = null;
        }

        void Awake()
        {
            CreateSequencer();
        }

        void OnDestroy()
        {
            DeleteNativeSequencer();
        }

        void OnEnable()
        {
            if (sequencer != null)
                EnableSequencer(sequencer, true);
        }

        void OnDisable()
        {
            if (sequencer != null)
                EnableSequencer(sequencer, false);
        }

        void Start()
        {
            CreateNativeSequencer();
            IntPtr note = Cr eateNote(sequencer, 50, 1, 5);
            IntPtr note2 = CreateNote(sequencer, 55, 6, 10);
        }
    }
}
