// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Helm
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
                if (note_ == value)
                    return;
                int oldNote = note_;
                note_ = value;
                if (FullyNative())
                    ChangeNoteKey(parent.Reference(), reference, note_);
                if (parent)
                    parent.NotifyNoteKeyChanged(this, oldNote);
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
                if (start_ == value)
                    return;
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
                if (end_ == value)
                    return;
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
                if (velocity_ == value)
                    return;
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

        public void CopySettingsToNative()
        {
            if (!HasNativeNote() || !HasNativeSequencer())
                return;

            ChangeNoteEnd(parent.Reference(), reference, end);
            ChangeNoteStart(parent.Reference(), reference, start);
            ChangeNoteKey(parent.Reference(), reference, note);
            ChangeNoteVelocity(reference, velocity);
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
            {
                if (HasNativeNote())
                    CopySettingsToNative();
                else
                    reference = CreateNote(parent.Reference(), note, velocity, start, end);
            }
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

        public bool OverlapsRange(float rangeStart, float rangeEnd)
        {
            return !(start < rangeStart && end <= rangeStart) &&
                   !(start >= rangeEnd && end > rangeEnd);
        }

        public bool InsideRange(float rangeStart, float rangeEnd)
        {
            return start >= rangeStart && end <= rangeEnd;
        }

        public void RemoveRange(float rangeStart, float rangeEnd)
        {
            if (!OverlapsRange(rangeStart, rangeEnd))
                return;

            if (start > rangeStart)
                start = rangeEnd;
            else
                end = rangeStart;
        }
    }
}
