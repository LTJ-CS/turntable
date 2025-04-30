using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Sdk.Runtime.JsonSerializable;
using UnityEngine;

namespace GameScript.Runtime.GameLogic
{
    [Serializable]
    public class GameSettingsData
    {
        private bool _isDirty = false;

        //每次有字段变更或者问题修复，版本号需要加1
        private static int CurGameSettingVersion = 2;

        //本地存储版本号
        private int _version;

        public int Version
        {
            get => _version;
            set
            {
                if (_version != value)
                {
                    _version = value;
                    _isDirty = true;
                }
            }
        }

        // bgm音量
        private float _bgmVolume = 1;

        public float BgmVolume
        {
            get => _bgmVolume;
            set
            {
                if (!Mathf.Approximately(_bgmVolume, value))
                {
                    _bgmVolume = value;
                    _isDirty = true;
                }
            }
        }

        //sound音量
        private float _soundVolume = 1;

        public float SoundVolume
        {
            get => _soundVolume;
            set
            {
                if (!Mathf.Approximately(_soundVolume, value))
                {
                    _soundVolume = value;
                    _isDirty = true;
                }
            }
        }

        // bgm是否静音 -1是静音
        private int _bgmVolumeOn;

        public int BgmVolumeOn
        {
            get => _bgmVolumeOn;
            set
            {
                if (_bgmVolumeOn != value)
                {
                    _bgmVolumeOn = value;
                    _isDirty = true;
                }
            }
        }

        //sound 是否是静音  -1是静音
        private int _soundVolumeOn;

        public int SoundVolumeOn
        {
            get => _soundVolumeOn;
            set
            {
                if (_soundVolumeOn != value)
                {
                    _soundVolumeOn = value;
                    _isDirty = true;
                }
            }
        }

        //震动是否是静音  -1 是不震动
        private int _shakeOpen;

        public int ShakeOpenOn
        {
            get => _shakeOpen;
            set
            {
                if (_shakeOpen != value)
                {
                    _shakeOpen = value;
                    _isDirty = true;
                }
            }
        }

        //授权已经打开，-1是没打开
        private int _authorization;

        public int UserAuthorizationOn
        {
            get => _authorization;
            set
            {
                if (_authorization != value)
                {
                    _authorization = value;
                    _isDirty = true;
                }
            }
        }

        //
        //用于判断是否切换了平台账号
        private string openId = string.Empty;

        public string OpenId
        {
            get => openId;
            set
            {
                if (string.IsNullOrEmpty(openId) && !string.IsNullOrEmpty(value) || !openId.Equals(value))
                {
                    openId = value;
                    _isDirty = true;
                }
            }
        }

        //登陆的token
        private string _testLoginCode = string.Empty;

        public string TestLoginCode
        {
            get => _testLoginCode;
            set
            {
                if (string.IsNullOrEmpty(_testLoginCode) && !string.IsNullOrEmpty(value) || !_testLoginCode.Equals(value))
                {
                    _testLoginCode = value;
                    _isDirty = true;
                }
            }
        }
        
        //登陆的token
        private string _loginToken = string.Empty;

        public string LoginToken
        {
            get => _loginToken;
            set
            {
                if (string.IsNullOrEmpty(_loginToken) && !string.IsNullOrEmpty(value) || !_loginToken.Equals(value))
                {
                    _loginToken = value;
                    _isDirty = true;
                }
            }
        }

        //登陆的token过期时间
        private string _expiration = string.Empty;

        public string LoginTokenExpiration
        {
            get => _expiration;
            set
            {
                if (string.IsNullOrEmpty(_expiration) && !string.IsNullOrEmpty(value) || !_expiration.Equals(value))
                {
                    _expiration = value;
                    _isDirty = true;
                }
            }
        }

        //同意协议
        private bool isAuthorPolicy = false;

        public bool IsAuthorPolicy
        {
            get => isAuthorPolicy;
            set
            {
                if (isAuthorPolicy != value)
                {
                    isAuthorPolicy = value;
                    _isDirty = true;
                }
            }
        }

        //记录是否请求过平台玩家信息权限，用于在玩家拒绝了权限后，判断登陆时是否需要请求玩家信息权限
        // 玩家token没过期，需要判断下是否已经向玩家请求过权限了，如果请求过了，就不应该每次登陆都请求
        private bool _isRequestAuthorPlatformUserInfo = false;

