// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Helm
{
    public abstract class Sequencer : MonoBehaviour, NoteHandler
    {
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

        public enum Division
        {
            kEighth,
            kSixteenth,
            kTriplet,
            kThirtySecond,
        }

        public int length = 16;
        public int currentIndex = -1;
        public double syncTime = 0.0;
        public NoteRow[] allNotes = new NoteRow[Utils.kMidiSize];
        public Division division = Division.kSixteenth;

        public const int kMaxLength = 128;

        NoteComparer noteComparer = new NoteComparer();

        public abstract void AllNotesOff();
        public abstract void NoteOn(int note, float velocity = 1.0f);
        public abstract void NoteOff(int note);
        public abstract void StartScheduled(double dspTime);
        public abstract void StartOnNextCycle();

        public virtual IntPtr Reference()
        {
            return IntPtr.Zero;
        }

        protected void InitNoteRows()
        {
            for (int i = 0; i < allNotes.Length; ++i)
            {
                if (allNotes[i] == null)
                    allNotes[i] = new NoteRow();
            }
        }

        public float GetDivisionLength()
        {
            if (division == Division.kEighth)
                return 2.0f;
            if (division == Division.kSixteenth)
                return 1.0f;
            if (division == Division.kTriplet)
                return 4.0f / 3.0f;
            if (division == Division.kThirtySecond)
                return 0.5f;
            return 1.0f;
        }

        public void NotifyNoteKeyChanged(Note note, int oldKey)
        {
            allNotes[oldKey].notes.Remove(note);
            allNotes[note.note].notes.Add(note);
        }

        public void RemoveNote(Note note)
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
            if (note >= Utils.kMidiSize || note < 0 || allNotes == null || allNotes[note] == null)
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
                if (allNotes[i] != null)
                {
                    foreach (Note note in allNotes[i].notes)
                        note.TryDelete();

                    allNotes[i].notes.Clear();
                }
            }
        }

        public float GetSixteenthTime()
        {
            return 1.0f / (Utils.kBpmToSixteenths * HelmBpm.GetGlobalBpm());
        }

        protected double GetSequencerTime()
        {
            return (Utils.kBpmToSixteenths * HelmBpm.GetGlobalBpm()) * (AudioSettings.dspTime - syncTime);
        }

        protected double GetSequencerPosition()
        {
            double sequencerTime = GetSequencerTime();
            int cycles = (int)(sequencerTime / length);
            return sequencerTime - cycles * length;
        }

        protected void UpdatePosition()
        {
            currentIndex = (int)(GetSequencerPosition() / GetDivisionLength());
        }
    }
}
