// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Helm
{
    [RequireComponent(typeof(AudioSource))]
    public class SampleSequencer : Sequencer
    {
        double lastWindowTime = -0.01;

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
        }

        void OnDisable()
        {
            AllNotesOff();
        }

        public override void AllNotesOff()
        {
        }

        public override void NoteOn(int note, float velocity = 1.0f)
        {
        }

        public override void NoteOff(int note)
        {
        }

        void EnableComponent()
        {
            enabled = true;
        }

        public override void StartSequencerScheduled(double dspTime)
        {
            syncTime = dspTime;
            const float lookaheadTime = 0.5f;
            float waitToEnable = (float)(dspTime - AudioSettings.dspTime - lookaheadTime);
            Invoke("EnableComponent", waitToEnable);
        }

        void Update()
        {
            const float lookaheadTime = 0.05f;
            UpdatePosition();
            AudioSource audio = GetComponent<AudioSource>();

            double position = GetSequencerPosition();
            float sixteenthTime = GetSixteenthTime();
            double currentTime = position * sixteenthTime;

            double windowMax = currentTime + lookaheadTime;

            foreach (NoteRow row in allNotes)
            {
                foreach (Note note in row.notes)
                {
                    float startTime = sixteenthTime * note.start;
                    if (startTime < windowMax && startTime >= lastWindowTime)
                        audio.PlayScheduled(AudioSettings.dspTime + startTime - currentTime);
                }
            }
            lastWindowTime = windowMax;
        }
    }
}