        public bool IsRequestAuthorPlatformUserInfo
        {
            get => _isRequestAuthorPlatformUserInfo;
            set
            {
                if (_isRequestAuthorPlatformUserInfo != value)
                {
                    _isRequestAuthorPlatformUserInfo = value;
                    _isDirty = true;
                }
            }
        }


        // 是否同步了玩家头像，如果同步失败也不会设置已经同步
        private bool _hasSyncPlatformUserInfo = false;

        public bool HasSyncPlatformUserInfo
        {
            get => _hasSyncPlatformUserInfo;
            set
            {
                if (_hasSyncPlatformUserInfo != value)
                {
                    _hasSyncPlatformUserInfo = value;
                    _isDirty = true;
                }
            }
        }

        // 是否请求了隐私协议
        private bool _hasRequestPrivacySetting = false;

        public bool HasRequestPrivacySetting
        {
            get => _hasRequestPrivacySetting;
            set
            {
                if (_hasRequestPrivacySetting != value)
                {
                    _hasRequestPrivacySetting = value;
                    _isDirty = true;
                }
            }
        }

        //公告的缓存
        private string _noticeCache = string.Empty;

        public string NoticeCache
        {
            get => _noticeCache;
            set
            {
                if (string.IsNullOrEmpty(_noticeCache) && !string.IsNullOrEmpty(value) || !_noticeCache.Equals(value))
                {
                    _noticeCache = value;
                    _isDirty = true;
                }
            }
        }

        /// <summary>
        ///   修复每个版本的问题
        /// <see cref="CurGameSettingVersion"/>
        /// </summary>
        public void CheckSaveDataVersion()
        {
            //需要强制同步头像所以标志设置为没有请求过权限
            if (_version <= 1)
            {
                IsRequestAuthorPlatformUserInfo = false;
            }

            //如果版本号不一致，需要更新版本号
            if (Version != CurGameSettingVersion)
            {
                Version = CurGameSettingVersion;
                GameSettingLocalStorage.SaveSettings(this);
            }
        }

        public void Save()
        {
            if (_isDirty)
            {
                GameSettingLocalStorage.SaveSettings(this);
                _isDirty = false;
            }
        }
    }

    public class GameSettingLocalStorage
    {
        private const string SettingsKey = "GameSettings-Local";
        private const string HashKey     = "GameSettingsHash-Local";

        private static readonly JsonSerializerSettings PrivateJsonSerializerSettings = new()
                                                                                       {
                                                                                           ContractResolver = new PrivateResolver(),
                                                                                           Formatting = Formatting.Indented
                                                                                       };


        // 保存设置
        public static void SaveSettings(GameSettingsData settings)
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
        public static GameSettingsData LoadSettings()
        {
            // 获取保存的数据和哈希值
            string jsonData = PlayerPrefs.GetString(SettingsKey, string.Empty);
            string savedHash = PlayerPrefs.GetString(HashKey, string.Empty);

            if (string.IsNullOrEmpty(jsonData))
            {
                var settingData = new GameSettingsData();
                settingData = CompatibleOldSaveVersion(settingData);
                settingData.CheckSaveDataVersion();
                return settingData;
            }

            // 重新计算 MD5
            string calculatedHash = CalculateMD5Hash(jsonData);

            // 验证哈希值
            if (savedHash == calculatedHash)
            {
                // 如果校验通过，反序列化并返回数据
                var setting = JsonConvert.DeserializeObject<GameSettingsData>(jsonData, PrivateJsonSerializerSettings);
                setting.CheckSaveDataVersion();
                return setting;
            }
            else
            {
                Debug.LogError("Data integrity check failed! Settings may be corrupted.");
                return new GameSettingsData();
            }
        }

        //兼容本地存储老的版本
        static GameSettingsData CompatibleOldSaveVersion(GameSettingsData settingsData)
        {
            int shakeValue = PlayerPrefs.GetInt(GameConstant.ShakeSaveKey);
            var token = PlayerPrefs.GetString("token");
            var expiration = PlayerPrefs.GetString("token_expiration");
            var noticeCache = PlayerPrefs.GetString("notice_cache", "");
            int bgmValue = PlayerPrefs.GetInt(GameConstant.BgmSaveKey);
            int soundValue = PlayerPrefs.GetInt(GameConstant.SoundSaveKey);
            settingsData.ShakeOpenOn = shakeValue;
            settingsData.LoginToken = token;
            settingsData.LoginTokenExpiration = expiration;
            settingsData.NoticeCache = noticeCache;
            settingsData.BgmVolumeOn = bgmValue;
            settingsData.SoundVolumeOn = soundValue;
            SaveSettings(settingsData);
            return settingsData;
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