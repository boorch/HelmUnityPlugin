// Copyright 2017 Matt Tytel

using UnityEngine;

namespace Helm
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(AudioSource))]
    public class HelmAudioInit : MonoBehaviour
    {
        bool warnedNoAudioGroup = false;

        void Awake()
        {
            Utils.InitAudioSource(GetComponent<AudioSource>());

            if (!Application.runInBackground) {
                Debug.LogWarning("Setting application to run in background to keep audio in sync!");
                Application.runInBackground = true;
            }
        }

        void Update()
        {
            AudioSource audio = GetComponent<AudioSource>();

            if (Application.isPlaying && audio.clip == null)
            {
                if (!warnedNoAudioGroup)
                {
                    Debug.LogWarning("AudioSource output needs an AudioMixerGroup with a Helm Instance.");
                    warnedNoAudioGroup = true;
                }
            }
            else
                warnedNoAudioGroup = false;

            audio.pitch = 1.0f;
        }
    }
}
