using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Tytel {
    public class GroundKeyboard : MonoBehaviour {
        public GroundKey keyModel;
        public HelmController synth;
        public Vector3 keyOffset;
        public float verticalOffset = 0.02f;
        public int startingKey = 50;

        const int maxKey = 100;
        const int minKey = 10;

        GroundKey[] keys = new GroundKey[maxKey - minKey];
        HashSet<int> currentKeys = new HashSet<int>();
        HashSet<int> newKeys = new HashSet<int>();

        GroundKey CreateKey(int key) {
            GroundKey groundKey = Instantiate(keyModel, transform) as GroundKey;
            Vector3 position = (key - startingKey) * keyOffset;
            position.y = transform.position.y + verticalOffset;
            groundKey.transform.position = position;
            return groundKey;
        }

        void Start() {
            for (int i = minKey; i < maxKey; ++i)
                keys[i - minKey] = CreateKey(i);
        }

        void TryNoteOn(int key, Vector3 contactPoint) {
            int index = key - minKey;
            if (index >= 0 && index < keys.Length && keys[index].IsInside(contactPoint)) {
                if (!keys[index].IsOn()) {
                    if (synth)
                        synth.NoteOn(key);
                    keys[index].SetOn(true);
                }
                newKeys.Add(key);
            }
        }

        void TryNoteOff(int key) {
            int index = key - minKey;
            if (keys[index].IsOn()) {
                if (synth)
                    synth.NoteOff(key);
                keys[index].SetOn(false);
            }
        }

        void Impulse(Collision collision, float magnification) {
            foreach (ContactPoint contact in collision.contacts) {
                float dot = Vector3.Dot(contact.point, keyOffset);
                int closestKey = (int)Mathf.Round(dot / keyOffset.sqrMagnitude);
                TryNoteOn(startingKey + closestKey, contact.point);
            }
        }

        IEnumerator OnCollisionStay(Collision collision) {
            yield return new WaitForFixedUpdate();
            Impulse(collision, 1.0f);
        }

        IEnumerator OnCollisionEnter(Collision collision) {
            yield return new WaitForFixedUpdate();
            Impulse(collision, 1.0f);
        }

        void FixedUpdate() {
            foreach (int key in currentKeys) {
                if (!newKeys.Contains(key))
                    TryNoteOff(key);
            }

            currentKeys = newKeys;
            newKeys = new HashSet<int>();
        }
    }
}
