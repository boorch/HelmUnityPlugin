// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Helm
{
    public class SequencerVelocityUI
    {
        const float velocityMeterWidth = 3.0f;
        const float velocityHandleWidth = 7.0f;
        const float velocityHandleGrabWidth = 13.0f;

        Note currentNote;

        float leftPadding = 0.0f;
        float rightPadding = 0.0f;
        Color activeAreaColor = new Color(0.6f, 0.6f, 0.6f);
        Color background = new Color(0.5f, 0.5f, 0.5f);
        Color velocityColor = new Color(1.0f, 0.3f, 0.3f);
        Color velocityActiveColor = new Color(0.6f, 1.0f, 1.0f);

        float sixteenthWidth = 1.0f;
        float height = 1.0f;

        public SequencerVelocityUI(float left, float right)
        {
            leftPadding = left;
            rightPadding = right;
        }

        public bool MouseActive()
        {
            return currentNote != null;
        }

        void MouseUp()
        {
            currentNote = null;
        }

        void MouseDown(Rect rect, Sequencer sequencer, Vector2 mousePosition)
        {
            currentNote = null;
            float closest = 2.0f  * velocityHandleGrabWidth;
            float mouseX = mousePosition.x - rect.x - leftPadding;
            float mouseY = mousePosition.y - rect.y;
            for (int i = sequencer.allNotes.Length - 1; i >= 0; --i)
            {
                foreach (Note note in sequencer.allNotes[i].notes)
                {
                    float x = sixteenthWidth * note.start;
                    float yInv = note.velocity * (rect.height - velocityHandleWidth) + velocityHandleWidth / 2.0f;
                    float y = rect.height - yInv;
                    float xDiff = Mathf.Abs(x - mouseX);
                    float yDiff = Mathf.Abs(y - mouseY);
                    float diffTotal = xDiff + yDiff;

                    if (xDiff <= velocityHandleGrabWidth && yDiff <= velocityHandleGrabWidth && diffTotal < closest)
                    {
                        closest = diffTotal;
                        currentNote = note;
                    }
                }
            }

            if (currentNote != null)
                Undo.RegisterCompleteObjectUndo(sequencer, "Set Note Velocity");
        }

        void MouseDrag(float velocity)
        {
            if (currentNote != null)
                currentNote.velocity = velocity;
        }

        public bool DoVelocityEvents(Rect rect, Sequencer sequencer)
        {
            Event evt = Event.current;

            sixteenthWidth = (rect.width - rightPadding - leftPadding) / sequencer.length;

            float velocityMovementHeight = rect.height - velocityHandleWidth;
            float minVelocityY = rect.y + velocityHandleWidth;
            float velocity = 1.0f - (evt.mousePosition.y - minVelocityY) / velocityMovementHeight;
            velocity = Mathf.Clamp(velocity, 0.001f, 1.0f);

            if (evt.type == EventType.MouseUp && MouseActive())
            {
                MouseUp();
                return true;
            }

            if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
            {
                MouseDown(rect, sequencer, evt.mousePosition);
                MouseDrag(velocity);
            }
            else if (evt.type == EventType.MouseDrag && MouseActive())
                MouseDrag(velocity);
            return true;
        }

        void DrawNote(Note note, Color color)
        {
            float x = sixteenthWidth * note.start;
            float h = note.velocity * (height - velocityHandleWidth) + velocityHandleWidth / 2.0f;
            float y = height - h;

            EditorGUI.DrawRect(new Rect(x - velocityMeterWidth / 2.0f, y, velocityMeterWidth, h), color);
            EditorGUI.DrawRect(new Rect(x - velocityHandleWidth / 2.0f, y - velocityHandleWidth / 2.0f,
                                        velocityHandleWidth, velocityHandleWidth), color);
        }

        void DrawRowNotes(List<Note> rowNotes)
        {
            foreach (Note note in rowNotes)
                DrawNote(note, velocityColor);
        }

        void DrawNoteVelocities(Sequencer sequencer)
        {
            if (sequencer.allNotes == null)
                return;

            for (int i = 0; i < sequencer.allNotes.Length; ++i)
                DrawRowNotes(sequencer.allNotes[i].notes);
        }

        public void DrawTextMeasurements(Rect rect)
        {
            const int barWidth = 4;

            GUIStyle style = new GUIStyle();
            style.fontSize = 9;
            style.padding = new RectOffset(0, barWidth, 0, 0);
            style.alignment = TextAnchor.UpperRight;
            GUI.Label(rect, "127", style);
            style.alignment = TextAnchor.LowerRight;
            GUI.Label(rect, "1", style);

            Rect bar = new Rect(rect.x + rect.width - 1, rect.y, 1, rect.height);
            Rect top = new Rect(rect.x + rect.width - barWidth, rect.y, barWidth, 1);
            Rect bottom = new Rect(rect.x + rect.width - barWidth, rect.y + rect.height - 1, barWidth, 1);
            Rect middle = new Rect(rect.x + rect.width - barWidth / 2.0f, rect.y + rect.height / 2.0f, barWidth / 2.0f, 1);
            EditorGUI.DrawRect(bar, Color.black);
            EditorGUI.DrawRect(top, Color.black);
            EditorGUI.DrawRect(bottom, Color.black);
            EditorGUI.DrawRect(middle, Color.black);
        }

        public void DrawSequencerPosition(Rect rect, Sequencer sequencer)
        {
            Rect activeArea = new Rect(rect);
            activeArea.x += leftPadding;
            activeArea.width -= leftPadding + rightPadding;

            sixteenthWidth = activeArea.width / sequencer.length;
            height = activeArea.height;

            EditorGUI.DrawRect(rect, background);
            EditorGUI.DrawRect(activeArea, activeAreaColor);

            Rect leftBufferArea = new Rect(rect);
            leftBufferArea.width = leftPadding;
            DrawTextMeasurements(leftBufferArea);

            GUI.BeginGroup(activeArea);
            DrawNoteVelocities(sequencer);
            if (currentNote != null)
                DrawNote(currentNote, velocityActiveColor);

            GUI.EndGroup();
        }
    }
}
