using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Sdk.Runtime.PlayMakerActions.Addressable
{
    /// <summary>
    /// 封装一下 AssetReference, 因为它不是从 UnityEngine.Object 派生的, 无法直接使用 EditorGUILayout.PropertyField 来编辑它, 也无法直接存档
    /// 见: https://hutonggames.com/playmakerforum/index.php?topic=21385.0
    /// </summary>
    public class AssetReferenceWrapper : ScriptableObject
    {
        public AssetReference asset;
    }
}