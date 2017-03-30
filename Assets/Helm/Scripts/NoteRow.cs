// Copyright 2017 Matt Tytel

using UnityEngine;
using System.Collections.Generic;

namespace Helm
{
    [System.Serializable]
    public class NoteRow : ScriptableObject
    {
        public List<Note> notes = new List<Note>();
    }
}
