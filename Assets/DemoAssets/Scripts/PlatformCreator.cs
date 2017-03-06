using UnityEngine;
using System.Collections;

namespace Tytel {
    public class PlatformCreator : MonoBehaviour {

        public Transform platformModel;
        public float minWidth = 0.1f;

        Transform currentPlatform;
        Vector2 startPosition;

        void Start() {
        }

        void TryInitialize(Vector2 position) {
            if (Input.GetMouseButtonDown(0)) {
                startPosition = position;
                currentPlatform = Instantiate(platformModel, null);
                currentPlatform.position = startPosition;
            }
        }

        void TryRelease(Vector2 position) {
            if (Input.GetMouseButtonUp(0) && currentPlatform) {
                if ((position - startPosition).sqrMagnitude < minWidth * minWidth)
                    Destroy(currentPlatform.gameObject);
                currentPlatform = null;
            }
        }

        void TryUpdate(Vector2 position) {
            if (currentPlatform == null)
                return;

            Vector3 delta = position - startPosition;
            Vector3 center = (position + startPosition) / 2.0f;
            currentPlatform.position = center;
            currentPlatform.right = delta;

            Vector3 localScale = currentPlatform.localScale;
            localScale.x = delta.magnitude;
            currentPlatform.localScale = localScale;
        }

        void Update() {
            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TryInitialize(position);
            TryUpdate(position);
            TryRelease(position);
        }
    }
}
