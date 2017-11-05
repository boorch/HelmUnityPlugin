// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;
using System.IO;

namespace AudioHelm
{
    [CustomEditor(typeof(HelmSequencer))]
    class HelmSequencerUI : Editor
    {
        const float keyboardWidth = 30.0f;
        const float scrollWidth = 15.0f;

        SequencerUI sequencer = new SequencerUI(keyboardWidth, scrollWidth + 1);
        SequencerPositionUI sequencerPosition = new SequencerPositionUI(keyboardWidth, scrollWidth);
        SerializedProperty channel;
        SerializedProperty length;
        SerializedProperty zoom;
        SerializedProperty division;
        SerializedProperty allNotes;

        float positionHeight = 10.0f;
        float sequencerHeight = 440.0f;
        const float minWidth = 200.0f;

        void OnEnable()
        {
            channel = serializedObject.FindProperty("channel");
            length = serializedObject.FindProperty("length");
            division = serializedObject.FindProperty("division");
            allNotes = serializedObject.FindProperty("allNotes");
            zoom = serializedObject.FindProperty("zoom");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Color prev_color = GUI.backgroundColor;
            GUILayout.Space(5f);
            HelmSequencer helmSequencer = target as HelmSequencer;
            Rect sequencerPositionRect = GUILayoutUtility.GetRect(minWidth, positionHeight, GUILayout.ExpandWidth(true));
            Rect rect = GUILayoutUtility.GetRect(minWidth, sequencerHeight, GUILayout.ExpandWidth(true));

            float startWindow = sequencer.GetMinVisibleTime() / length.intValue;
            float endWindow = sequencer.GetMaxVisibleTime(rect.width) / length.intValue;
            sequencerPosition.DrawSequencerPosition(sequencerPositionRect, helmSequencer, startWindow, endWindow);

            if (rect.height == sequencerHeight)
                sequencer.DrawSequencer(rect, helmSequencer, zoom.floatValue, allNotes);
            
            if (sequencer.DoSequencerEvents(rect, helmSequencer, allNotes))
                Repaint();
            
            GUILayout.Space(5f);
            GUI.backgroundColor = prev_color;

            if (GUILayout.Button(new GUIContent("Clear Sequencer", "Remove all notes from the sequencer.")))
            {
                Undo.RecordObject(helmSequencer, "Clear Sequencer");
                helmSequencer.Clear();
            }

            if (GUILayout.Button(new GUIContent("Load MIDI File [BETA]", "Load a MIDI sequence into this sequencer.")))
            {
                string path = EditorUtility.OpenFilePanel("Load MIDI Sequence", "", "mid");
                if (path.Length != 0)
                {
                    Undo.RecordObject(helmSequencer, "Load MIDI File");
                    helmSequencer.ReadMidiFile(new FileStream(path, FileMode.Open, FileAccess.Read));
                }
            }

            EditorGUILayout.IntSlider(channel, 0, Utils.kMaxChannels - 1);
            EditorGUILayout.IntSlider(length, 1, Sequencer.kMaxLength);
            EditorGUILayout.PropertyField(division);
            EditorGUILayout.Slider(zoom, 0.0f, 1.0f);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
