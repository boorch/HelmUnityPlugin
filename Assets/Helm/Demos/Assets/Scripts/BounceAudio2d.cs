using UnityEngine;
using System.Collections;

namespace Tytel
{
    public class BounceAudio2d : MonoBehaviour
    {
        public HelmController synth;

        public int[] scale = new int[] { 0, 2, 4, 7, 9 };
        public int minNote = 24;
        public float maxSize = 10.0f;
        public float noteLength = 0.1f;
        public float maxSpeed = 1.0f;

        void OnCollisionEnter2D(Collision2D collision)
        {
            float size = transform.localScale.x;
            float octaves = Mathf.Max(0.0f, Mathf.Log(maxSize / size, 2.0f));
            int playOctave = (int)octaves;
            int scaleNote = (int)(scale.Length * (octaves - playOctave));

            float speed = collision.relativeVelocity.magnitude;
            float noteVelocity = Mathf.Clamp(speed / maxSpeed, 0.0f, 1.0f);

            int note = minNote + playOctave * Utils.kNotesPerOctave + scale[scaleNote];
            if (synth)
                synth.NoteOn(note, noteVelocity, noteLength);

            MaterialPulse pulse = GetComponent<MaterialPulse>();
            if (pulse)
                pulse.Pulse(noteVelocity);
        }
    }
}
