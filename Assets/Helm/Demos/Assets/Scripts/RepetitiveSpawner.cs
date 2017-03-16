using UnityEngine;
using System.Collections;

namespace Tytel
{
    public class RepetitiveSpawner : MonoBehaviour
    {
        public Transform model;
        public float rate = 1.0f;

        float progress = 0.0f;

        void Spawn()
        {
            Transform next = Instantiate(model, transform);
            next.localPosition = Vector3.zero;
        }

        void Update()
        {
            progress += rate * Time.deltaTime;
            if (progress >= 1.0f)
            {
                Spawn();
                progress -= 1.0f;
            }
        }
    }
}
