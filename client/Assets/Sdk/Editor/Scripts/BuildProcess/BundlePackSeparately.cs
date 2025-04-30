using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Callbacks;
using Debug = UnityEngine.Debug;

public class BundlePackSeparately : IPreprocessBuildWithReport
{
    private static AddressableAssetSettings    _settings;
    private static List<AddressableAssetEntry> _entriesToRemove = new();
    private static List<AddressableAssetEntry> _entriesToAdd    = new();
    private static AddressableAssetGroup       _targetGroup;

    public int callbackOrder
    {
        get { return 0; }
    }

    public void OnPreprocessBuild(BuildReport report)
    {
    }

    public static void PreprocessBuild()
    {
        _entriesToRemove.Clear();
        _entriesToAdd.Clear();

        _settings = AddressableAssetSettingsDefaultObject.Settings;
        if (_settings == null)
        {
            Debug.LogError("AddressableAssetSettings not found!");
            return;
        }

        AddressableAssetEntry targetEntry = null;
        foreach (var group in _settings.groups)
        {
            foreach (var entry in group.entries)
            {
                if (entry.labels.Contains("PackSeparately"))
                {
                    _targetGroup = group;
                    targetEntry  = entry;
                    if (entry.SubAssets == null)
                    {
                        Debug.Log($"### entry.SubAssets == null {entry.AssetPath}");
                        continue;
                    }
                    
                    foreach (var subAsset in entry.SubAssets)
                    {
                        if (string.IsNullOrEmpty(subAsset.AssetPath))
                        {
                            continue;
                        }

                        _entriesToAdd.Add(subAsset);
                    }
                }
            }
        }

        foreach (var variable in _entriesToAdd)
        {
            AddAssetEntry(_targetGroup, variable.AssetPath);
        }

        if (targetEntry == null)
        {
            return;
        }

        _entriesToRemove.Add(targetEntry);
        _targetGroup.RemoveAssetEntry(targetEntry);

        EditorUtility.SetDirty(_settings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static AddressableAssetEntry AddAssetEntry(AddressableAssetGroup group, string assetPath)
    {
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        AddressableAssetEntry entry = group.entries.FirstOrDefault(e => e.guid == guid);
        if (entry == null)
        {
            entry = _settings.CreateOrMoveEntry(guid, group, false, false);
        }

        entry.address = assetPath;
        return entry;
    }

    [PostProcessBuild(0)]
    public static void RestoreOriginalAddressables(BuildTarget target, string pathToBuiltProject)
    {
        if (_targetGroup == null)
        {
            return;
        }

        foreach (var variable in _entriesToRemove)
        {
            var entry = AddAssetEntry(_targetGroup, variable.AssetPath);
            entry.SetLabel("PackSeparately", true, false, false);
        }

        foreach (var variable in _entriesToAdd)
        {
            _targetGroup.RemoveAssetEntry(variable);
        }

        _entriesToRemove.Clear();
        _entriesToAdd.Clear();

        EditorUtility.SetDirty(_settings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}