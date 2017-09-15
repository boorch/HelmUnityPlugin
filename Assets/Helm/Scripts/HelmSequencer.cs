// Copyright 2017 Matt Tytel

using UnityEngine;
using System;

namespace AudioHelm
{
    /// <summary>
    /// A sequencer of notes over time that will send its note on/off events to
    /// instances of a Helm native synthesizer
    /// </summary>
    [RequireComponent(typeof(HelmAudioInit))]
    [AddComponentMenu("Audio Helm/Helm Sequencer")]
    public class HelmSequencer : Sequencer
    {
        /// <summary>
        /// Specifies which Helm instance(s) to control.
        /// Every Helm instance in any AudioMixerGroup matching this channel number is controlled by this class.
        /// </summary>
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

        /// <summary>
        /// Reference to the native sequencer instance memory (if any).
        /// </summary>
        /// <returns>The reference the native sequencer. IntPtr.Zero if it doesn't exist.</returns>
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
            Native.SyncSequencerStart(reference, 0.0);
            syncTime = AudioSettings.dspTime;
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

        /// <summary>
        /// Triggers note off events for all notes currently on in the referenced Helm instance(s).
        /// </summary>
        public override void AllNotesOff()
        {
            Native.HelmAllNotesOff(channel);
        }

        /// <summary>
        /// Triggers a note on event for the Helm instance(s) this points to.
        /// You must trigger a note off event later for this note by calling NoteOff.
        /// </summary>
        /// <param name="note">The MIDI keyboard note to play. [0, 127]</param>
        /// <param name="velocity">How hard you hit the key. [0.0, 1.0]</param>
        public override void NoteOn(int note, float velocity = 1.0f)
        {
            Native.HelmNoteOn(channel, note, velocity);
        }

        /// <summary>
        /// Triggers a note off event for the Helm instance(s) this points to.
        /// </summary>
        /// <param name="note">The MIDI keyboard note to turn off. [0, 127]</param>
        public override void NoteOff(int note)
        {
            Native.HelmNoteOff(channel, note);
        }

        void EnableComponent()
        {
            enabled = true;
        }

        /// <summary>
        /// Starts the sequencer at a given time in the future.
        /// This is synced to AudioSettings.dspTime.
        /// </summary>
        /// <param name="dspTime">The time to start the sequencer, synced to AudioSettings.dspTime.</param>
        public override void StartScheduled(double dspTime)
        {
            if (reference != IntPtr.Zero)
            {
                syncTime = dspTime;
                const float lookaheadTime = 0.5f;
                double waitTime = dspTime - AudioSettings.dspTime;
                Native.SyncSequencerStart(reference, waitTime);
                float waitToEnable = (float)(waitTime - lookaheadTime);
                Invoke("EnableComponent", waitToEnable);
            }
        }

        /// <summary>
        /// Starts the sequencer on the start next cycle.
        /// This is useful if you have multiple synced sequencers and you want to start this one on the next go around.
        /// </summary>
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
