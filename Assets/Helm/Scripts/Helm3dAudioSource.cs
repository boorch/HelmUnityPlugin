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
            AudioSource audio = GetComponent<AudioSource>();
            AudioClip one = AudioClip.Create("one", 1, 1, AudioSettings.outputSampleRate, false);

            one.SetData(new float[] { 1 }, 0);
            audio.clip = one;
            audio.loop = true;
            audio.spatialBlend = 1.0f;
            audio.Play();
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            /*
            if (running)
                HelmGetBuffer(channel, data, data.Length / channels, channels);
            */
            for (int i = 0; i < data.Length; ++i)
                data[i] = data[i] * ((1.0f * i) / data.Length - 0.5f);
        }
    }
}
