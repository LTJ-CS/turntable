using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
namespace Sdk.Editor.Scripts.Addressable
{
    /// <summary>
    /// 保存手动构建的配置
    /// </summary>
    public class ManualPlayerBuildSettings
    {
        /// <summary>
        /// 小游戏appId 默认为测试appId
        /// </summary>
        public string AppId = "wx9918c95b3543f7d8";
        
        /// <summary>
        /// 程序的版本号
        /// </summary>
        public string AppVersion = "1.0";
        
        /// <summary>
        /// 资源的版本号
        /// </summary>
        public string ResVersion = "1";
        
        /// <summary>
        /// 标识是否是构建产品级的版本
        /// </summary>
        public bool ProductionBuild = false;
        
        /// <summary>
        /// 存储开发环境的资源CDN地址
        /// </summary>
        public string DevelopmentCdnUrl = "http://192.168.31.116:8082/";
        
        private const string SettingsKey = "ManualPlayerBuildSettings";

        private static ManualPlayerBuildSettings _settings;
        
        /// <summary>
        /// 保存配置
        /// </summary>
        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            EditorPrefs.SetString(SettingsKey, json);
        }
        
        /// <summary>
        /// 获取手动构建的设置
        /// </summary>
        public static ManualPlayerBuildSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    // 如果没有手动构建的设置, 则创建一个
                    _settings = new ManualPlayerBuildSettings();
                    var json = EditorPrefs.GetString(SettingsKey);
                    if (!string.IsNullOrEmpty(json))
                    {
                        _settings = JsonUtility.FromJson<ManualPlayerBuildSettings>(json);
                    }
                }
                return _settings;
            }
        }
    }
}
