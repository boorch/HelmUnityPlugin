using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Tytel
{
    public class SequencerUI
    {
        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOn(int channel, int note);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOff(int channel, int note);

        enum Mode
        {
            kWaiting,
            kAdding,
            kDeleting,
            kKeyboarding,
            kNumModes
        }

        Mode mode = Mode.kWaiting;
        int pressNote = 0;
        float pressTime = 0.0f;
        float dragTime = 0.0f;
        int pressedKey = -1;

        Color emptyCellBlack = new Color(0.5f, 0.5f, 0.5f);
        Color emptyCellWhite = new Color(0.6f, 0.6f, 0.6f);
        Color noteDivision = new Color(0.4f, 0.4f, 0.4f);
        Color beatDivision = new Color(0.2f, 0.2f, 0.2f);
        Color fullCell = Color.red;
        Color pressedCell = new Color(0.9f, 0.8f, 0.7f);
        Color deletingCell = new Color(0.7f, 1.0f, 0.7f);
        Color lightenColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        Color blackKey = Color.black;
        Color whiteKey = Color.white;
        Color blackKeyPressed = Color.red;
        Color whiteKeyPressed = Color.red;

        float rowHeight = 10.0f;
        float keyboardWidth = 20.0f;
        int numRows = 128;
        int numCols = 16;
        int notesPerBeat = 4;
        float colWidth = 30.0f;

        Vector2 scrollPosition;

        Vector2 mouseBump = new Vector2(0.0f, -3.0f);
        const float rightPadding = 15.0f;

        Vector2 GetSequencerPosition(Rect rect, Vector2 mousePosition)
        {
            if (!rect.Contains(mousePosition))
                return -Vector2.one;

            Vector2 localPosition = mousePosition - rect.position + scrollPosition + mouseBump;
            float note = numRows - Mathf.Floor((localPosition.y / rowHeight)) - 1;
            float time = (localPosition.x - keyboardWidth) / colWidth;
            return new Vector2(time, note);
        }

        void MouseDown(int note, float time, HelmSequencer sequencer)
        {
            if (pressedKey >= 0)
            {
                HelmNoteOff(sequencer.channel, pressedKey);
                pressedKey = -1;
            }
            if (time < 0.0f)
            {
                mode = Mode.kKeyboarding;
                pressedKey = note;
                HelmNoteOn(sequencer.channel, pressedKey);
                Debug.Log(pressedKey);
                return;
            }
            else if (sequencer.NoteExistsInRange(note, time, time))
                mode = Mode.kDeleting;
            else
                mode = Mode.kAdding;

            pressNote = note;
            pressTime = time;
            dragTime = time;
        }

        void MouseDrag(int note, float time, HelmSequencer sequencer)
        {
            if (mode == Mode.kKeyboarding)
            {
                if (note != pressedKey)
                {
                    HelmNoteOff(sequencer.channel, pressedKey);
                    HelmNoteOn(sequencer.channel, note);
                    pressedKey = note;
                }
            }
            else
                dragTime = time;
        }

        void MouseUp(float time, HelmSequencer sequencer)
        {
            if (mode == Mode.kKeyboarding)
            {
                HelmNoteOff(sequencer.channel, pressedKey);
                pressedKey = -1;
                return;
            }
            dragTime = time;
            float startTime = Mathf.Min(pressTime, dragTime);
            float endTime = Mathf.Max(pressTime, dragTime);
            sequencer.RemoveNotesInRange(pressNote, startTime, endTime);

            int startDrag = Mathf.Max(0, (int)Mathf.Floor(startTime));
            int endDrag = (int)Mathf.Ceil(endTime);

            if (mode == Mode.kAdding)
            {
                for (int i = startDrag; i < endDrag; ++i)
                    sequencer.AddNote(pressNote, i, i + 1);
            }
            mode = Mode.kWaiting;
        }

        public bool DoSequencerEvents(Rect rect, HelmSequencer sequencer)
        {
            Event evt = Event.current;
            Vector2 sequencerPosition = GetSequencerPosition(rect, evt.mousePosition);
            float time = sequencerPosition.x;

            if (evt.type == EventType.MouseUp)
            {
                MouseUp(time, sequencer);
                return true;
            }

            if (!rect.Contains(evt.mousePosition))
                return false;

            int note = (int)sequencerPosition.y;
            if (note >= numRows || note < 0)
                return false;

            if (evt.type == EventType.MouseDown)
                MouseDown(note, time, sequencer);
            else if (evt.type == EventType.MouseDrag)
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
                Rect row = new Rect(keyboardWidth, y, rect.width, rowHeight);
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
            Rect noteRect = new Rect(x + 1, y, width - 1, rowHeight);
            EditorGUI.DrawRect(noteRect, color);
        }

        void DrawRowNotes(List<HelmSequencer.Note> notes)
        {
            if (notes == null)
                return;

            foreach (HelmSequencer.Note note in notes)
            {
                Color color = fullCell;
                if (mode == Mode.kDeleting && pressNote == note.note)
                {
                    float start = Mathf.Min(pressTime, dragTime);
                    float end = Mathf.Max(pressTime, dragTime);

                    if (HelmSequencer.IsNoteInRange(note, start, end))
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
                DrawRowNotes(sequencer.allNotes[i]);
        }

        void DrawPressedNotes()
        {
            if (mode != Mode.kAdding)
                return;

            int startDrag = Mathf.Max(0, (int)Mathf.Floor(Mathf.Min(pressTime, dragTime)));
            int endDrag = (int)Mathf.Ceil(Mathf.Max(pressTime, dragTime));

            for (int i = startDrag; i < endDrag; ++i)
                DrawNote(pressNote, i, i + 1, pressedCell);
        }

        public void DrawSequencer(Rect rect, HelmSequencer sequencer)
        {
            numRows = sequencer.rows;
            float scrollableWidth = numCols * colWidth + keyboardWidth + 1;
            float scrollableHeight = numRows * rowHeight;
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
