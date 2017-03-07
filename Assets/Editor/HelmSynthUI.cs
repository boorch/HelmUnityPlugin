using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Tytel {
    public class HelmSynthUI : IAudioEffectPluginGUI {

        KeyboardUI keyboard = new KeyboardUI();

        public override string Name {
            get { return "Helm"; }
        }

        public override string Description {
            get { return "Audio plugin for live synthesis in Unity"; }
        }

        public override string Vendor {
            get { return "Matt Tytel"; }
        }

        void LoadPatch(IAudioEffectPlugin plugin, string name) {
            string patchText = File.ReadAllText(Application.dataPath + "/Resources/" + name);
            HelmPatchFormat patch = JsonUtility.FromJson<HelmPatchFormat>(patchText);

            FieldInfo[] fields = typeof(HelmPatchSettings).GetFields();

            foreach (FieldInfo field in fields) {
                if (!field.FieldType.IsArray) {
                    float value = (float)field.GetValue(patch.settings);
                    plugin.SetFloatParameter(field.Name, value);
                }
            }
        }

        public override bool OnGUI(IAudioEffectPlugin plugin) {
            float channel = 0.0f;
            plugin.GetFloatParameter("Channel", out channel);

            GUILayout.Space(5.0f);
            Rect rect = GUILayoutUtility.GetRect(200, 60, GUILayout.ExpandWidth(true));
            keyboard.DoKeyboardEvents(rect, (int)channel);

            Color prev_color = GUI.backgroundColor;
            keyboard.DrawKeyboard(rect);

            GUILayout.Space(5.0f);
            GUI.backgroundColor = prev_color;
            return true;
        }
    }
}
