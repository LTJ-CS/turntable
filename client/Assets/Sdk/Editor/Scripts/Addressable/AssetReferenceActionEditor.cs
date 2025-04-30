using HutongGames.PlayMakerEditor;
using Sdk.Runtime.PlayMakerActions.Addressable;
using UnityEditor;
using UnityEngine;
namespace Sdk.Editor.Scripts.Addressable
{
   
    
    /// <summary>
    /// 为 LoadGameObjectAssetReference 提供编辑 UI, 否则 AssetReference 变量无法编辑
    /// </summary>
    [CustomActionEditor(typeof(LoadGameObjectAssetReference))]
    public class AssetReferenceActionEditor : CustomActionEditor
    {
        private SerializedObject _serializedObject;
        private SerializedProperty _serializedProperty;
        private GUIContent _guiContent;
        
        public override bool OnGUI()
        {
            if (_serializedObject == null)
            {
                _serializedObject = new SerializedObject((target as LoadGameObjectAssetReference)?.AssetWrapper);
                _serializedProperty = _serializedObject.FindProperty(nameof(AssetReferenceWrapper.asset));
                _guiContent = new GUIContent("Asset", "指定要加载的 Prefab(GameObject)");
            }
            
            EditorGUI.BeginChangeCheck();
            _serializedObject.Update();
            EditorGUILayout.PropertyField(_serializedProperty, _guiContent, false);
            var isDirty = EditorGUI.EndChangeCheck();
           
            EditField(nameof(LoadGameObjectAssetReference.StoreObject));
            EditField(nameof(LoadGameObjectAssetReference.IsDone));
            EditField(nameof(LoadGameObjectAssetReference.IsError));
            return isDirty || GUI.changed;
        }
        
        public override void OnDisable()
        {
            _serializedProperty.Dispose();
            _serializedObject.Dispose();
            _serializedObject = null;
            base.OnDisable();
        }
    }
}