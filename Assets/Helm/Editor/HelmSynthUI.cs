// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Tytel
{
    public class HelmSynthUI : IAudioEffectPluginGUI
    {
        KeyboardUI keyboard = new KeyboardUI();
        PatchBrowserUI patchBrowser = new PatchBrowserUI();
        bool showOptions = false;

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
            Color prev_color = GUI.backgroundColor;

            GUILayout.Space(5.0f);
            Rect keyboardRect = GUILayoutUtility.GetRect(200, 60, GUILayout.ExpandWidth(true));

            float channel = 0.0f;
            plugin.GetFloatParameter("Channel", out channel);
            keyboard.DoKeyboardEvents(keyboardRect, (int)channel);
            keyboard.DrawKeyboard(keyboardRect);

            GUI.backgroundColor = prev_color;
            GUILayout.Space(5.0f);
            Rect browserRect = GUILayoutUtility.GetRect(200, 120, GUILayout.ExpandWidth(true));
            browserRect.x -= 14.0f;
            browserRect.width += 18.0f;
            patchBrowser.DoBrowserEvents(plugin, browserRect);
            patchBrowser.DrawBrowser(browserRect);

            GUILayout.Space(5.0f);
            GUI.backgroundColor = prev_color;

            float newChannel = EditorGUILayout.IntSlider("Channel", (int)channel, 0, Utils.kMaxChannels - 1);
            showOptions = EditorGUILayout.Toggle("Show All Options", showOptions);

            if (newChannel != channel)
                plugin.SetFloatParameter("Channel", newChannel);

            return showOptions;
        }
    }
}
