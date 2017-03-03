using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;

public class HelmControllerUI : IAudioEffectPluginGUI {

    [DllImport("AudioPluginHelm")]
    private static extern void HelmNoteOn(int channel, int note);

    [DllImport("AudioPluginHelm")]
    private static extern void HelmNoteOff(int channel, int note);

    Color black_unpressed = Color.black;
    Color black_pressed = Color.blue;
    Color white_unpressed = Color.white;
    Color white_pressed = Color.yellow;

    float key_width = 15.0f;
    float vertical_stagger = 5.0f;
    int middle_key = 60;
    int midi_size = 128;
    int max_polyphony = 8;
    int current_key = -1;
    bool[] black_keys = new bool[] { false, true, false, true, false,
                                     false, true, false, true, false, true, false };

    public override string Name {
        get { return "Helm"; }
    }

    public override string Description {
        get { return "Helm plugin for live audio synthesis in Unity"; }
    }

    public override string Vendor {
        get { return "Matt Tytel"; }
    }

    void DoKeyboardEvents(IAudioEffectPlugin plugin, Rect rect) {
        float channel = 0.0f;
        plugin.GetFloatParameter("Channel", out channel);
        Event evt = Event.current;
        if (evt.type == EventType.MouseUp) {
            if (current_key >= 0)
                HelmNoteOff((int)channel, current_key);
            current_key = -1;
        }
        else if (rect.Contains(evt.mousePosition) &&
                (evt.type == EventType.MouseDrag || evt.type == EventType.MouseDown)) {
            int hovered = GetHoveredKey(evt.mousePosition.x, rect);
            if (hovered != current_key) {
                if (current_key >= 0)
                    HelmNoteOff((int)channel, current_key);
                HelmNoteOn((int)channel, hovered);
                current_key = hovered;
            }
        }
    }

    int GetHoveredKey(float position, Rect rect) {
        float key_offset = (position - rect.center.x + key_width / 2.0f) / key_width;
        return (int)(middle_key + key_offset);
    }

    float GetKeyXPosition(int key, Rect rect) {
        float offset = key_width * (key - middle_key) - (key_width / 2.0f);
        return rect.center.x + offset;
    }

    bool IsBlackKey(int key) {
        return black_keys[key % black_keys.Length];
    }

    Color GetKeyColor(int key) {
        if (IsBlackKey(key)) {
            if (key == current_key)
                return black_pressed;
            else
                return black_unpressed;
        }
        if (key == current_key)
            return white_pressed;
        return white_unpressed;
    }

    bool DrawKey(int key, Rect rect) {
        if (key < 0 || key >= midi_size)
            return false;

        float position = GetKeyXPosition(key, rect);
        float left = Mathf.Max(position, rect.min.x);
        float right = Mathf.Min(position + key_width, rect.max.x);
        if (right - 2 <= left)
            return false;

        float y = rect.y;
        if (!IsBlackKey(key))
            y = rect.y + vertical_stagger;
        Rect key_rect = new Rect(left, y, right - left, rect.height - vertical_stagger);
        GUI.backgroundColor = GetKeyColor(key);
        GUI.Box(key_rect, GUIContent.none);
        return true;
    }

    public void DrawKeyboard(Rect rect) {
        rect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);

        for (int key = middle_key; DrawKey(key, rect); ++key)
            ;

        for (int key = middle_key - 1; DrawKey(key, rect); --key)
            ;
    }

    public override bool OnGUI(IAudioEffectPlugin plugin)  {
        Color prev_color = GUI.backgroundColor;
        GUILayout.Space(5f);
        Rect rect = GUILayoutUtility.GetRect(200, 50, GUILayout.ExpandWidth(true));
        DoKeyboardEvents(plugin, rect);
        DrawKeyboard(rect);
        GUILayout.Space(5f);
        GUI.backgroundColor = prev_color;
        return true;
    }
}
