// Copyright 2017 Matt Tytel

using UnityEngine;

namespace Tytel
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioHeartBeat : MonoBehaviour
    {
        void OnAudioFilterRead(float[] data, int channels)
        {
        }
    }
}
