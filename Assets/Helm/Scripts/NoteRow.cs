// Copyright 2017 Matt Tytel

using UnityEngine;
using System.Collections.Generic;

namespace Helm
{
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
}
