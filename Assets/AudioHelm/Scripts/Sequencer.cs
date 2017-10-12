// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Sanford.Multimedia.Midi;

namespace AudioHelm
{
    /// <summary>
    /// A series of notes and velocities on a timeline that can be used to trigger synth or sampler notes.
    /// </summary>
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

        /// <summary>
        /// Possible divisions of the sequencer UI.
        /// </summary>
        public enum Division
        {
            kEighth,
            kSixteenth,
            kTriplet,
            kThirtySecond,
        }

        /// <summary>
        /// The length of the sequence measured in sixteenth notes.
        /// </summary>
        [Tooltip("The number of sixteenth notes in the sequencer.")]
        public int length = 16;

        /// <summary>
        /// The current sixteenth index.
        /// </summary>
        public int currentIndex = -1;

        /// <summary>
        /// The time to align loops of the sequencer to.
        /// </summary>
        public double syncTime = 0.0;

        /// <summary>
        /// All notes in the seqeuncer.
        /// </summary>
        public NoteRow[] allNotes = new NoteRow[Utils.kMidiSize];

        /// <summary>
        /// The division of the graphical sequencer.
        /// </summary>
        [Tooltip("How often a bar or a division is placed in the sequencer.")]
        public Division division = Division.kSixteenth;

        public const int kMaxLength = 128;

        NoteComparer noteComparer = new NoteComparer();

        public abstract void AllNotesOff();
        public abstract void NoteOn(int note, float velocity = 1.0f);
        public abstract void NoteOff(int note);
        public abstract void StartScheduled(double dspTime);
        public abstract void StartOnNextCycle();

        /// <summary>
        /// Reference to the native sequencer instance memory (if any).
        /// </summary>
        /// <returns>The reference the native sequencer. IntPtr.Zero if it doesn't exist.</returns>
        public virtual IntPtr Reference()
        {
            return IntPtr.Zero;
        }

        /// <summary>
        /// Resets the sequencer at the beginning.
        /// </summary>
        public virtual void Reset()
        {
            AllNotesOff();
            StartScheduled(AudioSettings.dspTime);
        }

        protected void InitNoteRows()
        {
            for (int i = 0; i < allNotes.Length; ++i)
            {
                if (allNotes[i] == null)
                    allNotes[i] = new NoteRow();
            }
        }

        /// <summary>
        /// Gets the length of the division measured in sixteenth notes.
        /// </summary>
        /// <returns>The division length measured in sixteenth notes.</returns>
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

        /// <summary>
        /// Notifies the sequencer of a change to one of the notes.
        /// </summary>
        /// <param name="note">The MIDI note that was changed.</param>
        /// <param name="oldKey">The key the note used to be.</param>
        public void NotifyNoteKeyChanged(Note note, int oldKey)
        {
            allNotes[oldKey].notes.Remove(note);
            allNotes[note.note].notes.Add(note);
        }

        /// <summary>
        /// Removes a note from the sequencer.
        /// </summary>
        /// <param name="note">Note.</param>
        public void RemoveNote(Note note)
        {
            allNotes[note.note].notes.Remove(note);
            note.TryDelete();
        }

        /// <summary>
        /// Check if a note exists within a given range in the sequencer.
        /// </summary>
        /// <returns><c>true</c>, if a note exists in the range, <c>false</c> otherwise.</returns>
        /// <param name="note">The MIDI note to check the range in.</param>
        /// <param name="start">The start of the range measured in sixteenths.</param>
        /// <param name="end">The end of the range measured in sixteenths.</param>
        public bool NoteExistsInRange(int note, float start, float end)
        {
            return GetNoteInRange(note, start, end) != null;
        }

        /// <summary>
        /// Gets the first note in a given range in the sequencer.
        /// </summary>
        /// <returns>The first found note. Returns null if no note was found.</returns>
        /// <param name="note">The MIDI note to look for.</param>
        /// <param name="start">The start of the range measured in sixteenths.</param>
        /// <param name="end">The end of the range measured in sixteenths.</param>
        /// <param name="ignore">A note to ignore if found.</param>
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

        /// <summary>
        /// Removes all notes that overlap a given range.
        /// </summary>
        /// <param name="note">The MIDI note to match.</param>
        /// <param name="start">The start of the range measured in sixteenths.</param>
        /// <param name="end">The end of the range measured in sixteenths.</param>
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

        /// <summary>
        /// Removes all notes that are fully contained in a given range.
        /// </summary>
        /// <param name="note">The MIDI note to match.</param>
        /// <param name="start">The start of the range measured in sixteenths.</param>
        /// <param name="end">The end of the range measured in sixteenths.</param>
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

