using UnityEngine;
using System.Collections;

namespace Tytel {
    public class ControlJump : MonoBehaviour {

        public float jumpForce = 1.0f;
        public KeyCode key = KeyCode.Space;
        Vector3 surface_normal_ = Vector3.zero;

        void Start () {
        }

        void Update () {
            if (Input.GetKeyDown(key)) {
                GetComponent<Rigidbody>().AddForce(surface_normal_ * jumpForce);
                surface_normal_ = Vector3.zero;
            }
        }

        void OnCollisionStay(Collision collision) {
            foreach (ContactPoint contact in collision.contacts)
                surface_normal_ = contact.normal;
        }
    }
}
