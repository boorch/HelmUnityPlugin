// Copyright 2017 Matt Tytel

using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Helm
{
    [RequireComponent(typeof(HelmAudioInit))]
    public class HelmController : MonoBehaviour, NoteHandler
    {
        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOn(int channel, int note, float velocity);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOff(int channel, int note);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmAllNotesOff(int channel);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmSetPitchWheel(int channel, float value);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmSetModWheel(int channel, float value);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmSetAftertouch(int channel, int note, float value);

        [DllImport("AudioPluginHelm")]
        private static extern bool HelmChangeParameter(int channel, int paramIndex, float newValue);

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
            HelmChangeParameter(channel, (int)parameter, newValue);
        }

        public void SetParameter(CommonParam parameter, float newValue)
        {
            HelmChangeParameter(channel, (int)parameter, newValue);
        }

        public void AllNotesOff()
        {
            HelmAllNotesOff(channel);
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
            HelmNoteOn(channel, note, velocity);
        }

        public void NoteOff(int note)
        {
            int number = 0;
            pressedNotes.TryGetValue(note, out number);
            if (number <= 1)
            {
                pressedNotes.Remove(note);
                HelmNoteOff(channel, note);
            }
            else
                pressedNotes[note] = number - 1;
        }

        public void SetPitchWheel(float wheelValue)
        {
            HelmSetPitchWheel(channel, wheelValue);
        }

        public void SetModWheel(float wheelValue)
        {
            HelmSetModWheel(channel, wheelValue);
        }

        public void SetAftertouch(int note, float aftertouchValue)
        {
            HelmSetAftertouch(channel, note, aftertouchValue);
        }
    }
}
