// Copyright 2017 Matt Tytel

using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Helm
{
    [System.Serializable]
    public class Note : ISerializationCallbackReceiver
    {
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
                    Native.ChangeNoteKey(parent.Reference(), reference, note_);
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
                    Native.ChangeNoteStart(parent.Reference(), reference, start_);
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
                    Native.ChangeNoteEnd(parent.Reference(), reference, end_);
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
                    Native.ChangeNoteVelocity(reference, velocity_);
            }
        }

        public Sequencer parent;

        private IntPtr reference;

        public void OnAfterDeserialize()
        {
            TryCreate();
        }

        public void OnBeforeSerialize()
        {
        }

        void CopySettingsToNative()
        {
            if (!HasNativeNote() || !HasNativeSequencer())
                return;

            Native.ChangeNoteEnd(parent.Reference(), reference, end);
            Native.ChangeNoteStart(parent.Reference(), reference, start);
            Native.ChangeNoteKey(parent.Reference(), reference, note);
            Native.ChangeNoteVelocity(reference, velocity);
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
                    reference = Native.CreateNote(parent.Reference(), note, velocity, start, end);
            }
        }

        public void TryDelete()
        {
            if (FullyNative())
                Native.DeleteNote(parent.Reference(), reference);
            reference = IntPtr.Zero;
        }

        public bool OverlapsRange(float rangeStart, float rangeEnd)
        {
            return Utils.RangesOverlap(start, end, rangeStart, rangeEnd);
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
