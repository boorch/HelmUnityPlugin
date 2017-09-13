// Copyright 2017 Matt Tytel

using UnityEngine;

namespace Helm
{
    [AddComponentMenu("")]
    public class SetSleepThreshold : MonoBehaviour
    {
        public float threshold = 0.0f;

        void Start()
        {
            GetComponent<Rigidbody>().sleepThreshold = threshold;
        }
    }
}
