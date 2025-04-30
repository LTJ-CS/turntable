using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace YooAsset.Editor
{
    /// <summary>
    /// 实现一些 Addressables 编辑器中使用的辅助函数
    /// </summary>
    public static class AddressableUtil
    {
        /// <summary>
        /// 获取指定名称的 Group, 如果获取不到, 则创建一个
        /// </summary>
        /// <param name="groupName">指定要获取的Group的名称</param>
        /// <returns></returns>
        public static AddressableAssetGroup GetOrCreateGroup(string groupName)
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup group = setting.FindGroup(groupName);
            if (group == null)
            {
                group = setting.CreateGroup(groupName, false,  false, false, null, typeof(BundledAssetGroupSchema));
            }

            return group;
        }
    }
}