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
        public int numVoices = 2;

        double lastWindowTime = -0.01;
        int audioIndex = 0;

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
            UpdatePosition();
        }

        void FixedUpdate()
        {
            AudioSource[] audios = GetComponents<AudioSource>();

            double position = GetSequencerPosition();
            float sixteenthTime = GetSixteenthTime();
            double currentTime = position * sixteenthTime;
            double sequencerTime = length * sixteenthTime;

            double windowMax = currentTime + lookaheadTime;
            if (windowMax == lastWindowTime)
                return;
            else if (windowMax < lastWindowTime)
                lastWindowTime -= sequencerTime;

            // TODO: performance.
            foreach (NoteRow row in allNotes)
            {
                foreach (Note note in row.notes)
                {
                    float startTime = sixteenthTime * note.start;
                    float endTime = sixteenthTime * note.end;
                    double loopTime = startTime + sequencerTime;
                    if (startTime <= windowMax && startTime > lastWindowTime)
                    {
                        audioIndex = (audioIndex + 1) % audios.Length;
                        audios[audioIndex].PlayScheduled(AudioSettings.dspTime + startTime - currentTime);
                        audios[audioIndex].SetScheduledEndTime(AudioSettings.dspTime + endTime - currentTime);
                        audios[audioIndex].pitch = Utils.MidiChangeToRatio(note.note - Utils.kMiddleC);
                    }
                    else if (loopTime <= windowMax && loopTime > lastWindowTime)
                    {
                        audioIndex = (audioIndex + 1) % audios.Length;
                        audios[audioIndex].PlayScheduled(AudioSettings.dspTime + loopTime - currentTime);
                        audios[audioIndex].SetScheduledEndTime(AudioSettings.dspTime + endTime - currentTime);
                        audios[audioIndex].pitch = Utils.MidiChangeToRatio(note.note - Utils.kMiddleC);
                    }
                }
            }
            lastWindowTime = windowMax;
        }
    }
}
