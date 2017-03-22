// Copyright 2017 Matt Tytel

using UnityEngine;
using System.Runtime.InteropServices;

namespace Helm
{
    public class HelmBpm : MonoBehaviour
    {
        [DllImport("AudioPluginHelm")]
        private static extern void SetBpm(float bpm);

        [SerializeField]
        private float bpm_ = 120.0f;
        public float bpm
        {
            get
            {
                return bpm_;
            }
            set
            {
                bpm_ = value;
                SetNative();
            }
        }

        void OnEnable()
        {
            SetNative();
        }

        public void SetNative()
        {
            if (bpm_ > 0.0f)
                SetBpm(bpm_);
        }
    }
}
