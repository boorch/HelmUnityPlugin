using UnityEditor;
using UnityEngine;

namespace Tytel
{
    [CustomEditor(typeof(HelmController))]
    class HelmControllerUI : Editor
    {
        private SerializedObject serialized;
        KeyboardUI keyboard = new KeyboardUI();
        SerializedProperty channel;

        void OnEnable()
        {
            serialized = new SerializedObject(target);
            channel = serialized.FindProperty("channel");
        }

        public override void OnInspectorGUI()
        {
            serialized.Update();
            Color prev_color = GUI.backgroundColor;
            GUILayout.Space(5f);
            Rect rect = GUILayoutUtility.GetRect(200, 60, GUILayout.ExpandWidth(true));

            if (keyboard.DoKeyboardEvents(rect, channel.intValue))
                Repaint();

            HelmController controller = target as HelmController;
            keyboard.DrawKeyboard(rect, controller.GetPressedNotes());
            GUILayout.Space(5f);
            GUI.backgroundColor = prev_color;

            channel.intValue = EditorGUILayout.IntSlider("Channel", channel.intValue, 0, Utils.kMaxChannels - 1);
            serialized.ApplyModifiedProperties();
        }
    }
}
