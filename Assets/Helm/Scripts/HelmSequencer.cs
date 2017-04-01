// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Helm
{
    [RequireComponent(typeof(HelmAudioInit))]
    public class HelmSequencer : Sequencer
    {
        [DllImport("AudioPluginHelm")]
        private static extern IntPtr CreateSequencer();

        [DllImport("AudioPluginHelm")]
        private static extern void DeleteSequencer(IntPtr sequencer);

        [DllImport("AudioPluginHelm")]
        private static extern void EnableSequencer(IntPtr sequencer, bool enable);

        [DllImport("AudioPluginHelm")]
        private static extern void ChangeSequencerLength(IntPtr sequencer, float length);

        [DllImport("AudioPluginHelm")]
        private static extern bool ChangeSequencerChannel(IntPtr sequencer, int channel);

        [DllImport("AudioPluginHelm")]
        private static extern void SyncSequencerStart(IntPtr sequencer, double dspTime);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOn(int channel, int note, float velocity);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOff(int channel, int note);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmAllNotesOff(int channel);

        [DllImport("AudioPluginHelm")]
        private static extern bool HelmChangeParameter(int channel, int paramIndex, float newValue);

        public int channel = 0;
        IntPtr reference = IntPtr.Zero;
        int currentChannel = -1;
        int currentLength = -1;

        void CreateNativeSequencer()
        {
            if (reference == IntPtr.Zero)
                reference = CreateSequencer();
        }

        void DeleteNativeSequencer()
        {
            if (reference != IntPtr.Zero)
                DeleteSequencer(reference);
            reference = IntPtr.Zero;
            currentIndex = -1;
        }

        public override IntPtr Reference()
        {
            return reference;
        }

        void Awake()
        {
            InitNoteRows();
            CreateNativeSequencer();
            ChangeSequencerChannel(reference, channel);
            ChangeSequencerLength(reference, length);

            for (int i = 0; i < allNotes.Length; ++i)
            {
                foreach (Note note in allNotes[i].notes)
                    note.TryCreate();
            }
            AllNotesOff();
        }

        void OnDestroy()
        {
            if (reference != IntPtr.Zero)
            {
                AllNotesOff();
                DeleteNativeSequencer();
            }
        }

        void OnEnable()
        {
            if (reference != IntPtr.Zero)
                EnableSequencer(reference, true);
        }

        void OnDisable()
        {
            if (reference != IntPtr.Zero)
            {
                EnableSequencer(reference, false);
                AllNotesOff();
            }
        }

        public override void AllNotesOff()
        {
            HelmAllNotesOff(channel);
        }

        public override void NoteOn(int note, float velocity = 1.0f)
        {
            HelmNoteOn(channel, note, velocity);
        }

        public override void NoteOff(int note)
        {
            HelmNoteOff(channel, note);
        }

        void EnableComponent()
        {
            enabled = true;
        }

        public override void StartSequencerScheduled(double dspTime)
        {
            syncTime = dspTime;
            const float lookaheadTime = 0.5f;
            SyncSequencerStart(reference, dspTime);
            float waitToEnable = (float)(dspTime - AudioSettings.dspTime - lookaheadTime);
            Invoke("EnableComponent", waitToEnable);
        }

        void Update()
        {
            UpdatePosition();

            if (length != currentLength)
            {
                if (reference != IntPtr.Zero)
                {
                    HelmAllNotesOff(currentChannel);
                    ChangeSequencerLength(reference, length);
                }
                currentLength = length;
            }
            if (channel != currentChannel)
            {
                if (reference != IntPtr.Zero)
                {
                    HelmAllNotesOff(currentChannel);
                    ChangeSequencerChannel(reference, channel);
                }
                currentChannel = channel;
            }
        }
    }
}
