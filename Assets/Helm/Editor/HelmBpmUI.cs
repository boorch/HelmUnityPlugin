// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;

namespace Tytel
{
    [CustomEditor(typeof(HelmBpm))]
    class HelmBpmUI : Editor
    {
        private SerializedObject serialized;
        private SerializedProperty bpm;

        const float kMinBpm = 20.0f;
        const float kMaxBpm = 400.0f;

        void OnEnable()
        {
            serialized = new SerializedObject(target);
            bpm = serialized.FindProperty("bpm_");
        }

        public override void OnInspectorGUI()
        {
            serialized.Update();
            EditorGUI.BeginChangeCheck();
            bpm.floatValue = EditorGUILayout.Slider("BPM", bpm.floatValue, kMinBpm, kMaxBpm);
            serialized.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                HelmBpm helmBpm = target as HelmBpm;
                helmBpm.SetNative();
            }
        }
    }
}
