﻿using UnityEngine;
using UnityEngine.Audio;
using System.Runtime.InteropServices;

public class HelmNoteTrigger : MonoBehaviour {

    [DllImport("AudioPluginHelm")]
    private static extern void HelmNoteOn(int instance, int note);

    [DllImport("AudioPluginHelm")]
    private static extern void HelmNoteOff(int instance, int note);

    [DllImport("AudioPluginHelm")]
    private static extern void HelmAllNotesOff(int instance);

    public int instance = 0;
    public bool noteOn = false;
    public bool noteOff = false;

    public AudioMixer helmSource;
    void Start() {
    }

    void OnDestroy() {
        HelmAllNotesOff(instance);
    }

    void NoteOn() {
        HelmNoteOn(instance, 70);
    }

    void NoteOff() {
        HelmNoteOff(instance, 70);
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
