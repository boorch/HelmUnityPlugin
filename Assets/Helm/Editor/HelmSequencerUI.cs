using UnityEditor;
using UnityEngine;

namespace Tytel
{
    [CustomEditor(typeof(HelmSequencer))]
    class HelmSequencerUI : Editor
    {
        private SerializedObject serialized;
        SequencerUI sequencer = new SequencerUI();
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

            HelmSequencer helmSequencer = target as HelmSequencer;
            Rect rect = GUILayoutUtility.GetRect(200, 300, GUILayout.ExpandWidth(true));

            if (sequencer.DoSequencerEvents(rect, helmSequencer))
                Repaint();

            sequencer.DrawSequencer(rect, helmSequencer);
            GUILayout.Space(5f);
            GUI.backgroundColor = prev_color;

            channel.intValue = EditorGUILayout.IntSlider("Channel", channel.intValue, 0, Utils.kMaxChannels - 1);
            serialized.ApplyModifiedProperties();
        }
    }
}
