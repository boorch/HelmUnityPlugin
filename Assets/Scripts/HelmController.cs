using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Runtime.InteropServices;

namespace Tytel {

    [RequireComponent(typeof(AudioHeartBeat))]
    public class HelmController : MonoBehaviour {

        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOn(int channel, int note);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOff(int channel, int note);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmAllNotesOff(int channel);

        public int channel = 0;
        public bool noteOn = false;
        public bool noteOff = false;

        void Start() {
        }

        void OnDestroy() {
            HelmAllNotesOff(channel);
        }

        public void NoteOn(int note, float length) {
            NoteOn(note);
            StartCoroutine(WaitNoteOff(note, length));
        }

        IEnumerator WaitNoteOff(int note, float length) {
            yield return new WaitForSeconds(length);
            NoteOff(note);
        }

        public void NoteOn(int note) {
            HelmNoteOn(channel, note);
        }

        public void NoteOff(int note) {
            HelmNoteOff(channel, note);
        }

        void Update() {
            if (noteOn) {
                NoteOn(50);
                noteOn = false;
            }
            if (noteOff) {
                NoteOff(50);
                noteOff = false;
            }
        }
    }
}
