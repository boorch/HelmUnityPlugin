// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;

namespace Helm
{
    public class KeyzoneUI
    {
        bool mouseActive = false;
        Vector2 keyboardScrollPosition;
        Vector2 keyzoneScrollPosition;
        float rightPadding = 15.0f;

        Color lightenColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        Color keylaneBackground = new Color(0.5f, 0.5f, 0.5f);
        const int keyboardHeight = 15;
        const int rowHeight = 15;
        const int keyzoneWidth = 150;
        const int keyWidth = 10;

        public KeyzoneUI(float scroll)
        {
            rightPadding = scroll;
        }

        Vector2 GetKeyzonePosition(Rect rect, Vector2 mousePosition)
        {
            float row = (mousePosition.y - keyboardHeight + keyzoneScrollPosition.y) / rowHeight;
            float key = (mousePosition.x - keyzoneWidth + keyboardScrollPosition.x) / keyWidth;
            return new Vector2(key, row);
        }

        void MouseUp(float key, int row, Sampler sampler)
        {

        }

        void MouseDown(float key, int row, Sampler sampler)
        {

        }

        void MouseDrag(float key, int row, Sampler sampler)
        {

        }

        public bool DoKeyzoneEvents(Rect rect, Sampler sampler)
        {
            Event evt = Event.current;
            if (!evt.isMouse)
                return false;

            Vector2 keyzonePosition = GetKeyzonePosition(rect, evt.mousePosition);
            float key = keyzonePosition.x;
            int row = (int)keyzonePosition.y;

            if (evt.type == EventType.MouseUp && mouseActive)
            {
                MouseUp(key, row, sampler);
                return true;
            }

            Rect ignoreScrollRect = new Rect(rect);
            ignoreScrollRect.width -= rightPadding;

            if (evt.type == EventType.MouseDown && ignoreScrollRect.Contains(evt.mousePosition))
                MouseDown(key, row, sampler);
            else if (evt.type == EventType.MouseDrag && mouseActive)
                MouseDrag(key, row, sampler);
            return true;
        }

        void DrawKeyboard(float height)
        {
            for (int i = 0; i < Utils.kMidiSize; ++i)
            {
                int x = i * keyWidth;

                Color color = Color.white;
                if (Utils.IsBlackKey(i))
                    color = Color.black;
                else
                {
                    Rect keyLane = new Rect(x, keyboardHeight, keyWidth, height - keyboardHeight);
                    EditorGUI.DrawRect(keyLane, lightenColor);
                }

                EditorGUI.DrawRect(new Rect(x, 0, keyWidth, keyboardHeight), color);
                EditorGUI.DrawRect(new Rect(x + keyWidth - 1, 0, 1, keyboardHeight), Color.black);
            }
        }

        public void DrawKeyzones(Rect rect, Sampler sampler)
        {
            int numKeyzones = 0;
            float scrollableHeight = Mathf.Max(rect.height, keyboardHeight + numKeyzones * rowHeight);
            float scrollableWidth = Utils.kMidiSize * keyWidth;

            Rect keyzoneScroll = new Rect(0, 0, rect.width - rightPadding, scrollableHeight);
            keyzoneScrollPosition = GUI.BeginScrollView(rect, keyzoneScrollPosition, keyzoneScroll, false, true);
            // EditorGUI.DrawRect(new Rect(0, 0, keyzoneScroll.width, keyzoneScroll.height), Color.white);

            Rect keySection = new Rect(keyzoneWidth, 0, keyzoneScroll.width - keyzoneWidth, keyzoneScroll.height);
            Rect keyboardScroll = new Rect(0, 0, scrollableWidth, keyzoneScroll.height - rightPadding);
            keyboardScrollPosition = GUI.BeginScrollView(keySection, keyboardScrollPosition, keyboardScroll, true, false);

            EditorGUI.DrawRect(new Rect(0, 0, keyboardScroll.width, keyboardScroll.height), keylaneBackground);

            DrawKeyboard(scrollableHeight);
            GUI.EndScrollView();
            GUI.EndScrollView();
        }
    }
}
