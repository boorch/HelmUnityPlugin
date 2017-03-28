// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Helm
{
    [RequireComponent(typeof(AudioSource))]
    public class Sampler : MonoBehaviour, NoteHandler
    {
        public float velocityTracking = 1.0f;
        public int numVoices = 2;

        int audioIndex = 0;

        // We end sample early to prevent click at end of sample caused by Unity pitch change.
        const double endEarlyTime = 0.01;

        void Awake()
        {
            AllNotesOff();

            AudioSource[] audios = GetComponents<AudioSource>();
            int voicesToAdd = audios.Length;
            int originalIndex = 0;
            for (int i = 0; i < voicesToAdd; ++i)
            {
                Utils.CopyComponent(audios[originalIndex], gameObject);
                originalIndex = (originalIndex + 1) % audios.Length;
            }
        }

        void OnDestroy()
        {
            AllNotesOff();
        }

        void OnDisable()
        {
            AllNotesOff();
        }

        AudioSource GetNextAudioSource()
        {
            AudioSource[] audios = GetComponents<AudioSource>();
            audioIndex = (audioIndex + 1) % audios.Length;
            return audios[audioIndex];
        }

        void PrepNote(AudioSource audio, int note, float velocity)
        {
            audio.pitch = Utils.MidiChangeToRatio(note - Utils.kMiddleC);
            audio.volume = Mathf.Lerp(1.0f - velocityTracking, 1.0f, velocity);
        }

        public void AllNotesOff()
        {
            AudioSource[] audios = GetComponents<AudioSource>();
            foreach (AudioSource audio in audios)
                audio.Stop();
        }

        public void NoteOn(int note, float velocity = 1.0f)
        {
            AudioSource audio = GetNextAudioSource();
            PrepNote(audio, note, velocity);
            audio.Play();
            if (!audio.loop)
            {
                double length = (audio.clip.length - endEarlyTime) / audio.pitch;
                audio.SetScheduledEndTime(AudioSettings.dspTime + length);
            }
        }

        public void NoteOnScheduled(int note, float velocity, double timeToStart, double timeToEnd)
        {
            AudioSource audio = GetNextAudioSource();
            PrepNote(audio, note, velocity);

            double length = timeToEnd - timeToStart;
            if (!audio.loop)
                length = Math.Min(length, (audio.clip.length - endEarlyTime) / audio.pitch);

            audio.PlayScheduled(AudioSettings.dspTime + timeToStart);
            audio.SetScheduledEndTime(AudioSettings.dspTime + timeToStart + length);
        }

        public void NoteOff(int note)
        {
            float pitch = Utils.MidiChangeToRatio(note - Utils.kMiddleC);
            AudioSource[] audios = GetComponents<AudioSource>();
            foreach (AudioSource audio in audios)
            {
                if (audio.pitch == pitch)
                    audio.Stop();
            }
        }
    }
}
