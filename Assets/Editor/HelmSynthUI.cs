using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Tytel
{
    public class HelmSynthUI : IAudioEffectPluginGUI
    {
        KeyboardUI keyboard = new KeyboardUI();

        public override string Name
        {
            get { return "Helm"; }
        }

        public override string Description
        {
            get { return "Audio plugin for live synthesis in Unity"; }
        }

        public override string Vendor
        {
            get { return "Matt Tytel"; }
        }

        public override bool OnGUI(IAudioEffectPlugin plugin)
        {
            float channel = 0.0f;
            plugin.GetFloatParameter("Channel", out channel);

            GUILayout.Space(5.0f);
            Rect rect = GUILayoutUtility.GetRect(200, 60, GUILayout.ExpandWidth(true));
            keyboard.DoKeyboardEvents(rect, (int)channel);

            Color prev_color = GUI.backgroundColor;
            keyboard.DrawKeyboard(rect);

            GUILayout.Space(5.0f);
            GUI.backgroundColor = prev_color;
            return true;
        }
    }
}
