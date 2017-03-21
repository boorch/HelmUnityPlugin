using UnityEngine;
using System.Collections.Generic;

namespace Tytel
{
    public class SequenceGenerator : MonoBehaviour
    {
        public HelmSequencer sequencer;
        public int[] scale = new int[] { 0, 2, 4, 7, 9 };

        public int minNote = 24;
        public int octaveSpan = 2;
        public float minDensity = 0.5f;
        public float maxDensity = 1.0f;

        List<float> rhythm;
        List<int> melody;

        void GenerateRhythm()
        {
        }

        void GenerateMelody()
        {
        }

        int GetKeyFromRandomWalk(int note)
        {
            int octave = note / scale.Length;
            int scalePosition = note % scale.Length;
            return minNote + octave * Utils.kNotesPerOctave + scale[scalePosition];
        }

        public void Generate()
        {
            sequencer.Clear();

            int maxNote = scale.Length * octaveSpan;
            int currentNote = Random.Range(0, maxNote);
            Note lastNote = sequencer.AddNote(GetKeyFromRandomWalk(currentNote), 0, 1);

            for (int i = 1; i < sequencer.length; ++i)
            {
                float density = Random.Range(minDensity, maxDensity);

                if (Random.Range(0.0f, 1.0f) < density)
                {
                    currentNote = currentNote + Random.Range(-3, 3);
                    if (currentNote > maxNote)
                        currentNote = 2 * maxNote - currentNote;
                    else if (currentNote < 0)
                        currentNote = Mathf.Abs(currentNote);

                    lastNote = sequencer.AddNote(GetKeyFromRandomWalk(currentNote), i, i + 1);
                }
                else
                    lastNote.end = i + 1;
            }
        }
    }
}
