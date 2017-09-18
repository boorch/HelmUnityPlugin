﻿// Copyright 2017 Matt Tytel

using UnityEngine;

namespace AudioHelm
{
    /// <summary>
    /// A sequencer of notes over time that will send its note on/off events to
    /// a Sampler instance that is attatched to the same object.
    /// </summary>
    [RequireComponent(typeof(Sampler))]
    [AddComponentMenu("Audio Helm/Sample Sequencer")]
    [HelpURL("http://tytel.org/audiohelm/scripting/class_helm_1_1_sample_sequencer.html")]
    public class SampleSequencer : Sequencer
    {
        double lastWindowTime = -0.01;
        bool waitTillNextCycle = false;

        const float lookaheadTime = 0.12f;

        void Awake()
        {
            InitNoteRows();
            AllNotesOff();
            syncTime = AudioSettings.dspTime;
        }

        void OnDestroy()
        {
            AllNotesOff();
        }

        void OnEnable()
        {
            double position = GetSequencerPosition();
            float sixteenthTime = GetSixteenthTime();
            double currentTime = position * sixteenthTime;
            lastWindowTime = currentTime + lookaheadTime;
        }

        void OnDisable()
        {
            AllNotesOff();
            waitTillNextCycle = false;
        }

        /// <summary>
        /// Triggers note off events for all notes currently on in the referenced Sampler.
        /// </summary>
        public override void AllNotesOff()
        {
            GetComponent<Sampler>().AllNotesOff();
        }

        /// <summary>
        /// Triggers a note on event for the referenced sampler.
        /// If the AudioSource is set to loop, you must trigger a note off event
        /// later for this note by calling NoteOff.
        /// </summary>
        /// <param name="note">The MIDI keyboard note to play. [0, 127]</param>
        /// <param name="velocity">How hard you hit the key. [0.0, 1.0]</param>
        public override void NoteOn(int note, float velocity = 1.0f)
        {
            GetComponent<Sampler>().NoteOn(note, velocity);
        }

        /// <summary>
        /// Triggers a note off event for the referenced Sampler.
        /// </summary>
        /// <param name="note">The MIDI keyboard note to turn off. [0, 127]</param>
        public override void NoteOff(int note)
        {
            GetComponent<Sampler>().NoteOff(note);
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
            syncTime = dspTime;
            float waitToEnable = (float)(dspTime - AudioSettings.dspTime - lookaheadTime);
            Invoke("EnableComponent", waitToEnable);
        }

        /// <summary>
        /// Starts the sequencer on the start next cycle.
        /// This is useful if you have multiple synced sequencers and you want to start this one on the next go around.
        /// </summary>
        public override void StartOnNextCycle()
        {
            enabled = true;
            waitTillNextCycle = true;
        }

        void Update()
        {
            UpdatePosition();
        }

        void FixedUpdate()
        {
            double position = GetSequencerPosition();
            float sixteenthTime = GetSixteenthTime();
            double currentTime = position * sixteenthTime;
            double sequencerTime = length * sixteenthTime;

            double windowMax = currentTime + lookaheadTime;
            if (windowMax == lastWindowTime)
                return;

            if (windowMax < lastWindowTime)
            {
                waitTillNextCycle = false;
                lastWindowTime -= sequencerTime;
            }

            if (waitTillNextCycle)
            {
                lastWindowTime = windowMax;
                return;
            }

            // TODO: search performance.
            foreach (NoteRow row in allNotes)
            {
                foreach (Note note in row.notes)
                {
                    double startTime = sixteenthTime * note.start;
                    double endTime = sixteenthTime * note.end;
                    if (startTime < lastWindowTime)
                        startTime += sequencerTime;
                    if (startTime < windowMax && startTime >= lastWindowTime)
                    {
                        endTime = startTime + sixteenthTime * (note.end - note.start);
                        double timeToStart = startTime - currentTime;
                        double timeToEnd = endTime - currentTime;
                        GetComponent<Sampler>().NoteOnScheduled(note.note, note.velocity, timeToStart, timeToEnd);
                    }
                }
            }
            lastWindowTime = windowMax;
        }
    }
}
