// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Tytel
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

        void MouseDown(Rect rect, HelmSequencer sequencer, Vector2 mousePosition)
        {
            currentNote = null;
            float closest = 2.0f  * velocityHandleGrabWidth;
            float mouseX = mousePosition.x - rect.x;
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
                Undo.RecordObject(sequencer, "Set Note Velocity");
        }

        void MouseDrag(float velocity)
        {
            if (currentNote != null)
                currentNote.velocity = velocity;
        }

        public bool DoVelocityEvents(Rect rect, HelmSequencer sequencer)
        {
            Event evt = Event.current;

            sixteenthWidth = rect.width / sequencer.length;

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

        void DrawNote(Note note)
        {
            float x = sixteenthWidth * note.start;
            float h = note.velocity * (height - velocityHandleWidth) + velocityHandleWidth / 2.0f;
            float y = height - h;

            EditorGUI.DrawRect(new Rect(x - velocityMeterWidth / 2.0f, y, velocityMeterWidth, h), velocityColor);
            EditorGUI.DrawRect(new Rect(x - velocityHandleWidth / 2.0f, y - velocityHandleWidth / 2.0f, velocityHandleWidth, velocityHandleWidth), velocityColor);
        }

        void DrawRowNotes(List<Note> rowNotes)
        {
            foreach (Note note in rowNotes)
                DrawNote(note);
        }

        void DrawNoteVelocities(HelmSequencer sequencer)
        {
            if (sequencer.allNotes == null)
                return;

            for (int i = 0; i < sequencer.allNotes.Length; ++i)
                DrawRowNotes(sequencer.allNotes[i].notes);
        }

        public void DrawSequencerPosition(Rect rect, HelmSequencer sequencer)
        {
            Rect activeArea = new Rect(rect);
            activeArea.x += leftPadding;
            activeArea.width -= leftPadding + rightPadding;

            sixteenthWidth = activeArea.width / sequencer.length;
            height = activeArea.height;

            EditorGUI.DrawRect(rect, background);
            EditorGUI.DrawRect(activeArea, activeAreaColor);

            GUI.BeginGroup(activeArea);
            DrawNoteVelocities(sequencer);
            GUI.EndGroup();
        }
    }
}
