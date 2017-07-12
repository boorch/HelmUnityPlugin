// Copyright 2017 Matt Tytel

using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

namespace Helm
{
    [RequireComponent(typeof(HelmAudioInit))]
    public class HelmController : MonoBehaviour, NoteHandler
    {
        public int channel = 0;

        Dictionary<int, int> pressedNotes = new Dictionary<int, int>();

        void OnDestroy()
        {
            AllNotesOff();
        }

        void Awake()
        {
            AllNotesOff();
        }

        void Start()
        {
            Utils.InitAudioSource(GetComponent<AudioSource>());
        }

        public void SetParameter(Param parameter, float newValue)
        {
            Native.HelmChangeParameter(channel, (int)parameter, newValue);
        }

        public void SetParameter(CommonParam parameter, float newValue)
        {
            Native.HelmChangeParameter(channel, (int)parameter, newValue);
        }

        public void AllNotesOff()
        {
            Native.HelmAllNotesOff(channel);
            pressedNotes.Clear();
        }

        public bool IsNoteOn(int note)
        {
            return pressedNotes.ContainsKey(note);
        }

        public Dictionary<int, int> GetPressedNotes()
        {
            return pressedNotes;
        }

        public void NoteOn(int note, float velocity, float length)
        {
            NoteOn(note, velocity);
            StartCoroutine(WaitNoteOff(note, length));
        }

        IEnumerator WaitNoteOff(int note, float length)
        {
            yield return new WaitForSeconds(length);
            NoteOff(note);
        }

        public void NoteOn(int note, float velocity = 1.0f)
        {
            int number = 0;
            pressedNotes.TryGetValue(note, out number);
            pressedNotes[note] = number + 1;
            Native.HelmNoteOn(channel, note, velocity);
        }

        public void NoteOff(int note)
        {
            int number = 0;
            pressedNotes.TryGetValue(note, out number);
            if (number <= 1)
            {
                pressedNotes.Remove(note);
                Native.HelmNoteOff(channel, note);
            }
            else
                pressedNotes[note] = number - 1;
        }

        public void SetPitchWheel(float wheelValue)
        {
            Native.HelmSetPitchWheel(channel, wheelValue);
        }

        public void SetModWheel(float wheelValue)
        {
            Native.HelmSetModWheel(channel, wheelValue);
        }

        public void SetAftertouch(int note, float aftertouchValue)
        {
            Native.HelmSetAftertouch(channel, note, aftertouchValue);
        }
    }
}
