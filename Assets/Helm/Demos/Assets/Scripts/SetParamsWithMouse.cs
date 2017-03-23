using UnityEngine;
using System.Collections;

namespace Helm
{
    public class SetParamsWithMouse : MonoBehaviour
    {
        public Param xParameter;
        public float xMin = 0.0f;
        public float xMax = 1.0f;
        public CommonParam yParameter;
        public float yMin = 0.0f;
        public float yMax = 1.0f;

        public HelmController controller;

        void Update()
        {
            Vector2 mouse = Input.mousePosition;

            float xPosition = Mathf.Clamp(mouse.x / Screen.width, 0.0f, 1.0f);
            float xValue = Mathf.Lerp(xMin, xMax, xPosition);
            controller.SetParameter(xParameter, xValue);

            float yPosition = Mathf.Clamp(mouse.y / Screen.height, 0.0f, 1.0f);
            float yValue = Mathf.Lerp(yMin, yMax, yPosition);
            controller.SetParameter(yParameter, yValue);
        }
    }
}
