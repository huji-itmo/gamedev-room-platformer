using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BakeLighting
{
    [MenuItem("Tools/Bake Lighting (Mobile)")]
    public static void BakeForMobile()
    {
        LightmapEditorSettings.bakeResolution = 20f;
        LightmapEditorSettings.maxAtlasSize = 512;
        LightmapEditorSettings.textureCompression = true;
        LightmapEditorSettings.enableAmbientOcclusion = true;
        LightmapEditorSettings.aoMaxDistance = 1f;
        LightmapEditorSettings.aoContrast = 1f;

        var scenes = EditorBuildSettings.scenes;
        foreach (var scene in scenes)
        {
            if (!scene.enabled) continue;
            EditorSceneManager.OpenScene(scene.path);
            Lightmapping.Bake();
        }
        Debug.Log("Lighting bake complete for all scenes.");
    }
}
