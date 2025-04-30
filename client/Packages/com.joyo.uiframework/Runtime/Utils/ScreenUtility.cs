using System;
using UIFramework.Base;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
namespace UIFramework.Utils
{
    /// <summary>
    /// Screen 相关的辅助功能函数
    /// </summary>
    public static class ScreenUtility
    {
        /// <summary>
        /// 根据背景显示的类型来设置 Image 的颜色
        /// </summary>
        /// <param name="image">指定要设置颜色的Image</param>
        /// <param name="showType">背景显示类型</param>
        /// <param name="customColor">当 showType 为 EBackgroundShowType.CustomColor 时, 指定额外的颜色</param>
        public static void SetImageColor(RawImage image, EBackgroundShowType showType, Color customColor)
        {
            switch (showType)
            {
                case EBackgroundShowType.None:
                    image.enabled = false;
                    break;
                case EBackgroundShowType.CustomColor:
                    image.enabled = true;
                    image.color   = customColor;
                    break;
                case EBackgroundShowType.FullyTransparent:
                    image.enabled = false;
                    break;
                case EBackgroundShowType.SemiTransparent:
                    image.color   = customColor;
                    image.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(showType), showType, null);
            }
        }
    }
}