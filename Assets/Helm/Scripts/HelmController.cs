using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tytel
{
    [RequireComponent(typeof(AudioHeartBeat))]
    public class HelmController : MonoBehaviour
    {
        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOn(int channel, int note, float velocity);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOff(int channel, int note);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmAllNotesOff(int channel);

        public int channel = 0;

        HashSet<int> pressedNotes = new HashSet<int>();

        void OnDestroy()
        {
            HelmAllNotesOff(channel);
        }

        public bool IsNoteOn(int note)
        {
            return pressedNotes.Contains(note);
        }

        public HashSet<int> GetPressedNotes()
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
            pressedNotes.Add(note);
            HelmNoteOn(channel, note, velocity);
        }

        public void NoteOff(int note)
        {
            pressedNotes.Remove(note);
            HelmNoteOff(channel, note);
        }
    }
}
