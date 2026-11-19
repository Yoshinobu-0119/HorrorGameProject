using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

internal sealed class MiniMapWallLayerEditor : EditorWindow
{
    private const string TargetName = "MiniMapWall";
    private const string WindowTitle = "MiniMapWall のレイヤー変更";

    private readonly Dictionary<string, int> preview = new();
    private string selectedLayerName = "";
    private string resultMessage = "";
    private Vector2 scrollPosition;
    private bool hasPreview;

    [MenuItem("Tools/MiniMapWall のレイヤー変更")]
    private static void Open()
    {
        GetWindow<MiniMapWallLayerEditor>(WindowTitle);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField($"Assets 内の Prefab で名前が {TargetName} と完全一致するオブジェクトが対象です。", EditorStyles.wordWrappedLabel);

        var layerNames = new List<string> { "選択してください" };
        layerNames.AddRange(UnityEditorInternal.InternalEditorUtility.layers);
        int selectedIndex = Math.Max(0, layerNames.IndexOf(selectedLayerName, 1));
        int nextIndex = EditorGUILayout.Popup("変更先レイヤー", selectedIndex, layerNames.ToArray());
        if (nextIndex != selectedIndex)
        {
            selectedLayerName = nextIndex == 0 ? "" : layerNames[nextIndex];
            ClearPreview();
            resultMessage = "";
        }

        int layer = LayerMask.NameToLayer(selectedLayerName);
        using (new EditorGUI.DisabledScope(layer < 0))
        {
            if (GUILayout.Button("変更対象を確認"))
            {
                Scan(layer);
            }
        }

        if (!string.IsNullOrEmpty(resultMessage))
        {
            EditorGUILayout.HelpBox(resultMessage, MessageType.Info);
        }

        if (!hasPreview || layer < 0)
        {
            return;
        }

        if (preview.Count == 0)
        {
            EditorGUILayout.LabelField("変更が必要なオブジェクトはありません。");
            return;
        }

        EditorGUILayout.LabelField($"変更対象の Prefab は {preview.Count} 件です。");
        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
        {
            scrollPosition = scrollView.scrollPosition;
            foreach (var entry in preview)
            {
                EditorGUILayout.LabelField($"{entry.Key}（{entry.Value} 個）", EditorStyles.wordWrappedLabel);
            }
        }

        if (GUILayout.Button("一括変更して保存"))
        {
            Apply(layer);
        }
    }

    private void ClearPreview()
    {
        preview.Clear();
        hasPreview = false;
        scrollPosition = Vector2.zero;
    }

    private void Scan(int layer)
    {
        ClearPreview();
        resultMessage = "";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        var paths = new SortedSet<string>(StringComparer.Ordinal);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                paths.Add(path);
            }
        }

        int unreadableCount = 0;
        foreach (string path in paths)
        {
            GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (root == null)
            {
                unreadableCount++;
                Debug.LogWarning($"Prefab を読み込めませんでした。{path}");
                continue;
            }

            int count = 0;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (NeedsChange(child.gameObject, layer))
                {
                    count++;
                }
            }

            if (count > 0)
            {
                preview.Add(path, count);
            }
        }

        hasPreview = true;
        if (unreadableCount > 0)
        {
            resultMessage = $"読み込めなかった Prefab が {unreadableCount} 件あります。詳細は Console に表示しました。";
        }
    }

    private void Apply(int layer)
    {
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null && stage.scene.isDirty && preview.ContainsKey(stage.assetPath))
        {
            resultMessage = "Prefab モードで編集中の Prefab を保存してから、変更対象を確認し直してください。";
            return;
        }

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var orderedPaths = new List<string>();
        foreach (string path in preview.Keys)
        {
            AddInDependencyOrder(path, visited, orderedPaths);
        }

        int savedPrefabs = 0;
        int changedObjects = 0;
        int failedPrefabs = 0;
        foreach (string path in orderedPaths)
        {
            try
            {
                int count = SaveLayerChanges(path, layer);
                if (count > 0)
                {
                    savedPrefabs++;
                    changedObjects += count;
                }
            }
            catch (Exception exception)
            {
                failedPrefabs++;
                Debug.LogError($"Prefab を保存できませんでした。{path}\n{exception}");
            }
        }

        Scan(layer);
        string scanMessage = resultMessage;
        resultMessage = $"{savedPrefabs} 件の Prefab に、{changedObjects} 個のレイヤー変更を保存しました。";
        if (failedPrefabs > 0)
        {
            resultMessage += $"\n保存に失敗した Prefab は {failedPrefabs} 件です。詳細は Console に表示しました。";
        }

        if (!string.IsNullOrEmpty(scanMessage))
        {
            resultMessage += $"\n{scanMessage}";
        }
    }

    private void AddInDependencyOrder(string path, HashSet<string> visited, List<string> orderedPaths)
    {
        if (!visited.Add(path))
        {
            return;
        }

        //継承元を先に変更して、親 Prefab に不要なオーバーライドを作らない。
        foreach (string dependency in AssetDatabase.GetDependencies(path, false))
        {
            if (preview.ContainsKey(dependency))
            {
                AddInDependencyOrder(dependency, visited, orderedPaths);
            }
        }

        orderedPaths.Add(path);
    }

    private static int SaveLayerChanges(string path, int layer)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            int count = 0;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                GameObject target = child.gameObject;
                if (!NeedsChange(target, layer))
                {
                    continue;
                }

                target.layer = layer;
                if (PrefabUtility.IsPartOfPrefabInstance(target))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                }

                count++;
            }

            if (count > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
                if (!success)
                {
                    throw new InvalidOperationException("Unity の Prefab 保存処理に失敗しました。");
                }
            }

            return count;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static bool NeedsChange(GameObject target, int layer)
    {
        return target.name == TargetName && target.layer != layer;
    }
}
