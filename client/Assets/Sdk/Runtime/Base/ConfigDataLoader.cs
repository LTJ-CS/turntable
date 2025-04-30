using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Sdk.Runtime.Base
{
    public class ConfigDataLoader : IDisposable
    {
        private readonly string cfgLoadLabel;
    
        private AsyncOperationHandle<IList<TextAsset>> cfgDataHandle;

        public ConfigDataLoader(string loadLabel)
        {
            cfgLoadLabel = loadLabel;
        }
        /// <summary>
        /// 确保必须加载配置表
        /// </summary>
        /// <returns></returns>
        public async UniTask<IList<TextAsset>> EnsureLoadAssetAsync()
        {
            while (true)
            {
                cfgDataHandle =  Addressables.LoadAssetsAsync<TextAsset>(cfgLoadLabel, null);
                while (!cfgDataHandle.IsDone)
                {
                    await UniTask.Yield();
                }
                if (!cfgDataHandle.IsValid())
                {
                    throw new Exception($"找不到要加载的配置表标签{cfgLoadLabel}");
                }

                if (cfgDataHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(cfgDataHandle);
                    Debug.LogError($"[ConfigDataLoader] 加载配置表资源失败: {cfgLoadLabel}(重试中 ...");
                    await UniTask.Delay(500); // 可能是网络问题, 等待500ms后重试
                    continue;
                }
                var resultList = cfgDataHandle.Result;

            
                return resultList;
            }
        }

        void ClearData()
        {
            if(cfgDataHandle.IsValid())
                Addressables.Release(cfgDataHandle);
        }
    
        #region IDisposable Support

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ClearData();
                }

                disposed = true;
            }
        }

        ~ConfigDataLoader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
