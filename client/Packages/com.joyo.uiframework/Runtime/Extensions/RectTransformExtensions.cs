using UnityEngine;
// ReSharper disable once CheckNamespace
namespace UIFramework.Extensions
{
    /// <summary>
    /// RectTransform 的扩展
    /// </summary>
    public static class RectTransformExtensions
    {
        /// <summary>
        /// 创建并返回一个UI矩形变换对象，可选地设置其父对象。
        /// </summary>
        /// <param name="parent">新创建的UI矩形变换对象的父对象。</param>
        /// <param name="name">创建的 GameObject 的名称</param>
        /// <returns>新创建的UI RectTransform 对象。</returns>
        public static RectTransform CreateUIRect(RectTransform parent, string name = null)
        {
            var obj = new GameObject(name);
            var rect = obj.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }


        // 重置为全屏自适应UI
        public static void ResetToFullScreen(this RectTransform self)
        {
            self.anchorMin          = Vector2.zero;
            self.anchorMax          = Vector2.one;
            self.anchoredPosition3D = Vector3.zero;
            self.pivot              = new Vector2(0.5f, 0.5f);
            self.offsetMax          = Vector2.zero;
            self.offsetMin          = Vector2.zero;
            self.sizeDelta          = Vector2.zero;
            self.localEulerAngles   = Vector3.zero;
            self.localScale         = Vector3.one;
        }

        // 重置位置与旋转
        public static void ResetLocalPosAndRot(this RectTransform self)
        {
            self.localPosition = Vector3.zero;
            self.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// 自动重置
        /// 一般情况下就2种 全屏的 那就全部归一
        /// 其他的 那就什么都不改 只修改大小就可以了
        /// </summary>
        public static void AutoReset(this RectTransform self)
        {
            if (self.anchorMax == Vector2.one && self.anchorMin == Vector2.zero)
            {
                self.ResetToFullScreen();
            }
            else
            {
                self.localScale = Vector3.one;
            }

            self.ResetLocalPosAndRot();
        }
    }
}