// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;

namespace Helm
{
    [CustomEditor(typeof(Sampler))]
    class SamplerUI : Editor
    {
        const float scrollWidth = 15.0f;

        private SerializedObject serialized;
        KeyzoneUI keyzones = new KeyzoneUI(scrollWidth);
        SerializedProperty numVoices;
        SerializedProperty velocityTracking;

        const float keyzoneHeight = 200.0f;
        const float minWidth = 200.0f;

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
            Rect keyzonesRect = GUILayoutUtility.GetRect(minWidth, keyzoneHeight, GUILayout.ExpandWidth(true));

            if (keyzones.DoKeyzoneEvents(keyzonesRect, sampler))
                Repaint();

            if (keyzonesRect.height == keyzoneHeight)
                keyzones.DrawKeyzones(keyzonesRect, sampler);

            GUILayout.Space(5f);
            GUI.backgroundColor = prev_color;

            EditorGUILayout.IntSlider(numVoices, 1, 16);
            EditorGUILayout.Slider(velocityTracking, 0.0f, 1.0f);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
