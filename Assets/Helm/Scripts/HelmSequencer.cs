using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tytel
{
    public class HelmSequencer : MonoBehaviour
    {
        [DllImport("AudioPluginHelm")]
        private static extern IntPtr CreateSequencer();

        [DllImport("AudioPluginHelm")]
        private static extern void DeleteSequencer(IntPtr sequencer);

        [DllImport("AudioPluginHelm")]
        private static extern void EnableSequencer(IntPtr sequencer, bool enable);

        [DllImport("AudioPluginHelm")]
        private static extern void ChangeSequencerLength(IntPtr sequencer, float length);

        [DllImport("AudioPluginHelm")]
        private static extern bool ChangeSequencerChannel(IntPtr sequencer, int channel);

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr CreateNote(IntPtr sequencer, int note, float velocity, float start, float end);

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr DeleteNote(IntPtr sequencer, IntPtr note);

        [System.Serializable]
        public class Note
        {
            public int note;
            public float start;
            public float end;
            public float velocity;
            public IntPtr noteRef;
        }

        class NoteComparer : IComparer<Note>
        {
            public int Compare(Note left, Note right)
            {
                if (left.start < right.start)
                    return -1;
                else if (left.start > right.start)
                    return 1;
                return 0;
            }
        }

        public int rows = Utils.kMidiSize;
        public int channel = 0;
        public List<Note>[] allNotes = new List<Note>[Utils.kMidiSize];

        IntPtr sequencer = IntPtr.Zero;
        NoteComparer noteComparer = new NoteComparer();

        void CreateNativeSequencer()
        {
            if (sequencer == IntPtr.Zero)
                sequencer = CreateSequencer();
        }

        void DeleteNativeSequencer()
        {
            if (sequencer != IntPtr.Zero)
                DeleteSequencer(sequencer);
            sequencer = IntPtr.Zero;
        }

        void Awake()
        {
            sequencer = CreateSequencer();
            for (int i = 0; i < allNotes.Length; ++i)
            {
                if (allNotes[i] == null)
                    allNotes[i] = new List<Note>();
                else
                {
                    foreach (Note note in allNotes[i])
                        note.noteRef = CreateNote(sequencer, note.note, note.velocity, note.start, note.end);
                }
            }
        }

        void OnDestroy()
        {
            DeleteNativeSequencer();
        }

        void OnEnable()
        {
            if (sequencer != IntPtr.Zero)
                EnableSequencer(sequencer, true);
        }

        void OnDisable()
        {
            if (sequencer != IntPtr.Zero)
                EnableSequencer(sequencer, false);
        }

        void RemoveNote(Note note)
        {
            allNotes[note.note].Remove(note);
            DeleteNote(sequencer, note.noteRef);
            note.noteRef = IntPtr.Zero;
        }

        public static bool IsNoteInRange(Note note, float start, float end)
        {
            return !(note.start < start && note.end <= start) &&
                   !(note.start >= end && note.end > end);
        }

        public bool NoteExistsInRange(int note, float start, float end)
        {
            if (note >= rows || note < 0 || allNotes == null || allNotes[note] == null)
                return false;
            foreach (Note noteObject in allNotes[note])
            {
                if (IsNoteInRange(noteObject, start, end))
                    return true;
            }
            return false;
        }

        public void RemoveNotesInRange(int note, float start, float end)
        {
            if (allNotes == null || allNotes[note] == null)
                return;

            List<Note> toRemove = new List<Note>();
            foreach (Note noteObject in allNotes[note])
            {
                if (IsNoteInRange(noteObject, start, end))
                    toRemove.Add(noteObject);
            }
            foreach (Note noteObject in toRemove)
                RemoveNote(noteObject);
        }

        public void AddNote(int note, float start, float end, float velocity = 1.0f)
        {
            if (sequencer == IntPtr.Zero)
                return;

            Note noteObject = new Note();
            noteObject.note = note;
            noteObject.start = start;
            noteObject.end = end;
            noteObject.velocity = velocity;
            noteObject.noteRef = CreateNote(sequencer, note, velocity, start, end);

            allNotes[note].Add(noteObject);
            allNotes[note].Sort(noteComparer);
        }

        public void Clear()
        {
            for (int i = 0; i < allNotes.Length; ++i)
            {
                foreach (Note note in allNotes[i])
                {
                    DeleteNote(sequencer, note.noteRef);
                    note.noteRef = IntPtr.Zero;
                }

                allNotes[i].Clear();
            }
        }

        void Start()
        {
            /*
            if (sequencer != IntPtr.Zero)
            {
                for (int i = 0; i < 16; ++i)
                    AddNote(24 + i * 3, 0.5f * i, 0.5f * i + 0.5f, 1.0f);
            }
            */
        }
    }
}
