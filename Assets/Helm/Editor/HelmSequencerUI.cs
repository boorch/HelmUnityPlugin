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
        SerializedProperty allNotes;
        SerializedProperty channel;

        float positionHeight = 10.0f;
        float sequencerHeight = 400.0f;

        Rect sequencerPositionRect;

        public override void OnInspectorGUI()
        {
            Color prev_color = GUI.backgroundColor;
            GUILayout.Space(5f);
            HelmSequencer helmSequencer = target as HelmSequencer;
            sequencerPositionRect = GUILayoutUtility.GetRect(200, positionHeight, GUILayout.ExpandWidth(true));
            Rect rect = GUILayoutUtility.GetRect(200, sequencerHeight, GUILayout.ExpandWidth(true));

            if (sequencer.DoSequencerEvents(rect, helmSequencer))
                Repaint();

            sequencerPosition.DrawSequencerPosition(sequencerPositionRect, helmSequencer);

            if (rect.height == sequencerHeight)
                sequencer.DrawSequencer(rect, helmSequencer);
            GUILayout.Space(5f);
            GUI.backgroundColor = prev_color;

            helmSequencer.channel = EditorGUILayout.IntSlider("Channel", helmSequencer.channel, 0, Utils.kMaxChannels - 1);
            helmSequencer.length = EditorGUILayout.IntSlider("Length", helmSequencer.length, 1, HelmSequencer.kMaxLength);
        }
    }
}
