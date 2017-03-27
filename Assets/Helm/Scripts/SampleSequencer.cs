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
        public float velocityTracking = 1.0f;

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
            AudioSource[] audios = GetComponents<AudioSource>();
            foreach (AudioSource audio in audios)
                audio.Stop();
        }

        public override void NoteOn(int note, float velocity = 1.0f)
        {
            AudioSource[] audios = GetComponents<AudioSource>();
            audioIndex = (audioIndex + 1) % audios.Length;
            AudioSource audio = audios[audioIndex];

            audio.pitch = Utils.MidiChangeToRatio(note - Utils.kMiddleC);
            audio.volume = Mathf.Lerp(1.0f - velocityTracking, 1.0f, velocity);
            audio.Play();
        }

        public override void NoteOff(int note)
        {
            float pitch = Utils.MidiChangeToRatio(note - Utils.kMiddleC);
            AudioSource[] audios = GetComponents<AudioSource>();
            foreach (AudioSource audio in audios)
            {
                if (audio.pitch == pitch)
                    audio.Stop();
            }
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
                    double startTime = sixteenthTime * note.start;
                    double endTime = sixteenthTime * note.end;
                    if (startTime < lastWindowTime)
                    {
                        startTime += sequencerTime;
                        endTime += sequencerTime;
                    }
                    if (startTime < windowMax && startTime >= lastWindowTime)
                    {
                        audioIndex = (audioIndex + 1) % audios.Length;
                        AudioSource audio = audios[audioIndex];

                        audio.pitch = Utils.MidiChangeToRatio(note.note - Utils.kMiddleC);
                        audio.volume = Mathf.Lerp(1.0f - velocityTracking, 1.0f, note.velocity);

                        audio.PlayScheduled(AudioSettings.dspTime + startTime - currentTime);
                        audio.SetScheduledEndTime(AudioSettings.dspTime + endTime - currentTime);
                    }
                }
            }
            lastWindowTime = windowMax;
        }
    }
}
