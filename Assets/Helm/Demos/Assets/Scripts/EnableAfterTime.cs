using UnityEngine;
using System.Collections;

namespace Tytel
{
    public class EnableAfterTime : MonoBehaviour
    {
        public float time = 1.0f;
        public MonoBehaviour behavior;

        void Start()
        {
            Invoke("Enable", time);
        }

        void Enable()
        {
            behavior.enabled = true;
        }
    }
}
