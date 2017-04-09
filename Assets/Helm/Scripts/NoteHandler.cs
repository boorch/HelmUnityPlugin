// Copyright 2017 Matt Tytel

using UnityEngine;

namespace Helm
{
    public interface NoteHandler
    {
        void AllNotesOff();
        void NoteOn(int note, float velocity = 1.0f);
        void NoteOff(int note);
    }
}
