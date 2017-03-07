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
        Vector2 mousePosition = Vector2.zero;
        GUIStyle rowStyle;
        FileInfo[] files;
        int lastSelectedIndex = -1;

        const float rowHeight = 22.0f;
        const int rightPadding = 15;

        public PatchBrowserUI()
        {
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

        FileInfo[] GetAllPatches()
        {
            DirectoryInfo directory = new DirectoryInfo(GetFullPatchesPath());
            return directory.GetFiles("*.helm");
        }

        void ReloadPatches()
        {
            FileInfo[] newFiles = GetAllPatches();
            if (newFiles.Length != files.Length)
                lastSelectedIndex = -1;
            files = newFiles;
        }

        public void DoBrowserEvents(IAudioEffectPlugin plugin, Rect rect)
        {
            Event evt = Event.current;
            mousePosition = evt.mousePosition;
            if (evt.type == EventType.MouseDown && rect.Contains(mousePosition))
            {
                FileInfo[] files = GetAllPatches();
                int index = GetPatchIndex(rect, evt.mousePosition);
                if (files.Length > index && index >= 0)
                {
                    lastSelectedIndex = index;
                    LoadPatch(plugin, files[index].FullName);
                }

                ReloadPatches();
            }
        }

        int GetPatchIndex(Rect guiRect, Vector2 mousePosition)
        {
            Rect rect = new Rect(guiRect);
            rect.width -= rightPadding;
            if (!rect.Contains(mousePosition))
                return -1;
            Vector2 localPosition = mousePosition - guiRect.position + scrollPosition;
            return (int)Mathf.Floor((localPosition.y / rowHeight));
        }

        public void DrawBrowser(Rect rect)
        {
            Color previousColor = GUI.color;
            Color colorEven = new Color(0.8f, 0.8f, 0.8f);
            Color colorOdd = new Color(0.9f, 0.9f, 0.9f);
            Color colorHover = Color.white;

            float rowWidth = rect.width - rightPadding;
            Rect scrollableArea = new Rect(0, 0, rowWidth, files.Length * rowHeight);
            scrollPosition = GUI.BeginScrollView(rect, scrollPosition, scrollableArea, false, true);

            int hoverIndex = GetPatchIndex(rect, mousePosition);

            float y = 0.0f;
            int index = 0;
            foreach (FileInfo file in files)
            {
                if (index == hoverIndex)
                    GUI.color = colorHover;
                else if (index % 2 == 0)
                    GUI.color = colorEven;
                else
                    GUI.color = colorOdd;

                if (lastSelectedIndex == index)
                    rowStyle.fontStyle = FontStyle.Bold;
                else
                    rowStyle.fontStyle = FontStyle.Normal;

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
