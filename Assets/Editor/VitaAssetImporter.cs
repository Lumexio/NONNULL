using UnityEditor;
using UnityEngine;

public class VitaAssetImporter : AssetPostprocessor {
    void OnPreprocessTexture() {
        if (!assetPath.Contains("GodotAssets")) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureCompression = TextureImporterCompression.Compressed;
        
        // Setup for PS Vita (PSP2)
        TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
        settings.overridden = true;
        settings.name = "PSP2"; 
        settings.format = TextureImporterFormat.ETC2_RGBA8;
        settings.maxTextureSize = 256; // Optimization limit from spec
        
        importer.SetPlatformTextureSettings(settings);
    }

    void OnPostprocessModel(GameObject g) {
        if (!assetPath.Contains("GodotAssets")) return;

        Renderer[] renderers = g.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) {
            foreach (Material m in r.sharedMaterials) {
                if (m != null) {
                    m.shader = Shader.Find("Unlit/Texture");
                }
            }
        }
    }
}
