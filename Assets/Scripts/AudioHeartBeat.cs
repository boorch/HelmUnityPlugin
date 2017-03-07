using UnityEngine;
using System.Collections;

namespace Tytel
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioHeartBeat : MonoBehaviour
    {
        void OnAudioFilterRead(float[] data, int channels)
        {
        }
    }
}
