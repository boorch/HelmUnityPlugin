using UnityEngine;
using System.Collections;

namespace Tytel {
    public class DestroyAfterTime : MonoBehaviour {

        public float time = 10.0f;

        void Start() {
            Invoke("Die", time);
        }

        void Die() {
            Destroy(gameObject);
        }
    }
}
