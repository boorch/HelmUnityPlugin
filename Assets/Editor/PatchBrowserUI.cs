using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Tytel
{
    public class PatchBrowserUI
    {
        Vector2 scrollPosition = Vector2.zero;
        const float rowHeight = 22.0f;
        const int rightPadding = 15;
        GUIStyle rowStyle;

        FileInfo[] files;

        public PatchBrowserUI() {
            rowStyle = new GUIStyle(GUI.skin.box);
            rowStyle.alignment = TextAnchor.MiddleLeft;
            rowStyle.padding = new RectOffset(10, 10, 0, 0);
            rowStyle.border = new RectOffset(11, 11, 2, 2);

            files = GetAllPatches();
        }

        string GetFullPatchesPath()
        {
            const string patchesPath = "/Helm/Patches/";
            return Application.dataPath + patchesPath;
        }

        void LoadPatch(IAudioEffectPlugin plugin, string path)
        {
            string patchText = File.ReadAllText(path);
            HelmPatchFormat patch = JsonUtility.FromJson<HelmPatchFormat>(patchText);

            FieldInfo[] fields = typeof(HelmPatchSettings).GetFields();

            foreach (FieldInfo field in fields)
            {
                if (!field.FieldType.IsArray)
               {
                    float value = (float)field.GetValue(patch.settings);
                    plugin.SetFloatParameter(field.Name, value);
                }
            }
        }

        FileInfo[] GetAllPatches() {
            DirectoryInfo directory = new DirectoryInfo(GetFullPatchesPath());
            return directory.GetFiles("*.helm");
        }

        public bool DoBrowserEvents(IAudioEffectPlugin plugin, Rect rect)
        {
            Event evt = Event.current;
            if (evt.type == EventType.MouseDown && rect.Contains(evt.mousePosition))
            {
                Vector2 localPosition = evt.mousePosition - rect.position + scrollPosition;
                FileInfo[] files = GetAllPatches();
                int index = (int)(localPosition.y / rowHeight);
                if (files.Length > index)
                {
                    LoadPatch(plugin, files[index].FullName);
                    return true;
                }
            }
            return false;
        }

        public void DrawBrowser(Rect rect)
        {
            Color previousColor = GUI.color;
            Color colorEven = new Color(0.8f, 0.8f, 0.8f);
            Color colorOdd = new Color(0.9f, 0.9f, 0.9f);

            float rowWidth = rect.width - rightPadding;
            Rect scrollableArea = new Rect(0, 0, rowWidth, files.Length * rowHeight);
            scrollPosition = GUI.BeginScrollView(rect, scrollPosition, scrollableArea, false, true);

            float y = 0.0f;
            int index = 0;
            foreach (FileInfo file in files)
            {
                if (index % 2 == 0 )
                    GUI.color = colorEven;
                else
                    GUI.color = colorOdd;

                string name = Path.GetFileNameWithoutExtension(file.Name);
                GUI.Label(new Rect(0, y, rowWidth, rowHeight + 1), name, rowStyle);
                y += rowHeight;
                index++;
            }
            GUI.EndScrollView();
            GUI.color = previousColor;
        }
    }
}
