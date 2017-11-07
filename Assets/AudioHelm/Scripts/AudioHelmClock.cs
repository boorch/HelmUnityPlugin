// Copyright 2017 Matt Tytel

using UnityEngine;

namespace AudioHelm
{
    /// <summary>
    /// ## [Switch to Manual](../manual/class_audio_helm_1_1_audio_helm_clock.html)<br>
    /// Sets the BPM (beats per minute) of all sequencers and native Helm instances.
    /// </summary>
    [AddComponentMenu("Audio Helm/Audio Helm Clock")]
    [HelpURL("http://tytel.org/audiohelm/manual/class_audio_helm_1_1_audio_helm_clock.html")]
    public class AudioHelmClock : MonoBehaviour
    {
        static float globalBpm = 120.0f;
        static double globalBeatTime = 0.0;
        static double lastSampledTime = 0.0;

        const double waitToSync = 0.5;
        const double SECONDS_PER_MIN = 60.0;

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

        void Awake()
        {
            SetGlobalBpm();
            globalBeatTime = -waitToSync;
            Native.SetBeatTime(globalBeatTime);
            lastSampledTime = AudioSettings.dspTime;
        }

        void SetGlobalBpm()
        {
            if (bpm_ > 0.0f)
            {
                Native.SetBpm(bpm_);
                globalBpm = bpm_;
            }
        }

        /// <summary>
        /// Gets the global beats per minute.
        /// </summary>
        public static float GetGlobalBpm()
        {
            return globalBpm;
        }

        /// <summary>
        /// Gets the global synchronize time.
        /// </summary>
        public static double GetGlobalBeatTime()
        {
            return globalBeatTime;
        }

        /// <summary>
        /// Get the last time the clock was updated.
        /// </summary>
        public static double GetLastSampledTime()
        {
            return lastSampledTime;
        }

        void FixedUpdate()
        {
            double time = AudioSettings.dspTime;
            double deltaTime = time - lastSampledTime;
            lastSampledTime = time;

            globalBeatTime += deltaTime * globalBpm / SECONDS_PER_MIN;
            Native.SetBeatTime(globalBeatTime);
        }
    }
}
