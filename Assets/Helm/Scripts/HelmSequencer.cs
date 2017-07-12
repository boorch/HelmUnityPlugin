// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Helm
{
    [RequireComponent(typeof(HelmAudioInit))]
    public class HelmSequencer : Sequencer
    {
        public int channel = 0;
        IntPtr reference = IntPtr.Zero;
        int currentChannel = -1;
        int currentLength = -1;

        void CreateNativeSequencer()
        {
            if (reference == IntPtr.Zero)
                reference = Native.CreateSequencer();
        }

        void DeleteNativeSequencer()
        {
            if (reference != IntPtr.Zero)
                Native.DeleteSequencer(reference);
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
            Native.ChangeSequencerChannel(reference, channel);
            Native.ChangeSequencerLength(reference, length);

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
                Native.EnableSequencer(reference, true);
        }

        void OnDisable()
        {
            if (reference != IntPtr.Zero)
            {
                Native.EnableSequencer(reference, false);
                AllNotesOff();
            }
        }

        public override void AllNotesOff()
        {
            Native.HelmAllNotesOff(channel);
        }

        public override void NoteOn(int note, float velocity = 1.0f)
        {
            Native.HelmNoteOn(channel, note, velocity);
        }

        public override void NoteOff(int note)
        {
            Native.HelmNoteOff(channel, note);
        }

        void EnableComponent()
        {
            enabled = true;
        }

        public override void StartScheduled(double dspTime)
        {
            if (reference != IntPtr.Zero)
            {
                syncTime = dspTime;
                const float lookaheadTime = 0.5f;
                Native.SyncSequencerStart(reference, dspTime);
                float waitToEnable = (float)(dspTime - AudioSettings.dspTime - lookaheadTime);
                Invoke("EnableComponent", waitToEnable);
            }
        }

        public override void StartOnNextCycle()
        {
            double timeSinceSync = AudioSettings.dspTime - syncTime;
            double sequenceLength = (length * GetSixteenthTime());
            int cyclesSinceSync = (int)(timeSinceSync / sequenceLength);
            StartScheduled(syncTime + (cyclesSinceSync + 1) * sequenceLength);
        }

        void Update()
        {
            UpdatePosition();

            if (length != currentLength)
            {
                if (reference != IntPtr.Zero)
                {
                    Native.HelmAllNotesOff(currentChannel);
                    Native.ChangeSequencerLength(reference, length);
                }
                currentLength = length;
            }
            if (channel != currentChannel)
            {
                if (reference != IntPtr.Zero)
                {
                    Native.HelmAllNotesOff(currentChannel);
                    Native.ChangeSequencerChannel(reference, channel);
                }
                currentChannel = channel;
            }
        }
    }
}
