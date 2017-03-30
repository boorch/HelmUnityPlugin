// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Helm
{
    public class SequencerUI
    {
        enum Mode
        {
            kWaiting,
            kAdding,
            kDeleting,
            kKeyboarding,
            kDragging,
            kDraggingStart,
            kDraggingEnd,
            kNumModes
        }

        public SequencerUI(float keyboard, float scroll)
        {
            keyboardWidth = keyboard;
            rightPadding = scroll;
        }

        public int minKey = 0;
        public int maxKey = Utils.kMidiSize - 1;

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
        Color darkenColor = new Color(0.0f, 0.0f, 0.0f, 0.05f);
        Color blackKey = Color.black;
        Color whiteKey = Color.white;
        Color blackKeyPressed = Color.red;
        Color whiteKeyPressed = Color.red;

        float rowHeight = 10.0f;
        const float minRowHeight = 10.0f;
        const float maxRowHeight = 35.0f;
        int numRows = Utils.kMidiSize;
        int numCols = 16;
        int notesPerBeat = 4;
        float lastHeight = 0;
        float colWidth = 30.0f;

        Vector2 scrollPosition;

        Vector2 GetSequencerPosition(Rect rect, Vector2 mousePosition)
        {
            if (!rect.Contains(mousePosition))
                return -Vector2.one;

            Vector2 localPosition = mousePosition - rect.position + scrollPosition;
            float note = minKey + numRows - Mathf.Floor((localPosition.y / rowHeight)) - 1;
            float time = (localPosition.x - keyboardWidth) / colWidth;
            return new Vector2(time, note);
        }

        public bool MouseActive()
        {
            return mouseActive;
        }

        void MouseDown(int note, float time, Sequencer sequencer, SerializedProperty allNotes, bool edit)
        {
            roundingToSixteenth = false;
            mouseActive = true;
            activeNote = sequencer.GetNoteInRange(note, time, time);
            dragTime = time;
            if (pressedKey >= 0)
            {
                sequencer.NoteOff(pressedKey);
                pressedKey = -1;
            }
            if (time < 0.0f)
            {
                mode = Mode.kKeyboarding;
                pressedKey = note;
                sequencer.NoteOn(pressedKey, 1.0f);
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
                    Undo.RecordObject(sequencer, "Move Note End");
                    mode = Mode.kDraggingEnd;
                    activeNote.end = Mathf.Max(activeNote.start + minNoteTime, dragTime);
                }
                else if (startPixels <= grabResizeWidth)
                {
                    Undo.RecordObject(sequencer, "Move Note Start");
                    mode = Mode.kDraggingStart;
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

        void MouseDrag(int note, float time, Sequencer sequencer, SerializedProperty allNotes)
        {
            float clampedTime = Mathf.Clamp(time, 0.0f, sequencer.length);
            dragTime = clampedTime;

            if (Mathf.Abs(dragTime - pressTime) >= dragDeltaStartRounding)
                roundingToSixteenth = true;

            if (mode == Mode.kKeyboarding)
            {
                if (note != pressedKey)
                {
                    sequencer.NoteOff(pressedKey);
                    sequencer.NoteOn(note);
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
                    sequencer.NoteOff(activeNote.note);
                    activeNote.note = note;
                }
            }
            else if (mode == Mode.kDraggingStart)
            {
                if (activeNote != null)
                {
                    float startTime = dragTime;
                    if (roundingToSixteenth)
                        startTime = Mathf.Round(dragTime);
                    activeNote.start = Mathf.Min(activeNote.end - minNoteTime, startTime);

                    sequencer.NoteOff(activeNote.note);
                }
            }
            else if (mode == Mode.kDraggingEnd)
            {
                if (activeNote != null)
                {
                    float endTime = dragTime;
                    if (roundingToSixteenth)
                        endTime = Mathf.Round(dragTime);
                    activeNote.end = Mathf.Max(activeNote.start + minNoteTime, endTime);

                    sequencer.NoteOff(activeNote.note);
                }
            }
        }

        void MouseUp(float time, Sequencer sequencer, SerializedProperty allNotes)
        {
            mouseActive = false;
            if (mode == Mode.kKeyboarding)
            {
                sequencer.NoteOff(pressedKey);
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

                sequencer.NoteOff(activeNote.note);
            }
            else if (mode == Mode.kDraggingStart)
            {
                Undo.RecordObject(sequencer, "Move Note Start");

                if (activeNote != null)
                    sequencer.ClampNotesInRange(pressNote, activeNote.start, activeNote.end, activeNote);

                sequencer.NoteOff(pressNote);
            }
            else if (mode == Mode.kDraggingEnd)
            {
                Undo.RecordObject(sequencer, "Move Note End");

                if (activeNote != null)
                    sequencer.ClampNotesInRange(pressNote, activeNote.start, activeNote.end, activeNote);

                sequencer.NoteOff(pressNote);
            }
            else if (mode == Mode.kAdding)
            {
                Undo.RecordObject(sequencer, "Add Sequencer Notes");
                int startDrag = Mathf.FloorToInt(startTime);
                int endDrag = Mathf.CeilToInt(endTime);

                sequencer.ClampNotesInRange(pressNote, startDrag, endDrag);

                for (int i = startDrag; i < endDrag; ++i)
                    sequencer.AddNote(pressNote, i, i + 1, defaultVelocity);

                sequencer.NoteOff(pressNote);
            }
            else if (mode == Mode.kDeleting)
            {
                Undo.RecordObject(sequencer, "Delete Sequencer Notes");
                sequencer.RemoveNotesInRange(pressNote, startTime, endTime);
                sequencer.NoteOff(pressNote);
            }
            mode = Mode.kWaiting;
        }

        public bool DoSequencerEvents(Rect rect, Sequencer sequencer, SerializedProperty allNotes)
        {
            Event evt = Event.current;
            if (!evt.isMouse)
                return false;

            Vector2 sequencerPosition = GetSequencerPosition(rect, evt.mousePosition);
            EventModifiers modifiers = (EventModifiers.Shift | EventModifiers.Control |
                                        EventModifiers.Alt | EventModifiers.Command);
            bool modifier = (evt.modifiers & modifiers) != EventModifiers.None;
            bool edit = evt.button > 0 || modifier;
            float time = sequencerPosition.x;

            if (evt.type == EventType.MouseUp && mouseActive)
            {
                MouseUp(time, sequencer, allNotes);
                return true;
            }

            int note = (int)sequencerPosition.y;
            if (note > maxKey || note < minKey)
                return false;

            Rect ignoreScrollRect = new Rect(rect);
            ignoreScrollRect.width -= rightPadding;
            if (evt.type == EventType.MouseDown && ignoreScrollRect.Contains(evt.mousePosition))
                MouseDown(note, time, sequencer, allNotes, edit);
            else if (evt.type == EventType.MouseDrag && mouseActive)
                MouseDrag(note, time, sequencer, allNotes);
            return true;
        }

        void DrawNoteRows(Rect rect)
        {
            GUIStyle cStyle = new GUIStyle();
            cStyle.fontSize = 8;
            cStyle.alignment = TextAnchor.MiddleRight;
            cStyle.padding = new RectOffset(0, 1, 0, 0);

            float y = 0.0f;
            for (int i = maxKey; i >= minKey; --i)
            {
                Color keyColor = whiteKey;
                Color rowColor = emptyCellWhite;

                if (Utils.IsBlackKey(i))
                {
                    if (pressedKey == i)
                        keyColor = blackKeyPressed;
                    else
                        keyColor = blackKey;
                    rowColor = emptyCellBlack;
                }
                else if (pressedKey == i)
                    keyColor = whiteKeyPressed;

                Rect key = new Rect(0.0f, y, keyboardWidth, rowHeight - 1);
                Rect row = new Rect(keyboardWidth, y, rect.width - keyboardWidth, rowHeight);
                EditorGUI.DrawRect(key, keyColor);
                EditorGUI.DrawRect(new Rect(key.x, key.yMax, key.width, 1), Color.black);
                if (i % Utils.kNotesPerOctave == 0)
                    GUI.Label(key, "C" + Utils.GetOctave(i), cStyle);

                EditorGUI.DrawRect(row, rowColor);
                EditorGUI.DrawRect(new Rect(row.x, row.yMax - 1, row.width, 1), darkenColor);
                y += rowHeight;
            }
        }

        void DrawBarHighlights(Rect rect)
        {
            float x = keyboardWidth;
            int numBars = 1 + (numCols - 1) / notesPerBeat;
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
            float y = (numRows - (note - minKey) - 1) * rowHeight;
            float width = end * colWidth + keyboardWidth - x;
            Rect noteOutsideRect = new Rect(x, y, width + 1, rowHeight);
            Rect noteRect = new Rect(x + 1, y + 1, width - 1, rowHeight - 2);
            EditorGUI.DrawRect(noteOutsideRect, Color.black);
            EditorGUI.DrawRect(noteRect, color);
            Rect leftResizeRect = new Rect(x, y, grabResizeWidth, rowHeight);
            Rect rightResizeRect = new Rect(noteRect.xMax - grabResizeWidth, y, grabResizeWidth, rowHeight);
            EditorGUIUtility.AddCursorRect(leftResizeRect, MouseCursor.SplitResizeLeftRight);
            EditorGUIUtility.AddCursorRect(rightResizeRect, MouseCursor.SplitResizeLeftRight);
        }

        void DrawRowNotes(SerializedProperty notes)
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

        void DrawActiveNotes(Sequencer sequencer, SerializedProperty allNotes)
        {
            if (sequencer.allNotes == null)
                return;

            for (int i = minKey; i <= maxKey; ++i)
            {
                SerializedProperty row = allNotes.GetArrayElementAtIndex(i);
                SerializedProperty notes = row.FindPropertyRelative("notes");

                if (notes != null)
                    DrawRowNotes(notes);
            }
        }

        void DrawPressedNotes()
        {
            if (mode == Mode.kDraggingStart || mode == Mode.kDraggingEnd)
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

        void DrawPositionOverlay(Sequencer sequencer)
        {
            if (!sequencer.isActiveAndEnabled || !Application.isPlaying)
                return;

            float x = keyboardWidth + colWidth * sequencer.currentSixteenth;
            float height = numRows * rowHeight;
            EditorGUI.DrawRect(new Rect(x, 0, colWidth, height), lightenColor);
        }

        public float GetScrollPosition(Sequencer sequencer, float height)
        {
            float lowerBuffer = rowHeight * 8.0f;
            float totalHeight = sequencer.allNotes.Length * rowHeight;

            for (int i = 0; i < sequencer.allNotes.Length; ++i)
            {
                if (sequencer.allNotes[i] != null && sequencer.allNotes[i].notes.Count > 0)
                {
                    float noteY = (sequencer.allNotes.Length - i - 1) * rowHeight;
                    float bottom = Mathf.Clamp(noteY + lowerBuffer, height, totalHeight);
                    return bottom - height;
                }
            }

            return (sequencer.allNotes.Length * rowHeight - height) / 2.0f;
        }

        public float GetMaxHeight()
        {
            return maxRowHeight * (maxKey - minKey + 1);
        }

        public void DrawSequencer(Rect rect, Sequencer sequencer, SerializedProperty allNotes)
        {
            numRows = maxKey - minKey + 1;
            numCols = sequencer.length;
            colWidth = (rect.width - keyboardWidth - rightPadding) / numCols;
            rowHeight = Mathf.Clamp(rect.height / numRows, minRowHeight, maxRowHeight);
            float scrollableWidth = numCols * colWidth + keyboardWidth + 1;
            float scrollableHeight = numRows * rowHeight;

            if (lastHeight != rect.height)
            {
                lastHeight = rect.height;
                scrollPosition.y = GetScrollPosition(sequencer, rect.height);
            }

            Rect scrollableArea = new Rect(0, 0, scrollableWidth, scrollableHeight);
            scrollPosition = GUI.BeginScrollView(rect, scrollPosition, scrollableArea, false, true);

            DrawNoteRows(scrollableArea);
            DrawBarHighlights(scrollableArea);
            DrawNoteDivisionLines(scrollableArea);
            DrawActiveNotes(sequencer, allNotes);
            DrawPressedNotes();
            DrawPositionOverlay(sequencer);

            GUI.EndScrollView();
        }
    }
}
