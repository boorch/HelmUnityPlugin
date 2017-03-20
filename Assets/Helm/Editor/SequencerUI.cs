// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Tytel
{
    public class SequencerUI
    {
        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOn(int channel, int note, float velocity);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOff(int channel, int note);

        enum Mode
        {
            kWaiting,
            kAdding,
            kDeleting,
            kKeyboarding,
            kDragging,
            kResizingStart,
            kResizingEnd,
            kNumModes
        }

        public SequencerUI(float keyboard, float scroll)
        {
            keyboardWidth = keyboard;
            rightPadding = scroll;
        }

        const float grabResizeWidth = 5.0f;
        const float minNoteTime = 0.15f;
        const float defaultVelocity = 0.8f;
        const float dragDeltaStartRounding = 0.8f;

        float keyboardWidth = 20.0f;
        float rightPadding = 15.0f;

        Mode mode = Mode.kWaiting;
        Note activeNote = null;
        bool mouseActive = false;
        bool roundingToSixteenth = false;
        float clickNoteStartOffset = 0.0f;

        int pressNote = 0;
        float pressTime = 0.0f;
        float dragTime = 0.0f;
        int pressedKey = -1;

        Color emptyCellBlack = new Color(0.5f, 0.5f, 0.5f);
        Color emptyCellWhite = new Color(0.6f, 0.6f, 0.6f);
        Color noteDivision = new Color(0.4f, 0.4f, 0.4f);
        Color beatDivision = new Color(0.2f, 0.2f, 0.2f);
        Color fullCellFullVelocity = Color.red;
        Color fullCellZeroVelocity = new Color(1.0f, 0.8f, 0.8f);
        Color pressedCell = new Color(0.6f, 1.0f, 1.0f);
        Color deletingCell = new Color(0.7f, 1.0f, 0.7f);
        Color lightenColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        Color blackKey = Color.black;
        Color whiteKey = Color.white;
        Color blackKeyPressed = Color.red;
        Color whiteKeyPressed = Color.red;

        float rowHeight = 10.0f;
        int numRows = 128;
        int numCols = 16;
        int notesPerBeat = 4;
        float lastHeight = 0;
        float colWidth = 30.0f;

        Vector2 scrollPosition;
        Vector2 mouseBump = new Vector2(-1.0f, -3.0f);

        Vector2 GetSequencerPosition(Rect rect, Vector2 mousePosition)
        {
            if (!rect.Contains(mousePosition))
                return -Vector2.one;

            Vector2 localPosition = mousePosition - rect.position + scrollPosition + mouseBump;
            float note = numRows - Mathf.Floor((localPosition.y / rowHeight)) - 1;
            float time = (localPosition.x - keyboardWidth) / colWidth;
            return new Vector2(time, note);
        }

        public bool MouseActive()
        {
            return mouseActive;
        }

        void MouseDown(int note, float time, HelmSequencer sequencer, bool edit)
        {
            roundingToSixteenth = false;
            mouseActive = true;
            activeNote = sequencer.GetNoteInRange(note, time, time);
            dragTime = time;
            if (pressedKey >= 0)
            {
                HelmNoteOff(sequencer.channel, pressedKey);
                pressedKey = -1;
            }
            if (time < 0.0f)
            {
                mode = Mode.kKeyboarding;
                pressedKey = note;
                HelmNoteOn(sequencer.channel, pressedKey, 1.0f);
                return;
            }
            else if (activeNote != null)
            {
                float startPixels = colWidth * (time - activeNote.start);
                float endPixels = colWidth * (activeNote.end - time);
                if (edit)
                {
                    Undo.RegisterCompleteObjectUndo(sequencer, "Drag Note");
                    mode = Mode.kDragging;
                    clickNoteStartOffset = time - activeNote.start;
                }
                else if (endPixels <= grabResizeWidth)
                {
                    Undo.RecordObject(sequencer, "Resize Note End");
                    mode = Mode.kResizingEnd;
                    activeNote.end = Mathf.Max(activeNote.start + minNoteTime, dragTime);
                }
                else if (startPixels <= grabResizeWidth)
                {
                    Undo.RecordObject(sequencer, "Resize Note Start");
                    mode = Mode.kResizingStart;
                    activeNote.start = Mathf.Min(activeNote.end - minNoteTime, dragTime);
                }
                else
                    mode = Mode.kDeleting;
            }
            else
                mode = Mode.kAdding;

            pressNote = note;
            pressTime = time;
            dragTime = time;
        }

        void MouseDrag(int note, float time, HelmSequencer sequencer)
        {
            float clampedTime = Mathf.Clamp(time, 0.0f, sequencer.length);
            dragTime = clampedTime;

            if (Mathf.Abs(dragTime - pressTime) >= dragDeltaStartRounding)
                roundingToSixteenth = true;

            if (mode == Mode.kKeyboarding)
            {
                if (note != pressedKey)
                {
                    HelmNoteOff(sequencer.channel, pressedKey);
                    HelmNoteOn(sequencer.channel, note, 1.0f);
                    pressedKey = note;
                }
            }
            else if (mode == Mode.kDragging)
            {
                if (activeNote != null)
                {
                    float newStart = dragTime - clickNoteStartOffset;
                    float length = activeNote.end - activeNote.start;
                    if (newStart + length > sequencer.length)
                        newStart = sequencer.length - length;
                    if (newStart < 0.0f)
                        newStart = 0.0f;

                    if (roundingToSixteenth)
                        newStart = Mathf.Round(newStart);
                    activeNote.start = newStart;
                    activeNote.end = newStart + length;
                    activeNote.note = note;
                }
            }
            else if (mode == Mode.kResizingStart)
            {
                if (activeNote != null)
                {
                    float startTime = dragTime;
                    if (roundingToSixteenth)
                        startTime = Mathf.Round(dragTime);
                    activeNote.start = Mathf.Min(activeNote.end - minNoteTime, startTime);
                }
            }
            else if (mode == Mode.kResizingEnd)
            {
                if (activeNote != null)
                {
                    float endTime = dragTime;
                    if (roundingToSixteenth)
                        endTime = Mathf.Round(dragTime);
                    activeNote.end = Mathf.Max(activeNote.start + minNoteTime, endTime);
                }
            }
        }

        void MouseUp(float time, HelmSequencer sequencer)
        {
            mouseActive = false;
            if (mode == Mode.kKeyboarding)
            {
                HelmNoteOff(sequencer.channel, pressedKey);
                pressedKey = -1;
                return;
            }

            dragTime = Mathf.Clamp(time, 0.0f, sequencer.length);
            float startTime = Mathf.Min(pressTime, dragTime);
            float endTime = Mathf.Max(pressTime, dragTime);

            if (mode == Mode.kDragging)
            {
                if (activeNote != null)
                    sequencer.ClampNotesInRange(activeNote.note, activeNote.start, activeNote.end, activeNote);
            }
            else if (mode == Mode.kResizingStart)
            {
                Undo.RecordObject(sequencer, "Resize Note Start");

                if (activeNote != null)
                    sequencer.ClampNotesInRange(pressNote, activeNote.start, activeNote.end, activeNote);
            }
            else if (mode == Mode.kResizingEnd)
            {
                Undo.RecordObject(sequencer, "Resize Note End");

                if (activeNote != null)
                    sequencer.ClampNotesInRange(pressNote, activeNote.start, activeNote.end, activeNote);
            }
            else if (mode == Mode.kAdding)
            {
                Undo.RecordObject(sequencer, "Add Sequencer Notes");
                int startDrag = Mathf.FloorToInt(startTime);
                int endDrag = Mathf.CeilToInt(endTime);

                sequencer.ClampNotesInRange(pressNote, startDrag, endDrag);

                for (int i = startDrag; i < endDrag; ++i)
                    sequencer.AddNote(pressNote, i, i + 1, defaultVelocity);
            }
            else if (mode == Mode.kDeleting)
            {
                Undo.RecordObject(sequencer, "Delete Sequencer Notes");
                sequencer.RemoveNotesInRange(pressNote, startTime, endTime);
            }
            mode = Mode.kWaiting;
        }

        public bool DoSequencerEvents(Rect rect, HelmSequencer sequencer)
        {
            Event evt = Event.current;
            Vector2 sequencerPosition = GetSequencerPosition(rect, evt.mousePosition);
            EventModifiers modifiers = (EventModifiers.Shift | EventModifiers.Control |
                                        EventModifiers.Alt | EventModifiers.Command);
            bool modifier = (evt.modifiers & modifiers) != EventModifiers.None;
            bool edit = evt.button > 0 || modifier;
            float time = sequencerPosition.x;

            if (evt.type == EventType.MouseUp && mouseActive)
            {
                MouseUp(time, sequencer);
                return true;
            }

            int note = (int)sequencerPosition.y;
            if (note >= numRows || note < 0)
                return false;

            if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
                MouseDown(note, time, sequencer, edit);
            else if (evt.type == EventType.MouseDrag && mouseActive)
                MouseDrag(note, time, sequencer);
            return true;
        }

        void DrawNoteRows(Rect rect)
        {
            float y = 0.0f;
            for (int i = 0; i < numRows; ++i)
            {
                int midiNote = numRows - i - 1;
                Color keyColor = whiteKey;
                Color rowColor = emptyCellWhite;

                if (Utils.IsBlackKey(midiNote))
                {
                    if (pressedKey == midiNote)
                        keyColor = blackKeyPressed;
                    else
                        keyColor = blackKey;
                    rowColor = emptyCellBlack;
                }
                else if (pressedKey == midiNote)
                    keyColor = whiteKeyPressed;

                Rect key = new Rect(0.0f, y, keyboardWidth, rowHeight);
                Rect row = new Rect(keyboardWidth, y, rect.width - keyboardWidth, rowHeight);
                EditorGUI.DrawRect(key, keyColor);
                EditorGUI.DrawRect(row, rowColor);
                y += rowHeight;
            }
        }

        void DrawBarHighlights(Rect rect)
        {
            float x = keyboardWidth;
            int numBars = numCols / notesPerBeat;
            float barWidth = colWidth * notesPerBeat;
            for (int i = 0; i < numBars; ++i)
            {
                if (i % 2 != 0)
                {
                    Rect bar = new Rect(x, 0, barWidth, rect.height);
                    EditorGUI.DrawRect(bar, lightenColor);
                }
                x += barWidth;
            }
        }

        void DrawNoteDivisionLines(Rect rect)
        {
            float x = keyboardWidth;
            for (int i = 0; i <= numCols; ++i)
            {
                Rect line = new Rect(x, 0, 1.0f, rect.height);
                if (i % notesPerBeat == 0)
                    EditorGUI.DrawRect(line, beatDivision);
                else
                    EditorGUI.DrawRect(line, noteDivision);
                x += colWidth;
            }
        }

        void DrawNote(int note, float start, float end, Color color)
        {
            float x = start * colWidth + keyboardWidth;
            float y = (numRows - note - 1) * rowHeight;
            float width = end * colWidth + keyboardWidth - x;
            Rect noteOutsideRect = new Rect(x, y, width + 1, rowHeight);
            Rect noteRect = new Rect(x + 1, y + 1, width - 1, rowHeight - 2);
            EditorGUI.DrawRect(noteOutsideRect, Color.black);
            EditorGUI.DrawRect(noteRect, color);
            Rect leftResizeRect = new Rect(x - mouseBump.x, y - mouseBump.y, grabResizeWidth, rowHeight);
            Rect rightResizeRect = new Rect(noteRect.xMax - grabResizeWidth - mouseBump.x, y - mouseBump.y,
                                            grabResizeWidth, rowHeight);
            EditorGUIUtility.AddCursorRect(leftResizeRect, MouseCursor.SplitResizeLeftRight);
            EditorGUIUtility.AddCursorRect(rightResizeRect, MouseCursor.SplitResizeLeftRight);
        }

        void DrawRowNotes(List<Note> notes)
        {
            if (notes == null)
                return;

            foreach (Note note in notes)
            {
                Color color = Color.Lerp(fullCellZeroVelocity, fullCellFullVelocity, note.velocity);
                if (mode == Mode.kDeleting && pressNote == note.note)
                {
                    float start = Mathf.Min(pressTime, dragTime);
                    float end = Mathf.Max(pressTime, dragTime);

                    if (note.OverlapsRange(start, end))
                        color = deletingCell;
                }
                DrawNote(note.note, note.start, note.end, color);
            }
        }

        void DrawActiveNotes(HelmSequencer sequencer)
        {
            if (sequencer.allNotes == null)
                return;

            for (int i = 0; i < numRows; ++i)
                DrawRowNotes(sequencer.allNotes[i].notes);
        }

        void DrawPressedNotes()
        {
            if (mode == Mode.kResizingStart || mode == Mode.kResizingEnd)
            {
                DrawNote(activeNote.note, activeNote.start, activeNote.end, pressedCell);
            }
            else if (mode == Mode.kAdding)
            {
                int startDrag = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(pressTime, dragTime)));
                int endDrag = (int)Mathf.Ceil(Mathf.Max(pressTime, dragTime));

                for (int i = startDrag; i < endDrag; ++i)
                    DrawNote(pressNote, i, i + 1, pressedCell);
            }
        }

        public void DrawSequencer(Rect rect, HelmSequencer sequencer)
        {
            numRows = HelmSequencer.kRows;
            numCols = sequencer.length;
            colWidth = (rect.width - keyboardWidth - rightPadding) / numCols;
            float scrollableWidth = numCols * colWidth + keyboardWidth + 1;
            float scrollableHeight = numRows * rowHeight;

            if (lastHeight != rect.height)
            {
                lastHeight = rect.height;
                scrollPosition.y = (scrollableHeight - rect.height) / 2.0f;
            }

            Rect scrollableArea = new Rect(0, 0, scrollableWidth, scrollableHeight);
            scrollPosition = GUI.BeginScrollView(rect, scrollPosition, scrollableArea, false, true);

            DrawNoteRows(scrollableArea);
            DrawBarHighlights(scrollableArea);
            DrawNoteDivisionLines(scrollableArea);
            DrawActiveNotes(sequencer);
            DrawPressedNotes();

            GUI.EndScrollView();
        }
    }
}
