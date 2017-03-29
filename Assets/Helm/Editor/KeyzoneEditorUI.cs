// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

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
        Color rootNoteColor = new Color(0.7f, 1.0f, 1.0f);
        Color keyzoneRangeColor = new Color(0.7f, 1.0f, 0.7f);
        const int keyboardHeight = 16;
        const int rowHeight = 32;
        const int keyzoneWidth = 150;
        const int minKeyWidth = 6;
        const int buttonBuffer = 17;

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
            int height = rowHeight / 2;
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.fontSize = height - 4;
            int y = keyboardHeight;

            Keyzone remove = null;

            foreach (Keyzone keyzone in sampler.keyzones)
            {
                Rect buttonRect = new Rect(0, y, height, height);
                Rect clipRect = new Rect(buttonRect.xMax, y, keyzoneWidth - buttonRect.width, height);
                Rect mixerRect = new Rect(buttonRect.xMax, y + height, keyzoneWidth - buttonRect.width, height);

                if (GUI.Button(buttonRect, "X", style))
                    remove = keyzone;

                AudioClip clip = EditorGUI.ObjectField(clipRect, keyzone.audioClip, typeof(AudioClip), false)
                                 as AudioClip;
                AudioMixerGroup mixer = EditorGUI.ObjectField(mixerRect, keyzone.mixer, typeof(AudioMixerGroup), false)
                                        as AudioMixerGroup;

                if (clip != keyzone.audioClip)
                {
                    if (clip == null)
                        Undo.RecordObject(sampler, "Remove AudioClip from Keyzone");
                    else
                        Undo.RecordObject(sampler, "Change AudioClip in Keyzone");
                    keyzone.audioClip = clip;
                }
                if (mixer != keyzone.mixer)
                {
                    if (mixer == null)
                        Undo.RecordObject(sampler, "Remove AudioMixerGroup from Keyzone");
                    else
                        Undo.RecordObject(sampler, "Change AudioMixerGroup in Keyzone");
                    keyzone.mixer = mixer;
                }
                y += rowHeight;
            }

            if (remove != null)
            {
                Undo.RecordObject(sampler, "Delete Keyzone");
                sampler.RemoveKeyzone(remove);
            }
        }

        void DrawKeyzoneRanges(Sampler sampler)
        {
            int y = keyboardHeight;

            foreach (Keyzone keyzone in sampler.keyzones)
            {
                int range = keyzone.maxKey - keyzone.minKey + 1;
                float rangeX = keyzone.minKey * keyWidth;
                float width = range * keyWidth;
                float height = rowHeight / 2.0f;
                Rect rootRect = new Rect(keyzone.rootKey * keyWidth, y, keyWidth, height);
                Rect rangeRect = new Rect(rangeX, y + height, width, height);
                EditorGUI.DrawRect(rootRect, rootNoteColor);
                EditorGUI.DrawRect(rangeRect, keyzoneRangeColor);
                y += rowHeight;
            }
        }

        public int GetHeight(Sampler sampler)
        {
            return keyboardHeight + rowHeight * sampler.keyzones.Count + scrollWidth;
        }

        void AddKeyzone(Sampler sampler)
        {
            Keyzone keyzone = sampler.AddKeyzone();
            if (sampler.keyzones.Count >= 2)
            {
                Keyzone lastKeyzone = sampler.keyzones[sampler.keyzones.Count - 2];
                int min = lastKeyzone.maxKey + 1;
                int range = lastKeyzone.maxKey - lastKeyzone.minKey;
                int max = min + range;
                if (max < Utils.kMidiSize)
                {
                    keyzone.minKey = min;
                    keyzone.maxKey = max;
                }
                keyzone.mixer = lastKeyzone.mixer;
            }
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
            Rect buttonRect = new Rect(0, 0, keyzoneWidth - buttonBuffer, keyboardHeight);
            if (GUI.Button(buttonRect, "Add Keyzone"))
            {
                Undo.RecordObject(sampler, "Add Keyzone");
                AddKeyzone(sampler);
            }

            Rect keySection = new Rect(keyzoneWidth, 0, rect.width - keyzoneWidth, rect.height);
            Rect keyboardScroll = new Rect(0, 0, scrollableWidth, rect.height - scrollWidth);
            keyboardScrollPosition = GUI.BeginScrollView(keySection, keyboardScrollPosition, keyboardScroll, true, false);

            EditorGUI.DrawRect(new Rect(0, 0, keyboardScroll.width, keyboardScroll.height), keylaneBackground);

            DrawKeyboard(scrollableHeight);
            DrawKeyzoneRanges(sampler);
            GUI.EndScrollView();
            GUI.EndGroup();
        }
    }
}
