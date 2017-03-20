using UnityEngine;
using System.Collections;

namespace Tytel
{
    public class SetSleepThreshold : MonoBehaviour
    {
        public float threshold = 0.0f;

        void Start()
        {
            GetComponent<Rigidbody>().sleepThreshold = threshold;
        }
    }
}
