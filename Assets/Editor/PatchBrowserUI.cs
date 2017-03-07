using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace Tytel
{
    public class PatchBrowserUI
    {
        const string patchesPath = "/Helm/Patches/";

        void LoadPatch(IAudioEffectPlugin plugin, string name)
        {
            string patchText = File.ReadAllText(Application.dataPath + patchesPath + name);
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

        public bool DoBrowserEvents(IAudioEffectPlugin plugin, Rect rect)
        {
            LoadPatch(plugin, "gold.helm");
            return false;
        }

        public void DrawBrowser(Rect rect)
        {

        }
    }
}
