// Copyright 2017 Matt Tytel

using UnityEngine;
using UnityEngine.Audio;

namespace Helm
{
    [System.Serializable]
    public class Keyzone
    {
        public AudioClip audioClip;
        public AudioMixerGroup mixer;
        public int rootKey = Utils.kMiddleC;
        public int minKey = 0;
        public int maxKey = Utils.kMidiSize;
        public float minVelocity = 0.0f;
        public float maxVelocity = 1.0f;

        bool ValidForNote(int note)
        {
            return note <= maxKey && note >= minKey;
        }

        bool ValidForNote(int note, float velocity)
        {
            return ValidForNote(note) && velocity >= minVelocity && velocity <= maxVelocity;
        }
    }
}
