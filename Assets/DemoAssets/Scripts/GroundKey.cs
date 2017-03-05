using UnityEngine;
using System.Collections;

namespace Tytel {
    public class GroundKey : MonoBehaviour {
        public Renderer keyLight;
        public Material onMaterial;
        public Material offMaterial;

        bool noteOn = false;

        public bool IsInside(Vector3 position) {
            Vector3 localPosition = transform.InverseTransformPoint(position);
            localPosition.y = 0.0f;
            return Mathf.Abs(localPosition.x) < 0.5f && Mathf.Abs(localPosition.z) < 0.5f;
        }

        public bool IsOn() {
            return noteOn;
        }

        public void SetOn(bool isOn) {
            if (isOn == noteOn)
                return;

            noteOn = isOn;
            if (noteOn)
                keyLight.material = onMaterial;
            else
                keyLight.material = offMaterial;
        }
    }
}
