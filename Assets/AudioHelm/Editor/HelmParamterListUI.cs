// Copyright 2017 Matt Tytel

using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioHelm
{
    public class HelmParameterListUI
    {
        const int rowHeight = 24;
        const int buttonHeight = 15;
        const int buttonBuffer = 17;
        const int addButtonHeight = 20;
        const int sliderWidth = 150;
        const int sliderBuffer = 10;

        void DrawParameterList(HelmController controller, SerializedObject serialized, float width)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.padding = new RectOffset(0, 0, 0, 0);
            style.fontSize = buttonHeight - 4;
            int y = addButtonHeight + sliderBuffer;
            int extra_y = (rowHeight - buttonHeight) / 3;

            HelmParameter remove = null;

            int paramIndex = 0;
            foreach (HelmParameter synthParameter in controller.synthParameters)
            {
                Rect buttonRect = new Rect(0, y + extra_y, buttonHeight, buttonHeight);
                Rect paramRect = new Rect(buttonRect.xMax, y + extra_y, sliderWidth - buttonRect.width, buttonHeight);
                Rect sliderRect = new Rect(paramRect.xMax + sliderBuffer, y + extra_y, width - sliderWidth, buttonHeight);

                if (GUI.Button(buttonRect, "X", style))
                    remove = synthParameter;

                System.Enum param = EditorGUI.EnumPopup(paramRect, synthParameter.parameter);
                SerializedProperty paramProperty = serialized.FindProperty("synthParamValue" + paramIndex);
                EditorGUI.Slider(sliderRect, paramProperty, 0.0f, 1.0f, "");

                if (param != (System.Enum)synthParameter.parameter)
                {
                    Undo.RecordObject(controller, "Change Parameter Control");
                    synthParameter.parameter = (Param)param;
                }
                y += rowHeight;

                paramIndex++;
            }

            if (remove != null)
            {
                Undo.RecordObject(controller, "Remove Parameter Control");
                controller.RemoveParameter(remove);
            }

            controller.UpdateAllParameters();
        }

        public int GetHeight(HelmController controller)
        {
            return addButtonHeight + rowHeight * controller.synthParameters.Count + sliderBuffer;
        }

        public void DrawParameters(Rect rect, HelmController controller, SerializedObject serialized)
        {
            GUI.BeginGroup(rect);
            DrawParameterList(controller, serialized, rect.width);
            Rect buttonRect = new Rect(rect.width / 4, 0, rect.width / 2, addButtonHeight);
            if (GUI.Button(buttonRect, "Add Parameter Control"))
            {
                Undo.RecordObject(controller, "Add Parameter Control");
                controller.AddEmptyParameter();
            }

            GUI.EndGroup();
        }
    }
}
