using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MapSceneMerger
{
    private const string SamplePath = "Assets/Scenes/SampleScene.unity";
    private const string MapPath = "Assets/Scenes/MapScene.unity";
    private const string BackupPath = "Assets/Scenes/SampleScene.before-map-merge.unity.bak";
    private static readonly string[] MapRoots =
    {
        "Grid",
        "Foliage - Ground Scatter",
        "Scene Dressing - Roads and Greenery"
    };

    [MenuItem("Tools/Hive Zombie/Merge MapScene Into SampleScene")]
    private static void Merge()
    {
        if (!File.Exists(SamplePath) || !File.Exists(MapPath))
        {
            Debug.LogError("Both MapScene and SampleScene must exist in Assets/Scenes.");
            return;
        }

        if (File.Exists(BackupPath))
        {
            Debug.LogError("The pre-merge backup already exists; refusing to overwrite it: " + BackupPath);
            return;
        }

        File.Copy(SamplePath, BackupPath);
        Scene sample = default;
        Scene map = default;
        try
        {
            sample = EditorSceneManager.OpenScene(SamplePath, OpenSceneMode.Single);
            map = EditorSceneManager.OpenScene(MapPath, OpenSceneMode.Additive);

            var sampleGroups = MapRoots.Select(name => RequireSingleRoot(sample, name)).ToArray();
            var mapGroups = MapRoots.Select(name => RequireSingleRoot(map, name)).ToArray();

            foreach (var root in sampleGroups)
                UnityEngine.Object.DestroyImmediate(root);

            foreach (var root in mapGroups)
                EditorSceneManager.MoveGameObjectToScene(root, sample);

            EditorSceneManager.SetActiveScene(sample);
            if (!EditorSceneManager.SaveScene(sample))
                throw new InvalidOperationException("Unity did not save SampleScene.");

            EditorSceneManager.CloseScene(map, true);
            Debug.Log("MapScene map groups merged into SampleScene. Player, follower, camera, and UI roots were preserved. Backup: " + BackupPath);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            if (File.Exists(BackupPath))
                File.Copy(BackupPath, SamplePath, true);
            if (map.IsValid() && map.isLoaded)
                EditorSceneManager.CloseScene(map, true);
            EditorSceneManager.OpenScene(SamplePath, OpenSceneMode.Single);
        }
    }

    private static GameObject RequireSingleRoot(Scene scene, string name)
    {
        var matches = scene.GetRootGameObjects().Where(root => root.name == name).ToArray();
        if (matches.Length != 1)
            throw new InvalidOperationException($"Expected one root named '{name}' in {scene.name}; found {matches.Length}.");
        return matches[0];
    }
}
