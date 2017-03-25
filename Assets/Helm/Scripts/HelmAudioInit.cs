// Copyright 2017 Matt Tytel

using UnityEngine;

namespace Helm
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(AudioSource))]
    public class HelmAudioInit : MonoBehaviour
    {
        void Awake()
        {
            Utils.InitAudioSource(GetComponent<AudioSource>());
        }

        void Update()
        {
            AudioSource audio = GetComponent<AudioSource>();
            audio.pitch = 1.0f;
        }
    }
}
