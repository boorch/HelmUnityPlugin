// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Helm
{
    [RequireComponent(typeof(Sampler))]
    public class SampleSequencer : Sequencer
    {
        double lastWindowTime = -0.01;
        bool waitTillNextCycle = false;

        const float lookaheadTime = 0.12f;

        void Awake()
        {
            InitNoteRows();
            AllNotesOff();
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

        public override void AllNotesOff()
        {
            GetComponent<Sampler>().AllNotesOff();
        }

        public override void NoteOn(int note, float velocity = 1.0f)
        {
            GetComponent<Sampler>().NoteOn(note, velocity);
        }

        public override void NoteOff(int note)
        {
            GetComponent<Sampler>().NoteOff(note);
        }

        public void EnableComponent()
        {
            enabled = true;
        }

        public override void StartScheduled(double dspTime)
        {
            syncTime = dspTime;
            const float lookaheadTime = 0.5f;
            float waitToEnable = (float)(dspTime - AudioSettings.dspTime - lookaheadTime);
            Invoke("EnableComponent", waitToEnable);
        }

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
