using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Helm
{
    public class Oscillate : MonoBehaviour
    {
        public Vector3 oscillationVector = Vector3.right;
        public float freqeuncy = 1.0f;

        float progress = 0.0f;
        Vector3 startingPosition = Vector3.zero;

        void Start()
        {
            startingPosition = transform.localPosition;
        }

        void Update()
        {
            progress += Mathf.PI * Time.deltaTime * freqeuncy;
            float mult = Mathf.Sin(progress);
            transform.localPosition = startingPosition + (mult * oscillationVector);
        }
    }
}
