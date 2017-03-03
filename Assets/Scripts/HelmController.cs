using UnityEngine;
using UnityEngine.Audio;
using System.Runtime.InteropServices;

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

    void NoteOn() {
        HelmNoteOn(channel, 70);
    }

    void NoteOff() {
        HelmNoteOff(channel, 70);
    }

    void Update() {
        if (noteOn) {
            NoteOn();
            noteOn = false;
        }
        if (noteOff) {
            NoteOff();
            noteOff = false;
        }
    }
}
