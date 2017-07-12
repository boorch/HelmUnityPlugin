// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Helm
{
    [RequireComponent(typeof(AudioSource))]
    public class Sampler : MonoBehaviour, NoteHandler
    {
        class ActiveNote
        {
            public int note = 0;
            public List<AudioSource> audioSources;
            public double startTime = 0.0;

            public ActiveNote(int n, List<AudioSource> sources, double start)
            {
                note = n;
                audioSources = sources;
                startTime = start;
            }
        }

        public List<Keyzone> keyzones = new List<Keyzone>() { new Keyzone() };
        public float velocityTracking = 1.0f;
        public int numVoices = 2;
        public bool useNoteOff = false;

        int audioIndex = 0;
        List<ActiveNote> activeNotes = new List<ActiveNote>();

        // We end sample early to prevent click at end of sample caused by Unity pitch change.
        const double endEarlyTime = 0.01;

        void Awake()
        {
            AllNotesOff();

            AudioSource[] audios = GetComponents<AudioSource>();
            int voicesToAdd = numVoices - audios.Length;
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

        public Keyzone AddKeyzone()
        {
            Keyzone keyzone = new Keyzone();
            keyzones.Add(keyzone);
            return keyzone;
        }

        public int RemoveKeyzone(Keyzone keyzone)
        {
            int index = keyzones.IndexOf(keyzone);
            keyzones.Remove(keyzone);
            return index;
        }

        AudioSource GetNextAudioSource()
        {
            AudioSource[] audios = GetComponents<AudioSource>();
            foreach (AudioSource audio in audios)
            {
                if (!audio.isPlaying)
                    return audio;
            }
            audioIndex = (audioIndex + 1) % audios.Length;
            return audios[audioIndex];
        }

        void PrepNote(AudioSource audio, int note, float velocity)
        {
            audio.pitch = Utils.MidiChangeToRatio(note - Utils.kMiddleC);
            audio.volume = Mathf.Lerp(1.0f - velocityTracking, 1.0f, velocity);
        }

        void PrepNote(AudioSource audio, Keyzone keyzone, int note, float velocity)
        {
            audio.pitch = Utils.MidiChangeToRatio(note - keyzone.rootKey);
            audio.clip = keyzone.audioClip;
            audio.outputAudioMixerGroup = keyzone.mixer;
            audio.volume = Mathf.Lerp(1.0f - velocityTracking, 1.0f, velocity);
        }

        List<Keyzone> GetValidKeyzones(int note, float velocity = 1.0f)
        {
            List<Keyzone> validKeyzones = new List<Keyzone>();
            foreach (Keyzone keyzone in keyzones)
            {
                if (keyzone.ValidForNote(note, velocity))
                    validKeyzones.Add(keyzone);
            }
            return validKeyzones;
        }

        List<AudioSource> GetPreppedAudioSources(int note, float velocity)
        {
            List<AudioSource> audioSources = new List<AudioSource>();
            List<Keyzone> validKeyzones = GetValidKeyzones(note, velocity);
            foreach (Keyzone keyzone in validKeyzones)
            {
                AudioSource audio = GetNextAudioSource();
                PrepNote(audio, keyzone, note, velocity);
                audioSources.Add(audio);
            }
            return audioSources;
        }

        public int GetMinKey()
        {
            if (keyzones.Count == 0)
                return 0;

            int min = Utils.kMidiSize;
            foreach (Keyzone keyzone in keyzones)
                min = Mathf.Min(keyzone.minKey, min);

            return min;
        }

        public int GetMaxKey()
        {
            if (keyzones.Count == 0)
                return Utils.kMidiSize - 1;

            int max = 0;
            foreach (Keyzone keyzone in keyzones)
                max = Mathf.Max(keyzone.maxKey, max);

            return max;
        }

        public void AllNotesOff()
        {
            AudioSource[] audios = GetComponents<AudioSource>();
            foreach (AudioSource audio in audios)
                audio.Stop();

            activeNotes.Clear();
        }

        public void NoteOn(int note, float velocity = 1.0f)
        {
            List<AudioSource> audioSources = GetPreppedAudioSources(note, velocity);
            activeNotes.Add(new ActiveNote(note, audioSources, AudioSettings.dspTime));
            foreach (AudioSource audio in audioSources)
            {
                if (audio.isActiveAndEnabled)
                {
                    audio.Play();
                    if (!audio.loop)
                    {
                        double length = (audio.clip.length - endEarlyTime) / audio.pitch;
                        audio.SetScheduledEndTime(AudioSettings.dspTime + length);
                    }
                }
            }
        }

        public void NoteOnScheduled(int note, float velocity, double timeToStart, double timeToEnd)
        {
            List<AudioSource> audioSources = GetPreppedAudioSources(note, velocity);
            foreach (AudioSource audio in audioSources)
            {
                double length = timeToEnd - timeToStart;
                if (!useNoteOff)
                    length = Mathf.Infinity;
                if (!audio.loop)
                    length = Math.Min(length, (audio.clip.length - endEarlyTime) / audio.pitch);

                audio.PlayScheduled(AudioSettings.dspTime + timeToStart);
                audio.SetScheduledEndTime(AudioSettings.dspTime + timeToStart + length);
            }
        }

        ActiveNote FindActiveNote(int note)
        {
            foreach (ActiveNote activeNote in activeNotes)
            {
                if (note == activeNote.note)
                    return activeNote;
            }
            return null;
        }

        public void NoteOff(int note)
        {
            if (!useNoteOff)
                return;

            ActiveNote activeNote = FindActiveNote(note);
            if (activeNote == null || AudioSettings.dspTime < activeNote.startTime)
                return;

            activeNotes.Remove(activeNote);
            foreach (AudioSource audio in activeNote.audioSources)
                audio.volume = 0.0f;
        }
    }
}
