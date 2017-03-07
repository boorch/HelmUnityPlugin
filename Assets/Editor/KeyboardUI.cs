using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Tytel
{
    public class KeyboardUI
    {
        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOn(int channel, int note);

        [DllImport("AudioPluginHelm")]
        private static extern void HelmNoteOff(int channel, int note);

        Color blackUnpressed = Color.black;
        Color blackPressed = Color.blue;
        Color whiteunPressed = Color.white;
        Color whitePressed = Color.yellow;

        const float leftGrowth = 15.0f;
        const float rightGrowth = 10.0f;

        float keyWidth = 20.0f;
        float blackKeyWidthPercent = 0.55f;
        float verticalStagger = 15.0f;
        int middleKey = 60;
        int midiSize = 128;
        int currentKey = -1;

        int widthsPerOctave = 7;
        float[] xOffsets = new float[] { 0.0f, 0.65f, 1.0f, 1.8f, 2.0f,
                                          3.0f, 3.6f, 4.0f, 4.7f, 5.0f, 5.8f, 6.0f };
        bool[] blackKeys = new bool[] { false, true, false, true, false,
                                         false, true, false, true, false, true, false };

        public bool DoKeyboardEvents(Rect rect, int channel)
        {
            Event evt = Event.current;
            if (evt.type == EventType.MouseUp)
            {
                if (currentKey >= 0)
                    HelmNoteOff(channel, currentKey);
                currentKey = -1;
                return true;
            }
            else if (rect.Contains(evt.mousePosition) &&
                (evt.type == EventType.MouseDrag || evt.type == EventType.MouseDown))
            {
                int hovered = GetHoveredKey(evt.mousePosition, rect);
                if (hovered != currentKey)
                {
                    if (currentKey >= 0)
                        HelmNoteOff(channel, currentKey);
                    HelmNoteOn(channel, hovered);
                    currentKey = hovered;
                    return true;
                }
            }
            return false;
        }

        int GetHoveredKey(Vector2 position, Rect rect)
        {
            float octaveWidth = widthsPerOctave * keyWidth;
            float octaveOffset = (position.x - rect.center.x) / octaveWidth;
            int octave = (int)(middleKey / xOffsets.Length + octaveOffset);
            int octaveKey = octave * xOffsets.Length;

            float positionOffset = position.x - GetKeyXPosition(octaveKey, rect);
            float keyOffset = positionOffset / keyWidth;

            int key = 0;
            for (int i = 0; i < xOffsets.Length; ++i)
            {
                float width = 1.0f;
                if (blackKeys[i])
                    width = blackKeyWidthPercent;
                if (keyOffset >= xOffsets[i] && keyOffset <= xOffsets[i] + width)
                {
                    if (blackKeys[i])
                    {
                        if (position.y <= rect.yMax - verticalStagger)
                            return octaveKey + i;
                    }
                    else
                        key = i;
                }
            }
            return octaveKey + key;
        }

        float GetKeyXPosition(int key, Rect rect)
        {
            float xOffset = xOffsets[key % xOffsets.Length];
            int octave = key / xOffsets.Length - middleKey / xOffsets.Length;
            float octaveOffset = octave * widthsPerOctave * keyWidth;
            float offset = octaveOffset + keyWidth * xOffset;
            return rect.center.x + offset;
        }

        bool IsBlackKey(int key)
        {
            return blackKeys[key % blackKeys.Length];
        }

        Color GetKeyColor(int key, bool pressed)
        {
            if (IsBlackKey(key))
            {
                if (key == currentKey || pressed)
                    return blackPressed;
                else
                    return blackUnpressed;
            }
            if (key == currentKey || pressed)
                return whitePressed;
            return whiteunPressed;
        }

        bool ValidKey(int key)
        {
            return key >= 0 && key < midiSize;
        }

        bool DrawKey(int key, Rect rect, bool pressed)
        {
            if (!ValidKey(key))
                return false;

            float width = keyWidth;
            float height = rect.height;
            float position = GetKeyXPosition(key, rect);
            float y = rect.y;
            if (IsBlackKey(key))
            {
                width = keyWidth * blackKeyWidthPercent;
                height = rect.height - verticalStagger;
            }

            float left = Mathf.Max(position, rect.min.x);
            float right = Mathf.Min(position + width, rect.max.x);
            if (right - 2 <= left)
                return false;

            Rect keyRect = new Rect(left, y, right - left + 1, height);
            GUI.backgroundColor = GetKeyColor(key, pressed);
            GUIStyle style = GUI.skin.box;
            style.padding = new RectOffset(0, 0, 0, 0);
            style.border = new RectOffset(1, 1, 1, 1);
            style.overflow = new RectOffset(0, 0, 0, 0);
            GUI.Box(keyRect, GUIContent.none, style);
            return true;
        }

        void DrawKeys(Rect rect, bool blackKeys, HashSet<int> pressedNotes)
        {
            for (int key = middleKey; ValidKey(key); ++key)
            {
                bool pressed = pressedNotes != null && pressedNotes.Contains(key);
                if (blackKeys == IsBlackKey(key))
                    DrawKey(key, rect, pressed);
            }

            for (int key = middleKey - 1; ValidKey(key); --key)
            {
                bool pressed = pressedNotes != null && pressedNotes.Contains(key);
                if (blackKeys == IsBlackKey(key))
                    DrawKey(key, rect, pressed);
            }
        }

        public void DrawKeyboard(Rect rect, HashSet<int> pressedNotes = null)
        {
            rect = new Rect(rect.x - leftGrowth, rect.y,
                            rect.width + leftGrowth + rightGrowth, rect.height);

            DrawKeys(rect, false, pressedNotes);
            DrawKeys(rect, true, pressedNotes);
        }
    }
}
