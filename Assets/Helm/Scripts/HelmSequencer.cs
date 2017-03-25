// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Helm
{
    public class HelmSequencer : HelmController
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
        private static extern void SyncSequencerStart(IntPtr sequencer, double dspTime);

        [DllImport("AudioPluginHelm")]
        private static extern float GetBpm();

        [System.Serializable]
        public class NoteRow : ISerializationCallbackReceiver
        {
            public List<Note> notes = new List<Note>();
            private List<Note> oldNotes = new List<Note>();

            public void OnBeforeSerialize()
            {
                oldNotes = new List<Note>(notes);
            }

            public void OnAfterDeserialize()
            {
                if (oldNotes.Count == notes.Count)
                    return;

                foreach (Note note in oldNotes)
                    note.TryDelete();
                foreach (Note note in notes)
                    note.TryCreate();
            }
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
        public int currentSixteenth = -1;
        public double syncTime = 0.0;
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
            currentSixteenth = -1;
        }

        public IntPtr Reference()
        {
            return reference;
        }

        void Awake()
        {
            CreateNativeSequencer();
            ChangeSequencerChannel(reference, channel);
            ChangeSequencerLength(reference, length);

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
            AllNotesOff();
        }

        void OnDestroy()
        {
            if (reference != IntPtr.Zero)
            {
                AllNotesOff();
                DeleteNativeSequencer();
            }
        }

        void OnEnable()
        {
            if (reference != IntPtr.Zero)
                EnableSequencer(reference, true);
        }

        void OnDisable()
        {
            if (reference != IntPtr.Zero)
            {
                EnableSequencer(reference, false);
                AllNotesOff();
            }
        }

        public void NotifyNoteKeyChanged(Note note, int oldKey)
        {
            allNotes[oldKey].notes.Remove(note);
            allNotes[note.note].notes.Add(note);
        }

        void RemoveNote(Note note)
        {
            allNotes[note.note].notes.Remove(note);
            note.TryDelete();
        }

        public bool NoteExistsInRange(int note, float start, float end)
        {
            return GetNoteInRange(note, start, end) != null;
        }

        public Note GetNoteInRange(int note, float start, float end, Note ignore = null)
        {
            if (note >= kRows || note < 0 || allNotes == null || allNotes[note] == null)
                return null;
            foreach (Note noteObject in allNotes[note].notes)
            {
                if (noteObject.OverlapsRange(start, end) && noteObject != ignore)
                    return noteObject;
            }
            return null;
        }

        public void RemoveNotesInRange(int note, float start, float end)
        {
            if (allNotes == null || allNotes[note] == null)
                return;

            List<Note> toRemove = new List<Note>();
            foreach (Note noteObject in allNotes[note].notes)
            {
                if (noteObject.OverlapsRange(start, end))
                    toRemove.Add(noteObject);
            }
            foreach (Note noteObject in toRemove)
                RemoveNote(noteObject);
        }

        public void RemoveNotesContainedInRange(int note, float start, float end, Note ignore = null)
        {
            if (allNotes == null || allNotes[note] == null)
                return;

            List<Note> toRemove = new List<Note>();
            foreach (Note noteObject in allNotes[note].notes)
            {
                if (noteObject.InsideRange(start, end) && noteObject != ignore)
                    toRemove.Add(noteObject);
            }
            foreach (Note noteObject in toRemove)
                RemoveNote(noteObject);
        }

        public void ClampNotesInRange(int note, float start, float end, Note ignore = null)
        {
            RemoveNotesContainedInRange(note, start, end, ignore);

            Note noteInRange = GetNoteInRange(note, start, end, ignore);
            while (noteInRange != null)
            {
                noteInRange.RemoveRange(start, end);
                noteInRange = GetNoteInRange(note, start, end, ignore);
            }
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

        void EnableComponent()
        {
            enabled = true;
        }

        public void StartSequencerScheduled(double dspTime)
        {
            syncTime = dspTime;
            const float lookaheadTime = 0.5f;
            SyncSequencerStart(reference, dspTime);
            float waitToEnable = (float)(dspTime - AudioSettings.dspTime - lookaheadTime);
            Invoke("EnableComponent", waitToEnable);
        }

        protected void UpdatePosition()
        {
            double sequencerTime = (Utils.kBpmToSixteenths * GetBpm()) * (AudioSettings.dspTime - syncTime);
            int cycles = (int)(sequencerTime / length);
            double position = sequencerTime - cycles * length;
            currentSixteenth = (int)position;
        }

        void Update()
        {
            UpdatePosition();

            if (length != currentLength)
            {
                if (reference != IntPtr.Zero)
                {
                    AllNotesOff();
                    ChangeSequencerLength(reference, length);
                }
                currentLength = length;
            }
            if (channel != currentChannel)
            {
                if (reference != IntPtr.Zero)
                {
                    AllNotesOff();
                    ChangeSequencerChannel(reference, channel);
                }
                currentChannel = channel;
            }
        }
    }
}
