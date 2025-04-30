using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Sdk.Runtime.JsonSerializable;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameScript.Runtime.GameLogic
{
    [Serializable]
    public class GameScoreData
    {
        public int[]                m_GuideList  = new int[3];
        public List<int>            m_PlotIndex  = new();
        public int                  m_First   = 0; //最大可进入关卡
    }

    public class GameScoreLocalStorage
    {
        private const string SettingsKey = "GameScore-Local";
        private const string HashKey     = "GameScoreHash-Local";

        private static readonly JsonSerializerSettings PrivateJsonSerializerSettings = new()
                                                                                       {
                                                                                           ContractResolver = new PrivateResolver(),
                                                                                           Formatting = Formatting.Indented
                                                                                       };


        // 保存设置
        public static void SaveSettings(GameScoreData settings)
        {
            // 序列化为 JSON
            string jsonData = JsonConvert.SerializeObject(settings, PrivateJsonSerializerSettings);

            // 计算 MD5
            string hash = CalculateMD5Hash(jsonData);

            // 保存数据和哈希值
            PlayerPrefs.SetString(SettingsKey, jsonData);
            PlayerPrefs.SetString(HashKey, hash);
            PlayerPrefs.Save();
        }

        // 加载设置
        public static GameScoreData LoadSettings()
        {
            // 获取保存的数据和哈希值
            string jsonData = PlayerPrefs.GetString(SettingsKey, string.Empty);
            string savedHash = PlayerPrefs.GetString(HashKey, string.Empty);

            if (string.IsNullOrEmpty(jsonData))
            {
                var settingData = new GameScoreData();
                return settingData;
            }

            // 重新计算 MD5
            string calculatedHash = CalculateMD5Hash(jsonData);

            // 验证哈希值
            if (savedHash == calculatedHash)
            {
                // 如果校验通过，反序列化并返回数据
                var setting = JsonConvert.DeserializeObject<GameScoreData>(jsonData, PrivateJsonSerializerSettings);
                return setting;
            }
            else
            {
                Debug.LogError("Data integrity check failed! Settings may be corrupted.");
                return new GameScoreData();
            }
        }

        // 清除设置
        public static void ClearSettings()
        {
            PlayerPrefs.DeleteKey(SettingsKey);
            PlayerPrefs.DeleteKey(HashKey);
        }

        // 计算 MD5 哈希值
        private static string CalculateMD5Hash(string input)
        {
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // 将字节数组转换为十六进制字符串
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }
}