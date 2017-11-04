using UnityEditor;
using UnityEngine;
using System.IO;

namespace AudioHelm
{
    public class NonNativeFileImporter : AssetPostprocessor
    {
        const string streamingFolder = "StreamingAssets/AudioHelm/";

        static bool ShouldImportFile(string asset)
        {
            return asset.ToLower().EndsWith(".mid") | asset.ToLower().EndsWith(".helm");
        }

        static string GetFolderPath()
        {
            return Application.dataPath + "/" + streamingFolder;
        }

        static void CreateFolderPath()
        {
            if (!Directory.Exists(GetFolderPath()))
                Directory.CreateDirectory(GetFolderPath());
        }

        static string GetFilePath(string asset)
        {
            string extension = Path.GetExtension(asset).ToLower().Substring(1);
            return GetFolderPath() + extension + "_" + Path.GetFileNameWithoutExtension(asset);
        }

        static void ImportNonNativeFile(string asset)
        {
            if (asset.Contains(streamingFolder))
                return;
            
            string newFilePath = GetFilePath(asset);

            File.Copy(asset, newFilePath, true);
            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }

        static void TryDelete(string asset)
        {
            CreateFolderPath();
            if (ShouldImportFile(asset))
                File.Delete(GetFilePath(asset));
        }

        static void TryCreate(string asset)
        {
            CreateFolderPath();
            if (ShouldImportFile(asset))
                ImportNonNativeFile(asset);
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in deletedAssets)
                TryDelete(asset);

            foreach (string asset in movedFromAssetPaths)
                TryDelete(asset);

            foreach (string asset in movedAssets)
                TryCreate(asset);

            foreach (string asset in importedAssets)
                TryCreate(asset);
        }
    }
}
