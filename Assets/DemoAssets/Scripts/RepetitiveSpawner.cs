using UnityEngine;
using System.Collections;

namespace Tytel {
    public class RepetitiveSpawner : MonoBehaviour {
        public Transform model;
        public float rate = 1.0f;

        public void Start() {
            InvokeRepeating("Spawn", 0.0f, rate);
        }

        void Destroy() {
            CancelInvoke("Spawn");
        }

        void Spawn() {
            Transform next = Instantiate(model, transform);
            next.localPosition = Vector3.zero;
        }
    }
}
