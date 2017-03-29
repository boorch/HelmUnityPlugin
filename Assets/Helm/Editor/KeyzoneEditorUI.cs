// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;

namespace Helm
{
    public class KeyzoneEditorUI
    {
        bool mouseActive = false;
        Vector2 keyboardScrollPosition;
        float keyWidth = minKeyWidth;
        int scrollWidth = 15;

        Color lightenColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        Color keylaneBackground = new Color(0.5f, 0.5f, 0.5f);
        const int keyboardHeight = 16;
        const int rowHeight = 16;
        const int keyzoneWidth = 150;
        const int minKeyWidth = 6;

        public KeyzoneEditorUI(int scroll)
        {
            scrollWidth = scroll;
        }

        Vector2 GetKeyzonePosition(Rect rect, Vector2 mousePosition)
        {
            float row = (mousePosition.y - keyboardHeight) / rowHeight;
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
                float x = i * keyWidth;
                Rect rect = new Rect(x, 0, keyWidth, keyboardHeight);

                if (Utils.IsBlackKey(i))
                    EditorGUI.DrawRect(rect, Color.black);
                else
                {
                    Rect keyLane = new Rect(x, keyboardHeight, keyWidth, height - keyboardHeight);
                    EditorGUI.DrawRect(rect, Color.white);
                    EditorGUI.DrawRect(keyLane, lightenColor);
                    if (!Utils.IsBlackKey(i + 1))
                        EditorGUI.DrawRect(new Rect(x + keyWidth - 1, 0, 1, keyboardHeight), Color.black);
                }
            }
        }

        void DrawClips(Sampler sampler)
        {
            GUIStyle style = GUI.skin.button;
            style.padding = new RectOffset(0, 0, 0, 0);
            style.fontSize = rowHeight - 4;
            int y = keyboardHeight;

            Keyzone remove = null;

            foreach (Keyzone keyzone in sampler.keyzones)
            {
                Rect buttonRect = new Rect(0, y, rowHeight, rowHeight);
                Rect clipRect = new Rect(buttonRect.xMax, y, keyzoneWidth - buttonRect.width, rowHeight);
                if (GUI.Button(buttonRect, "X", style))
                    remove = keyzone;
                AudioClip clip = EditorGUI.ObjectField(clipRect, keyzone.audioClip, typeof(AudioClip), false) as AudioClip;
                if (clip != keyzone.audioClip)
                {
                    if (clip == null)
                        Undo.RecordObject(sampler, "Remove AudioClip from Keyzone");
                    else
                        Undo.RecordObject(sampler, "Change AudioClip in Keyzone");
                    keyzone.audioClip = clip;
                }
                y += rowHeight;
            }

            if (remove != null)
            {
                Undo.RecordObject(sampler, "Delete Keyzone");
                sampler.RemoveKeyzone(remove);
            }
        }

        public int GetHeight(Sampler sampler)
        {
            return keyboardHeight + rowHeight * sampler.keyzones.Count + scrollWidth;
        }

        public void DrawKeyzones(Rect rect, Sampler sampler)
        {
            int numKeyzones = 0;
            float scrollableHeight = Mathf.Max(rect.height, keyboardHeight + numKeyzones * rowHeight);
            float computedKeyWidth = (1.0f * rect.width - keyzoneWidth) / Utils.kMidiSize;
            keyWidth = Mathf.Max(computedKeyWidth, minKeyWidth);
            float scrollableWidth = Utils.kMidiSize * keyWidth;

            GUI.BeginGroup(rect);
            DrawClips(sampler);
            GUIStyle style = GUI.skin.button;
            style.padding = new RectOffset(0, 0, 0, 1);
            style.fontSize = rowHeight - 2;
            Rect buttonRect = new Rect(0, 0, keyboardHeight, keyboardHeight);
            if (GUI.Button(buttonRect, "+", style))
            {
                Undo.RecordObject(sampler, "Add Keyzone");
                sampler.AddKeyzone();
            }

            Rect keySection = new Rect(keyzoneWidth, 0, rect.width - keyzoneWidth, rect.height);
            Rect keyboardScroll = new Rect(0, 0, scrollableWidth, rect.height - scrollWidth);
            keyboardScrollPosition = GUI.BeginScrollView(keySection, keyboardScrollPosition, keyboardScroll, true, false);

            EditorGUI.DrawRect(new Rect(0, 0, keyboardScroll.width, keyboardScroll.height), keylaneBackground);

            DrawKeyboard(scrollableHeight);
            GUI.EndScrollView();
            GUI.EndGroup();
        }
    }
}
