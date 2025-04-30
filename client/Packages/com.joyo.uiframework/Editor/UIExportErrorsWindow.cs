using System.Collections.Generic;
using System.Linq;
using UIFramework.Base;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    /// <summary>
    /// UI 导出错误描述的窗口, 用于跳转错误定位
    /// </summary>
    public class UIExportErrorsWindow : EditorWindow
    {
        private ListView        _listView;
        private List<ErrorInfo> _errors;

        /// <summary>
        /// 使用给定的错误列表信息打开 UI 导出错误描述的窗口
        /// </summary>
        /// <param name="errors"></param>
        public static void Open(List<ErrorInfo> errors, string title = null)
        {
            if (title == null)
                title = "UI 导出错误";
            
            // 只有错误数量 > 0 的时候才弹出错误弹窗
            if (errors.Count > 0)
            {
                var window = GetWindow<UIExportErrorsWindow>(title);
                window.Refresh(errors);    
            }
        }

        private void CreateGUI()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UXML/UIExportErrorWindow");
            var listItemTemplate = Resources.Load<VisualTreeAsset>("UXML/ErrorListItemTemplate");
            var root = visualTree.Instantiate();
            rootVisualElement.Add(root);
            _listView = root.Q<ListView>("lvErrors");

            _listView.makeItem = () =>
            {
                var listItem = listItemTemplate.Instantiate();
                return listItem;
            };
            _listView.bindItem = (e, index) =>
            {
                var item = e.Q<Label>("labelErrorDesc");
                item.text = _errors[index].Error;
            };

            _listView.onSelectedIndicesChange += OnSelectedIndicesChange;
        }

        private void OnSelectedIndicesChange(IEnumerable<int> itemIndices)
        {
            foreach (var itemIndex in itemIndices)
            {
                var errorInfo = _errors[itemIndex];
                if (errorInfo.GameObject == null)
                {
                    EditorUtility.DisplayDialog("错误", "对应 Prefab 的 PrefabStage 窗口已经被关闭或对象已经被删除, 无法再定位错误对象", "确定");
                    return;
                }

                Selection.activeGameObject = errorInfo.GameObject;
                break;
            }
        }

        /// <summary>
        /// 刷新错误信息显示
        /// </summary>
        /// <param name="errors"></param>
        private void Refresh(List<ErrorInfo> errors)
        {
            _errors = errors.ToList();
            _listView.itemsSource = _errors;
        }
    }
}