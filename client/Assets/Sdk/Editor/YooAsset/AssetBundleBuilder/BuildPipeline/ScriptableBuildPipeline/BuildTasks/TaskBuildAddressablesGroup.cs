using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建 Addressables 的 Group 的任务, 由设置收集策略来更新
    /// </summary>
    public class TaskBuildAddressablesGroup : IBuildTask
    {
        public void Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;
            var buildMode = buildParameters.BuildMode;
            var packageName = buildParameters.PackageName;

            // 1. 获取所有收集器收集的资源
            var collectResult = AssetBundleCollectorSettingData.Setting.GetPackageAssets(buildMode, packageName);
            var allCollectAssets = collectResult.CollectAssets;

            // 远程组的名称, 目前写死了, 减少配置工作量
            const string remoteGroupName = "Remote Group";
            // 重复资源隔离组的名称
            const string duplicateAssetIsolationGroupName = "Duplicate Asset Isolation";

            // 收集所有需要地址的资源的 guid
            var assetGuidSets = new HashSet<string>();
            foreach (var collectAssetInfo in allCollectAssets)
            {
                assetGuidSets.Add(collectAssetInfo.AssetInfo.AssetGUID);
            }

            var entriesCreated = new List<AddressableAssetEntry>();
            var entriesMoved = new List<AddressableAssetEntry>();
            var entriesRemoved = new List<AddressableAssetEntry>();

            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            {
                // 向 Remote 组中添加或删除资源项
                var group = AddressableUtil.GetOrCreateGroup(remoteGroupName);
                AddOrMoveEntries(assetGuidSets, group, entriesCreated, entriesMoved, entriesRemoved);
            }

            {
                // 向自动解析的依赖 组中添加或删除资源项
                
                // 确保组存在
                var group = AddressableUtil.GetOrCreateGroup(duplicateAssetIsolationGroupName);
                
                // 先清理自动依赖组的资源, 否则下面分析依赖关系时会不正确, 因为有些资源已经被添加到了自动依赖组
                foreach (var assetEntry in group.entries.ToArray())
                {
                    group.RemoveAssetEntry(assetEntry, false);
                }
                
                // 自己实现 CheckBundleDupeDependencies.FixIssues() 函数, 因为它总是会创建新的隔离组: Duplicate Asset Isolation
                var checkBundleDupeDependencies = new CheckBundleDupeDependencies();

                // 通过反射调用 CheckBundleDupeDependencies.CheckForDuplicateDependencies() 函数
                var checkForDuplicateDependenciesFunc =
                    typeof(CheckBundleDupeDependencies).GetMethod("CheckForDuplicateDependencies", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
                checkForDuplicateDependenciesFunc.Invoke(checkBundleDupeDependencies, new Object[] { aaSettings });

                // 通过反射获取 CheckBundleDupeDependencies.m_ImplicitAssets 变量
                var implicitAssetsField = typeof(CheckBundleDupeDependencies).GetField("m_ImplicitAssets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
                var implicitAssets = (HashSet<GUID>)implicitAssetsField.GetValue(checkBundleDupeDependencies);

                
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
                schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
                schema.UseAssetBundleCache = false;
                schema.UseAssetBundleCrc = false;
                schema.UseAssetBundleCrcForCachedBundles = false;
                schema.RetryCount = 3;
                schema.IncludeInBuild = true;
                schema.IncludeLabelsInCatalog = false;
                schema.IncludeAddressInCatalog = false;
                schema.IncludeGUIDInCatalog = false;
                schema.Timeout = 10;
                schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.Dynamic;
                schema.InternalBundleIdMode = BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdHash;

                if (implicitAssets.Count > 0)
                {
                    assetGuidSets.Clear();
                    // 添加共享的资源到 自动依赖组
                    var createAndAddEntryToGroupFunc = typeof(AddressableAssetSettings).GetMethod("CreateAndAddEntryToGroup", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
                    foreach (var assetGuid in implicitAssets)
                    {
                        var guidStr = assetGuid.ToString();
                        var assetEntry = aaSettings.FindAssetEntry(guidStr, false);
                        if (assetEntry == null)
                        { // 如果资源还不存在 AA 中, 则添加它. 注: CreateOrMoveEntry() 会重复执行 FindAssetEntry() 
                            // 通过反射调用 CreateAndAddEntryToGroup
                            createAndAddEntryToGroupFunc.Invoke(aaSettings, new Object[] { guidStr, group, true, false });
                        }
                    }
                }
            }
            
            // 刷新数据
            aaSettings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            
            BuildLogger.Log("<color=green>自动收集并同步 AA 资源成功</color>");
        }

        /// <summary>
        /// 添加给定的资源到 Addressables 组
        /// </summary>
        private void AddOrMoveEntries(HashSet<string> assetGuidSets, AddressableAssetGroup group, List<AddressableAssetEntry> entriesCreated, List<AddressableAssetEntry> entriesMoved,
                                      List<AddressableAssetEntry> entriesRemoved)
        {
            {
                // 先删除组中不再需要地址的项
                var entries = group.entries.ToArray();
                foreach (var assetEntry in entries)
                {
                    if (!assetGuidSets.Contains(assetEntry.guid))
                    {
                        entriesRemoved.Add(assetEntry);
                        group.RemoveAssetEntry(assetEntry, false);
                    }
                }
            }
            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            {
                // 添加所有需要地址的资源
                // 通过反射来获取 CreateOrMoveEntries 函数的调用, 因为它是 internal 的
                var createOrMoveEntriesFunc = typeof(AddressableAssetSettings).GetMethod("CreateOrMoveEntries", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
                createOrMoveEntriesFunc.Invoke(aaSettings, new Object[] { assetGuidSets, group, entriesCreated, entriesMoved, false, false });
            }
        }
    }
}