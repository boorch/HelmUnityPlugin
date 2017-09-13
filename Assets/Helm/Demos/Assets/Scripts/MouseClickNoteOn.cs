﻿// Copyright 2017 Matt Tytel

using UnityEngine;

namespace Helm
{
    [AddComponentMenu("")]
    public class MouseClickNoteOn : MonoBehaviour
    {
        public HelmController controller;
        public int note;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
                controller.NoteOn(note);
            if (Input.GetMouseButtonUp(0))
                controller.NoteOff(note);
        }
    }
}
