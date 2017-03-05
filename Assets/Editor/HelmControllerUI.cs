using UnityEditor;
using UnityEngine;

namespace Tytel {

    [CustomEditor(typeof(HelmController))]
    class HelmControllerUI : Editor {

        private SerializedObject serialized;
        KeyboardUI keyboard = new KeyboardUI();
        SerializedProperty channel;

        const int MAX_CHANNELS = 16;

        void OnEnable() {
            serialized = new SerializedObject(target);
            channel = serialized.FindProperty("channel");
        }

        public override void OnInspectorGUI() {
            serialized.Update();
            Color prev_color = GUI.backgroundColor;
            GUILayout.Space(5f);
            Rect rect = GUILayoutUtility.GetRect(200, 60, GUILayout.ExpandWidth(true));

            if (keyboard.DoKeyboardEvents(rect, channel.intValue))
                Repaint();

            keyboard.DrawKeyboard(rect);
            GUILayout.Space(5f);
            GUI.backgroundColor = prev_color;

            channel.intValue = EditorGUILayout.IntSlider("Channel", channel.intValue, 0, MAX_CHANNELS - 1);
            serialized.ApplyModifiedProperties();
        }
    }
}
