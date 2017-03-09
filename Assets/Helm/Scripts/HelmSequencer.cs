using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tytel
{
    public class HelmSequencer : MonoBehaviour
    {
        [DllImport("AudioPluginHelm")]
        private static extern IntPtr CreateSequencer();

        [DllImport("AudioPluginHelm")]
        private static extern void DeleteSequencer(IntPtr sequencer);

        [DllImport("AudioPluginHelm")]
        private static extern void EnableSequencer(IntPtr sequencer, bool enable);

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr CreateNote(IntPtr sequencer, int note, float velocity, float start, float end);

        [DllImport("AudioPluginHelm")]
        private static extern void ChangeSequencerLength(IntPtr sequencer, float length);

        [DllImport("AudioPluginHelm")]
        private static extern bool ChangeSequencerChannel(IntPtr sequencer, int channel);

        public struct Note
        {
            int note;
            float velocity;
            float start;
            float end;
        }

        public int rows = 127;
        public int channel = 0;
        public List<List<Note>> allNotes = new List<List<Note>>();

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
                for (int i = 0; i < 16; ++i)
                    CreateNote(sequencer, 24 + i * 3, 1.0f, 0.5f * i, 0.5f + 0.5f);

                ChangeSequencerLength(sequencer, 8.0f);
            }
        }
    }
}
