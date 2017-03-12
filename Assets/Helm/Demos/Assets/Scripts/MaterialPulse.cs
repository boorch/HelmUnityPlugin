using UnityEngine;
using System.Collections;

namespace Tytel
{
    public class MaterialPulse : MonoBehaviour
    {
        public Renderer render;
        public Material defaultMaterial;
        public Material pulseMaterial;
        public float decay = 2.0f;

        float progress = 0.0f;

        public void Pulse(float amount)
        {
            progress = Mathf.Max(progress, Mathf.Clamp(amount, 0.0f, 1.0f));
        }

        void Update()
        {
            if (progress == 0.0f)
                return;

            float t = Mathf.Clamp(decay * Time.deltaTime, 0.0f, 1.0f);
            progress = Mathf.Lerp(progress, 0.0f, t);
            render.material.Lerp(defaultMaterial, pulseMaterial, progress);
        }
    }
}
