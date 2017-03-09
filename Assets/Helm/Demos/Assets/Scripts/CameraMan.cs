using UnityEngine;
using System.Collections;

namespace Tytel
{
    public class CameraMan : MonoBehaviour
    {
        public Transform player;

        protected Vector3 diff_;

        void Start()
        {
            diff_ = transform.position - player.position;
        }

        void Update()
        {
            transform.position = player.position + diff_;
        }
    }
}
