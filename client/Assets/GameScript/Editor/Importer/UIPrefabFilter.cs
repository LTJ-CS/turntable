using System;
using System.Text.RegularExpressions;
using YooAsset.Editor;

namespace GameScript.Editor.Importer
{
    [DisplayName("收集UI预制体")]
    public class UIPrefabFilter : IFilterRule
    {
        /// <summary>
        /// 用于过滤 UI Prefab 路径的正则表达式
        /// </summary>
        private static readonly Regex Regex = new(@"^Assets/GameRes/Runtime/UI/[^/]+/Prefabs/[^/]+\.prefab$", RegexOptions.IgnoreCase);

        public bool IsCollectAsset(FilterRuleData data)
        {
            // 匹配 UI Prefab与 SpriteAtlas, 之所以必须把 SpriteAtlas 打成 AssetBundle 是因为如果不把 SpriteAtlas 打成 AssetBundle, 只有 UI 引用 SpriteAtlas 中的Sprite,
            // 这个 Sprite 与它所在的 SpriteAtlas 都会被打成单独的 AssetBundle, 导致多余的AssetBundle, 浪费下载性能
            return Regex.IsMatch(data.AssetPath) || data.AssetPath.EndsWith(".spriteatlas", StringComparison.OrdinalIgnoreCase);
        }
    }
}