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
    }
}
