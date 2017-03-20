// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;

namespace Tytel
{
    [CustomEditor(typeof(HelmSequencer))]
    class HelmSequencerUI : Editor
    {
        const float keyboardWidth = 30.0f;
        const float scrollWidth = 15.0f;

        private SerializedObject serialized;
        SequencerUI sequencer = new SequencerUI(keyboardWidth, scrollWidth + 1);
        SequencerPositionUI sequencerPosition = new SequencerPositionUI(keyboardWidth, scrollWidth);
        SequencerVelocityUI velocities = new SequencerVelocityUI(keyboardWidth, scrollWidth);
        SerializedProperty channel;
        SerializedProperty length;

        float positionHeight = 10.0f;
        float velocitiesHeight = 40.0f;
        float sequencerHeight = 400.0f;
        const float minWidth = 200.0f;

        void OnEnable()
        {
            channel = serializedObject.FindProperty("channel");
            length = serializedObject.FindProperty("length");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Color prev_color = GUI.backgroundColor;
            GUILayout.Space(5f);
            HelmSequencer helmSequencer = target as HelmSequencer;
            Rect sequencerPositionRect = GUILayoutUtility.GetRect(minWidth, positionHeight, GUILayout.ExpandWidth(true));
            Rect rect = GUILayoutUtility.GetRect(minWidth, sequencerHeight, GUILayout.ExpandWidth(true));
            Rect velocitiesRect = GUILayoutUtility.GetRect(minWidth, velocitiesHeight, GUILayout.ExpandWidth(true));

            if (sequencer.DoSequencerEvents(rect, helmSequencer))
                Repaint();
            if (velocities.DoVelocityEvents(velocitiesRect, helmSequencer))
                Repaint();

            sequencerPosition.DrawSequencerPosition(sequencerPositionRect, helmSequencer);
            velocities.DrawSequencerPosition(velocitiesRect, helmSequencer);

            if (rect.height == sequencerHeight)
                sequencer.DrawSequencer(rect, helmSequencer);
            GUILayout.Space(5f);
            GUI.backgroundColor = prev_color;

            if (GUILayout.Button("Clear Sequencer"))
                helmSequencer.Clear();

            EditorGUILayout.IntSlider(channel, 0, Utils.kMaxChannels - 1);
            EditorGUILayout.IntSlider(length, 1, HelmSequencer.kMaxLength);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
