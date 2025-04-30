using UIFramework;
using UnityEngine;
namespace GameScript.Runtime.UI 
{
    /// <summary>
    /// 定义项目相关的UI框架的设置
    /// </summary>
    public class MyUIFrameworkSettings : UIFrameworkSettings
    {
        protected override string m_ProjectName => "UI";
        protected override string m_Namespace => "MyUI";
        protected override Vector2 m_ReferenceResolution => new(750, 1334);
    }
}