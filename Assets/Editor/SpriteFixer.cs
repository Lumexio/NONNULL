using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SpriteFixer {
    static SpriteFixer() {
        string[] files = { "Angry.png", "Documents Folder.png", "Empty Recycle Bin.png" };
        foreach (var f in files) {
            string path = "Assets/GodotAssets/RetroWindowsGUI/" + f;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite) {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
        }
    }
}
