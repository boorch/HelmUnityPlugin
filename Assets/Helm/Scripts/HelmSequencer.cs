// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tytel
{
    [RequireComponent(typeof(AudioHeartBeat))]
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
        private static extern void HelmAllNotesOff(int channel);

        [System.Serializable]
        public class NoteRow
        {
            public List<Note> notes = new List<Note>();
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

        public int length = 16;
        public int channel = 0;
        public int currentSixteenth = 0;
        public NoteRow[] allNotes = new NoteRow[Utils.kMidiSize];

        public const int kRows = Utils.kMidiSize;
        public const int kMaxLength = 128;
        IntPtr reference = IntPtr.Zero;
        NoteComparer noteComparer = new NoteComparer();
        int currentChannel = -1;
        int currentLength = -1;

        void CreateNativeSequencer()
        {
            if (reference == IntPtr.Zero)
                reference = CreateSequencer();
        }

        void DeleteNativeSequencer()
        {
            if (reference != IntPtr.Zero)
                DeleteSequencer(reference);
            reference = IntPtr.Zero;
        }

        public IntPtr Reference()
        {
            return reference;
        }

        void Awake()
        {
            if (reference == IntPtr.Zero)
                reference = CreateSequencer();
            for (int i = 0; i < allNotes.Length; ++i)
            {
                if (allNotes[i] == null)
                    allNotes[i] = new NoteRow();
                else
                {
                    foreach (Note note in allNotes[i].notes)
                        note.TryCreate();
                }
            }
        }

        void OnDestroy()
        {
            DeleteNativeSequencer();
        }

        void OnEnable()
        {
            if (reference != IntPtr.Zero)
                EnableSequencer(reference, true);
        }

        void OnDisable()
        {
            if (reference != IntPtr.Zero)
                EnableSequencer(reference, false);
            HelmAllNotesOff(channel);
        }

        void RemoveNote(Note note)
        {
            allNotes[note.note].notes.Remove(note);
            note.TryDelete();
        }

        public static bool IsNoteInRange(Note note, float start, float end)
        {
            return !(note.start < start && note.end <= start) &&
                   !(note.start >= end && note.end > end);
        }

        public bool NoteExistsInRange(int note, float start, float end)
        {
            if (note >= kRows || note < 0 || allNotes == null || allNotes[note] == null)
                return false;
            foreach (Note noteObject in allNotes[note].notes)
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
            foreach (Note noteObject in allNotes[note].notes)
            {
                if (IsNoteInRange(noteObject, start, end))
                    toRemove.Add(noteObject);
            }
            foreach (Note noteObject in toRemove)
                RemoveNote(noteObject);
        }

        public Note AddNote(int note, float start, float end, float velocity = 1.0f)
        {
            Note noteObject = new Note();
            noteObject.note = note;
            noteObject.start = start;
            noteObject.end = end;
            noteObject.velocity = velocity;
            noteObject.parent = this;
            noteObject.TryCreate();

            if (allNotes[note] == null)
                allNotes[note] = new NoteRow();
            allNotes[note].notes.Add(noteObject);
            allNotes[note].notes.Sort(noteComparer);

            return noteObject;
        }

        public void Clear()
        {
            for (int i = 0; i < allNotes.Length; ++i)
            {
                foreach (Note note in allNotes[i].notes)
                    note.TryDelete();

                allNotes[i].notes.Clear();
            }
        }

        void Update()
        {
            float bpm = 120.0f;
            double sequencerTime = Utils.kBpmToSixteenths * bpm * AudioSettings.dspTime;
            float position = Mathf.Repeat((float)sequencerTime, length);
            currentSixteenth = (int)position;

            if (length != currentLength)
            {
                HelmAllNotesOff(currentChannel);
                ChangeSequencerLength(reference, length);
                currentLength = length;
            }
            if (channel != currentChannel)
            {
                HelmAllNotesOff(currentChannel);
                ChangeSequencerChannel(reference, channel);
                currentChannel = channel;
            }
        }
    }
}
