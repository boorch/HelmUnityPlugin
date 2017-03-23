using UnityEngine;
using System.Collections;

namespace Helm
{
    public class ToggleParameter : MonoBehaviour
    {
        public HelmController controller;
        public Param param;
        public float onValue = 1.0f;
        public float offValue = 0.0f;

        bool isOn = false;

        void Start()
        {
            controller.SetParameter(param, offValue);
        }

        public void Toggle()
        {
            isOn = !isOn;
            if (isOn)
                controller.SetParameter(param, onValue);
            else
                controller.SetParameter(param, offValue);
        }
    }
}
