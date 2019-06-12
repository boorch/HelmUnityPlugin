// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;

namespace AudioHelm
{
    [CustomEditor(typeof(HelmAudioInit))]
    class HelmAudioInitUI : Editor
    {
        SerializedObject serialized;
        SerializedProperty synthesizerGroup;
        SerializedProperty spatializerGroup;

        const float kMinBpm = 20.0f;
        const float kMaxBpm = 400.0f;

        void OnEnable()
        {
            serialized = new SerializedObject(target);
            synthesizerGroup = serialized.FindProperty("synthesizerMixerGroup");
            spatializerGroup = serialized.FindProperty("spatializerMixerGroup");
        }

        public override void OnInspectorGUI()
        {
            HelmAudioInit audioInit = target as HelmAudioInit;
            AudioSource audioSource = audioInit.GetComponent<AudioSource>();
            audioInit.spatialize = EditorGUILayout.Toggle("Spatialize", audioInit.spatialize);
            serialized.Update();

            if (audioInit.spatialize)
            {
                audioInit.spatializeWithPlugin = EditorGUILayout.Toggle("With Plugin", audioInit.spatializeWithPlugin);
                EditorGUILayout.PropertyField(synthesizerGroup);
                EditorGUILayout.PropertyField(spatializerGroup);
            }
            serialized.ApplyModifiedProperties();
        }
    }
}
