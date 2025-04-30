using UnityEngine;
using UnityEditor;

public class SortLayerEditor : MonoBehaviour
{
    // 自定义菜单项的函数，用于设置选中物体下的SpriteRenderer组件的SortingLayer属性
    [MenuItem("Custom/Set Sorting Layers for Selected Object")]
    static void SetSortingLayersForSelectedObject()
    {
        // 获取当前选中的物体
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject!= null)
        {
            // 获取选中物体下所有带有SpriteRenderer组件的子物体
            SpriteRenderer[] spriteRenderers = selectedObject.GetComponentsInChildren<SpriteRenderer>();

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                // 设置SortingLayer，这里假设SortingLayer名称可以用数字序号来表示，如 "Layer0"、"Layer1" 等
                spriteRenderers[i].sortingOrder = spriteRenderers.Length-i;
            }

            Debug.Log("已成功为选中物体下的SpriteRenderer组件设置SortingLayer属性。");
        }
        else
        {
            Debug.Log("请先选中一个物体再执行此操作。");
        }
    }
}
