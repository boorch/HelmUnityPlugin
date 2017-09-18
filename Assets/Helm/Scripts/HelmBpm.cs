// Copyright 2017 Matt Tytel

using UnityEngine;

namespace AudioHelm
{
    /// <summary>
    /// Sets the BPM (beats per minute) of all sequencers and native Helm instances.
    /// </summary>
    [AddComponentMenu("Audio Helm/Helm Bpm")]
    [HelpURL("http://tytel.org/audiohelm/scripting/class_helm_1_1_helm_bpm.html")]
    public class HelmBpm : MonoBehaviour
    {
        static float globalBpm = 120.0f;

        [SerializeField]
        float bpm_ = 120.0f;

        /// <summary>
        /// Gets or sets the beats per minute.
        /// </summary>
        /// <value>The new or current bpm.</value>
        public float bpm
        {
            get
            {
                return bpm_;
            }
            set
            {
                bpm_ = value;
                SetGlobalBpm();
            }
        }

        void OnEnable()
        {
            SetGlobalBpm();
        }

        public void SetGlobalBpm()
        {
            if (bpm_ > 0.0f)
            {
                Native.SetBpm(bpm_);
                globalBpm = bpm_;
            }
        }

        public static float GetGlobalBpm()
        {
            return globalBpm;
        }
    }
}
