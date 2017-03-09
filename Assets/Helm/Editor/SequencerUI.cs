using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Tytel
{
    public class SequencerUI
    {
        Color emptyCellEven = new Color(0.5f, 0.5f, 0.5f);
        Color emptyCellOdd = new Color(0.6f, 0.6f, 0.6f);
        Color noteDivision = new Color(0.4f, 0.4f, 0.4f);
        Color beatDivision = new Color(0.2f, 0.2f, 0.2f);
        Color fullCell = Color.red;
        Color lightenColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);

        float rowHeight = 10.0f;
        int numRows = 127;
        int numCols = 16;
        int notesPerBeat = 4;
        float colWidth = 30.0f;

        Vector2 scrollPosition;
        const float rightPadding = 15.0f;

        public bool DoSequencerEvents(Rect rect, HelmSequencer sequencer)
        {
            Event evt = Event.current;
            return false;
        }

        void DrawNoteRows(Rect rect)
        {
            float y = 0.0f;
            for (int i = 0; i < numRows; ++i)
            {
                Rect row = new Rect(0, y, rect.width, rowHeight + 1);
                if (i % 2 == 0)
                    EditorGUI.DrawRect(row, emptyCellEven);
                else
                    EditorGUI.DrawRect(row, emptyCellOdd);
                y += rowHeight;
            }
        }

        void DrawBarHighlights(Rect rect)
        {
            float x = 0.0f;
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
            float x = 0;
            for (int i = 0; i < numCols; ++i)
            {
                Rect line = new Rect(x, 0, 1.0f, rect.height);
                if (i % notesPerBeat == 0)
                    EditorGUI.DrawRect(line, beatDivision);
                else
                    EditorGUI.DrawRect(line, noteDivision);
                x += colWidth;
            }
        }

        public void DrawSequencer(Rect rect, HelmSequencer sequencer)
        {
            float scrollableWidth = numCols * colWidth;
            float scrollableHeight = numRows * rowHeight;
            Rect scrollableArea = new Rect(0, 0, scrollableWidth, scrollableHeight);
            scrollPosition = GUI.BeginScrollView(rect, scrollPosition, scrollableArea, false, true);

            DrawNoteRows(scrollableArea);
            DrawBarHighlights(scrollableArea);
            DrawNoteDivisionLines(scrollableArea);

            GUI.EndScrollView();
        }
    }
}
