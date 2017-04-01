using UnityEngine;
using System.Collections;

namespace Helm
{
    [AddComponentMenu("")]
    public class EnableAfterTime : MonoBehaviour
    {
        public float time = 1.0f;
        public Sequencer sequencer;

        void Start()
        {
            Invoke("Enable", time);
        }

        void Enable()
        {
            sequencer.StartOnNextCycle();
        }
    }
}
