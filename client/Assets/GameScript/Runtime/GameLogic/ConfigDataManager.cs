using System;
using System.Collections.Generic;
using ClientCfg;
using Cysharp.Threading.Tasks;
using Sdk.Runtime.Base;
using SimpleJSON;
using UnityEngine.ResourceManagement.Util;

namespace GameScript.Runtime.GameLogic
{
    public class ConfigDataManager : ComponentSingleton<ConfigDataManager>,IInitializable
    {
        private const string PreLoadLabel = "PreloadCfg";
        

        private Tables tables;
        public  Tables Tables => tables;
    
        private readonly Dictionary<string, string> configTextAssetDict = new Dictionary<string, string>();

        private ConfigDataLoader _cfgLoader;
        
        Tables GetConfigTables()
        {
            if (tables == null)
            {
                tables = new Tables(LoadByteBufByAddressable);
                
            }
            return tables;
        }
   
        //addressable 同步加载方式
        private static JSONNode LoadByteBufByAddressable(string file)
        {
            var dictionary = Instance.configTextAssetDict;
            if (dictionary.TryGetValue(file, out string value))
            {
                return JSON.Parse(value);
            }

            return default;
        }
    
        void ClearData()
        {
            tables = null;
            if(configTextAssetDict != null)
                configTextAssetDict.Clear();
            if(_cfgLoader != null)
                _cfgLoader.Dispose();
            _cfgLoader = null;
        }

        private void OnDestroy()
        {
            ClearData();
        }
        public void Register()
        {
            DontDestroyOnLoad(gameObject);
        }
        public void GetPreloadPath(ref List<string> pathList)
        {
            pathList.Add(PreLoadLabel);
        }

        public async UniTask InitializeAsync()
        {
            DontDestroyOnLoad(this);
            if (_cfgLoader != null)
            {
                _cfgLoader.Dispose();
            }

            _cfgLoader = new ConfigDataLoader(PreLoadLabel);
            configTextAssetDict.Clear();
            var resultList = await _cfgLoader.EnsureLoadAssetAsync();
            if (resultList != null && resultList.Count > 0)
            {
                for (int i = 0; i < resultList.Count; i++)
                {
                    if (configTextAssetDict.TryGetValue(resultList[i].name, out var v))
                    {
                        configTextAssetDict[resultList[i].name] = resultList[i].text;
                    }
                    else
                    {
                        configTextAssetDict.Add(resultList[i].name, resultList[i].text);
                    }
                }
            }
            GetConfigTables();
        }
    }
}