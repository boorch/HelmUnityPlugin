// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;

namespace Helm
{
    [CustomEditor(typeof(Sampler))]
    class SamplerUI : Editor
    {
        const int scrollWidth = 15;

        KeyboardUI keyboard = new KeyboardUI();
        KeyzoneEditorUI keyzones = new KeyzoneEditorUI(scrollWidth);
        SerializedProperty numVoices;
        SerializedProperty velocityTracking;

        const int keyzoneHeight = 120;
        const float minWidth = 200.0f;
        const float keyboardHeight = 60.0f;

        void OnEnable()
        {
            numVoices = serializedObject.FindProperty("numVoices");
            velocityTracking = serializedObject.FindProperty("velocityTracking");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Color prev_color = GUI.backgroundColor;
            GUILayout.Space(5f);
            Sampler sampler = target as Sampler;
            int height = Mathf.Max(keyzoneHeight, keyzones.GetHeight(sampler));

            Rect keyboardRect = GUILayoutUtility.GetRect(minWidth, keyboardHeight, GUILayout.ExpandWidth(true));
            GUILayout.Space(10.0f);
            Rect keyzonesRect = GUILayoutUtility.GetRect(minWidth, height, GUILayout.ExpandWidth(true));

            if (keyboard.DoKeyboardEvents(keyboardRect, sampler))
                Repaint();

            if (keyzones.DoKeyzoneEvents(keyzonesRect, sampler))
                Repaint();

            if (keyzonesRect.height == height)
                keyzones.DrawKeyzones(keyzonesRect, sampler);

            keyboard.DrawKeyboard(keyboardRect);

            GUILayout.Space(5f);
            GUI.backgroundColor = prev_color;

            EditorGUILayout.IntSlider(numVoices, 1, 16);
            EditorGUILayout.Slider(velocityTracking, 0.0f, 1.0f);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