        /// <summary>
        /// Removes all notes that are fully contained and trim notes that partially overlap range by removing the time inside the range.
        /// </summary>
        /// <param name="note">The MIDI note to match.</param>
        /// <param name="start">The start of the range measured in sixteenths.</param>
        /// <param name="end">The end of the range measured in sixteenths.</param>
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

        /// <summary>
        /// Add a note to the sequencer.
        /// </summary>
        /// <returns>The Note object added to the seqeuncer.</returns>
        /// <param name="note">The MIDI note.</param>
        /// <param name="start">The start of the note measured in sixteenths.</param>
        /// <param name="end">The end of the note measured in sixteenths.</param>
        /// <param name="velocity">The velocity of the note (how hard the key is hit).</param>
        public Note AddNote(int note, float start, float end, float velocity = 1.0f)
        {
            Note noteObject = new Note()
            {
                note = note,
                start = start,
                end = end,
                velocity = velocity,
                parent = this
            };

            noteObject.TryCreate();

            if (allNotes[note] == null)
                allNotes[note] = new NoteRow();
            allNotes[note].notes.Add(noteObject);
            allNotes[note].notes.Sort(noteComparer);

            return noteObject;
        }

        void ReadMidiTrack(Track midiTrack, int sequencerDivision)
        {
            Dictionary<int, float> noteTimes = new Dictionary<int, float>();
            Dictionary<int, float> noteVelocities = new Dictionary<int, float>();
            for (int i = 0; i < midiTrack.Count; ++i)
            {
                MidiEvent midiEvent = midiTrack.GetMidiEvent(i);
                if (midiEvent.MidiMessage.GetBytes().Length < 3)
                    continue;

                byte midiType = (byte)(midiEvent.MidiMessage.GetBytes()[0] & 0xFF);
                byte note = (byte)(midiEvent.MidiMessage.GetBytes()[1] & 0xFF);
                byte velocity = (byte)(midiEvent.MidiMessage.GetBytes()[2] & 0xFF);
                float time = (4.0f * midiEvent.AbsoluteTicks) / sequencerDivision;

                if (midiType == (byte)ChannelCommand.NoteOff ||
                    (midiType == (byte)ChannelCommand.NoteOn) && velocity == 0)
                {
                    if (noteTimes.ContainsKey(note))
                    {
                        AddNote(note, noteTimes[note], time, noteVelocities[note]);
                        noteTimes.Remove(note);
                        noteVelocities.Remove(note);
                    }
                }
                else if (midiType == (byte)ChannelCommand.NoteOn)
                {
                    noteTimes[note] = time;
                    noteVelocities[note] = Mathf.Min(1.0f, velocity / 127.0f);
                }
            }
        }

        // TODO: Get MIDI reading out of Beta.
        /// <summary>
        /// Read a MIDI file's tracks into this sequencer.
        /// Currently in Beta. This may not work for all MIDI files or as expected.
        /// </summary>
        /// <param name="midiStream">The MIDI file stream.</param>
        public void ReadMidiFile(Stream midiStream)
        {
            Clear();
            Sequence midiSequence = new Sequence(midiStream);
            length = 4 * midiSequence.GetLength() / midiSequence.Division;

            foreach (Track midiTrack in midiSequence)
                ReadMidiTrack(midiTrack, midiSequence.Division);
        }

        /// <summary>
        /// Read a MIDI file's tracks into this sequencer.
        /// Currently in Beta. This may not work for all MIDI files or as expected.
        /// </summary>
        /// <param name="midiFile">The MIDI file object.</param>
        public void ReadMidiFile(UnityEngine.Object midiFile)
        {
            TextAsset midiAsText = Resources.Load<TextAsset>("mid_" + midiFile.name);
            if (midiAsText != null)
                ReadMidiFile(new MemoryStream(midiAsText.bytes));
        }

        /// <summary>
        /// Clear the sequencer of all notes.
        /// </summary>
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

        /// <summary>
        /// Gets the time in seconds of a single sixteenth note in the sequencer.
        /// </summary>
        /// <returns>The time in seconds of a sixteenth note.</returns>
        public float GetSixteenthTime()
        {
            return 1.0f / (Utils.kBpmToSixteenths * AudioHelmClock.GetGlobalBpm());
        }

        protected double GetSequencerTime()
        {
            return (Utils.kBpmToSixteenths * AudioHelmClock.GetGlobalBpm()) * (AudioSettings.dspTime - syncTime);
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
