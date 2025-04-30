using System.Collections.Generic;
using GameScript.Runtime.GameLogic;
using GameScript.Runtime.Platform;
using JetBrains.Annotations;
using Protocol;
using UnityEngine;
using UnityEngine.UI;
namespace GameScript.Runtime.Common
{
    /// <summary>
    /// 根据服务器开关和平台能力检查自动设置Active的脚本
    /// </summary>
    public class AutoCheckFuncActiveComponent : MonoBehaviour
    {
        [SerializeField] [CanBeNull] GameObject m_Target;
        [SerializeField] bool m_Interactable = true;
        [SerializeField] List<EFunctionType> m_ServerFuncList;
        [SerializeField] List<PlatformFunc> m_PlatformFuncList;

        /// <summary>
        /// 是否支持开启
        /// </summary>
        public bool Enable
        {
            get
            {
                return m_Interactable && CheckServerFunc() && CheckPlatformFunc();
            }
        }

        /// <summary>
        /// Awake时自动检查一次
        /// </summary>
        void Awake()
        {
            if (!m_Interactable)
            {
                return;
            }
            if (m_Target == null)
            {
                m_Target = gameObject;
            }
            m_Target.SetActive(Enable);
        }

        /// <summary>
        /// 检查服务器开关
        /// </summary>
        /// <returns></returns>
        bool CheckServerFunc()
        {
            foreach (var func in m_ServerFuncList)
            {
                if (!GameInstance.Instance.GetEFunctionTypeSwitch(func))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 检查平台能力支持
        /// </summary>
        /// <returns></returns>
        bool CheckPlatformFunc()
        {
            foreach (var func in m_PlatformFuncList)
            {
                if (!PlatformHandler.Instance.IsSupport(func))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
