// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioHelm
{
    public class HelmParameterListUI
    {
        const int rowHeight = 32;
        const int buttonBuffer = 17;
        const int addButtonHeight = 20;
        const int sliderWidth = 150;

        void DrawParameterList(HelmController controller, SerializedProperty synthParameters)
        {
            int height = rowHeight / 2;
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.fontSize = height - 4;
            int y = addButtonHeight;

            HelmParameter remove = null;

            foreach (HelmParameter synthParameter in controller.synthParameters)
            {
                Rect buttonRect = new Rect(0, y, height, height);
                Rect paramRect = new Rect(buttonRect.xMax, y, sliderWidth - buttonRect.width, height);

                if (GUI.Button(buttonRect, "X", style))
                    remove = synthParameter;

                System.Enum param = EditorGUI.EnumPopup(paramRect, synthParameter.parameter);

                if (param != (System.Enum)synthParameter.parameter)
                {
                    Undo.RecordObject(controller, "Change Parameter Control");
                    synthParameter.parameter = (Param)param;
                }
                y += rowHeight;
            }

            if (remove != null)
            {
                Undo.RecordObject(controller, "Remove Parameter Control");
                int index = controller.RemoveParameter(remove);
                if (index >= 0)
                    synthParameters.DeleteArrayElementAtIndex(index);
            }
        }


        public int GetHeight(HelmController controller)
        {
            return addButtonHeight + rowHeight * controller.synthParameters.Count;
        }

        void AddParameter(HelmController controller, SerializedProperty synthParameters)
        {
            HelmParameter synthParameter = controller.AddEmptyParameter();

            synthParameters.arraySize++;
            SerializedProperty newParameter = synthParameters.GetArrayElementAtIndex(synthParameters.arraySize - 1);
            newParameter.FindPropertyRelative("parent").objectReferenceValue = synthParameter.parent;
            newParameter.FindPropertyRelative("parameter").intValue = (int)synthParameter.parameter;
        }

        public void DrawParameters(Rect rect, HelmController controller, SerializedProperty synthParameters)
        {
            GUI.BeginGroup(rect);
            DrawParameterList(controller, synthParameters);
            Rect buttonRect = new Rect(rect.width / 4, 0, rect.width / 2, addButtonHeight);
            if (GUI.Button(buttonRect, "Add Parameter Control"))
            {
                Undo.RecordObject(controller, "Add Parameter Control");
                AddParameter(controller, synthParameters);
            }

            GUI.EndGroup();
        }
    }
}
