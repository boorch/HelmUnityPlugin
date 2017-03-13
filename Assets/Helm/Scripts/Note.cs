// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tytel
{
    [System.Serializable]
    public class Note : ISerializationCallbackReceiver
    {
        [DllImport("AudioPluginHelm")]
        private static extern IntPtr CreateNote(IntPtr sequencer, int note, float velocity, float start, float end);

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr DeleteNote(IntPtr sequencer, IntPtr note);

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr ChangeNoteStart(IntPtr sequencer, IntPtr note, float start);

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr ChangeNoteEnd(IntPtr sequencer, IntPtr note, float end);

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr ChangeNoteKey(IntPtr sequencer, IntPtr note, int key);

        [DllImport("AudioPluginHelm")]
        private static extern IntPtr ChangeNoteVelocity(IntPtr note, float velocity);

        [SerializeField]
        private int note_;
        public int note
        {
            get
            {
                return note_;
            }
            set
            {
                note_ = value;
                if (FullyNative())
                    ChangeNoteKey(parent.Reference(), reference, note_);
            }
        }

        [SerializeField]
        private float start_;
        public float start
        {
            get
            {
                return start_;
            }
            set
            {
                start_ = value;
                if (FullyNative())
                    ChangeNoteStart(parent.Reference(), reference, start_);
            }
        }

        [SerializeField]
        private float end_;
        public float end
        {
            get
            {
                return end_;
            }
            set
            {
                end_ = value;
                if (FullyNative())
                    ChangeNoteEnd(parent.Reference(), reference, end_);
            }
        }

        [SerializeField]
        private float velocity_;
        public float velocity
        {
            get
            {
                return velocity_;
            }
            set
            {
                velocity_ = value;
                if (FullyNative())
                    ChangeNoteVelocity(reference, velocity_);
            }
        }

        public HelmSequencer parent;

        private IntPtr reference;

        public Note()
        {
            reference = IntPtr.Zero;
        }

        ~Note()
        {
            TryDelete();
        }

        bool HasNativeNote()
        {
            return reference != IntPtr.Zero;
        }

        bool HasNativeSequencer()
        {
            return parent != null && parent.Reference() != IntPtr.Zero;
        }

        bool FullyNative()
        {
            return HasNativeNote() && HasNativeSequencer();
        }

        public void TryCreate()
        {
            if (HasNativeSequencer())
                reference = CreateNote(parent.Reference(), note, velocity, start, end);
        }

        public void TryDelete()
        {
            if (FullyNative())
                DeleteNote(parent.Reference(), reference);
            reference = IntPtr.Zero;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            TryCreate();
        }
    }
}
