using UnityEditor;
using UnityEngine;

namespace Tytel
{
    [CustomEditor(typeof(HelmSequencer))]
    class HelmSequencerUI : Editor
    {
        private SerializedObject serialized;
        SequencerUI sequencer = new SequencerUI();
        SerializedProperty allNotes;
        SerializedProperty channel;

        void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            Color prev_color = GUI.backgroundColor;
            GUILayout.Space(5f);

            HelmSequencer helmSequencer = target as HelmSequencer;
            Rect rect = GUILayoutUtility.GetRect(200, 300, GUILayout.ExpandWidth(true));

            if (sequencer.DoSequencerEvents(rect, helmSequencer))
                Repaint();

            if (rect.height == 300)
                sequencer.DrawSequencer(rect, helmSequencer);
            GUILayout.Space(5f);
            GUI.backgroundColor = prev_color;

            helmSequencer.channel = EditorGUILayout.IntSlider("Channel", helmSequencer.channel, 0, Utils.kMaxChannels - 1);
        }
    }
}
