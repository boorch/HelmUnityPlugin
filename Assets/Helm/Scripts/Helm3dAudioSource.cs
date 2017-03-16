// Copyright 2017 Matt Tytel

using UnityEngine;
using System.Runtime.InteropServices;

namespace Tytel
{
    [RequireComponent(typeof(AudioSource))]
    public class Helm3dAudioSource : MonoBehaviour
    {
        [DllImport("AudioPluginHelm")]
        private static extern void HelmGetBuffer(int channel, float[] buffer, int samples, int channels);

        public int channel = 0;

        bool running = false;

        void Start()
        {
            running = true;
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            if (running)
                HelmGetBuffer(channel, data, data.Length / channels, channels);
        }
    }
}
