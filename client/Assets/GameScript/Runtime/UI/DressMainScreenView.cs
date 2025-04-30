using System;
using System.Collections.Generic;
using MyUI;
using Sdk.Runtime.Components;
using UIFramework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace GameScript.Runtime.UI
{
    public enum EDressType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,

        /// <summary>
        /// 头发
        /// </summary>
        Hair = 1,

        /// <summary>
        /// 面部
        /// </summary>
        Face = 2,

        /// <summary>
        /// 上身
        /// </summary>
        Clothes = 3,

        /// <summary>
        /// 下身
        /// </summary>
        Pants = 4,

        /// <summary>
        /// 帽子
        /// </summary>
        Hat = 5,

        /// <summary>
        /// 身体
        /// </summary>
        Body = 6,

        /// <summary>
        /// 鞋子
        /// </summary>
        Shoes = 7,

        /// <summary>
        /// 眼睛
        /// </summary>
        Eyes = 8,

        /// <summary>
        /// 耳朵
        /// </summary>
        Ears = 9,

        /// <summary>
        /// 嘴巴
        /// </summary>
        Mouth = 10,

        /// <summary>
        /// 鼻子
        /// </summary>
        Noses = 11,

        /// <summary>
        /// 眉毛
        /// </summary>
        Brow = 12,

        /// <summary>
        /// 脐部
        /// </summary>
        Navel = 13,

        /// <summary>
        /// 嘴部
        /// </summary>
        Lip = 14,

        /// <summary>
        /// 鼻部
        /// </summary>
        NosesSub = 15,

        /// <summary>
        /// 脸部
        /// </summary>
        EyesSub = 16,
    }

    public enum EBodyType
    {
        Fat  = 1,
        Thin = 2
    }
    public class DressMainScreenView : MonoBehaviour
    {

    }
}