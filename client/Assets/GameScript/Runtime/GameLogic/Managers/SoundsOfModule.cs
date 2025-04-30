using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sdk.Runtime.Base;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public partial class SoundManager 
{
    /// <summary>
    /// 关卡声音模块，只用于关卡的。目前先不改类名，等之后分支合并之后
    /// </summary>
    public class SoundsOfModule : IDisposable
    {
        private Dictionary<string, AsyncOperationHandle<AudioClip>> audioClipHandleDict;
            

        private Dictionary<string, AudioClip> audioClips;

        
        /// <summary>
        /// 加载声音模块数据
        /// </summary>
        /// <param name="handle">加载的句柄</param>
        public SoundsOfModule()
        {
            audioClipHandleDict = new Dictionary<string, AsyncOperationHandle<AudioClip>>();
            audioClips = new Dictionary<string, AudioClip>();

        }
        
        async UniTask<AudioClip> GetClip(string cName)
        {
            bool isContain = audioClipHandleDict.TryGetValue(cName, out var handle);
            if (!handle.IsValid())
            {
                var handle1 = LoadLevelSoundAsync(cName);
                if (isContain)
                    audioClipHandleDict[cName] = handle1;
                else
                {
                    audioClipHandleDict.Add(cName, handle1);
                }

                handle = handle1;
            }

            while (!handle.IsDone)
            {
                await UniTask.Yield();
            }

            var clip = handle.Result;
            if (clip.loadState == AudioDataLoadState.Failed || clip.loadState == AudioDataLoadState.Unloaded)
            {
                clip.LoadAudioData();
            }

            return clip;
        }
         AsyncOperationHandle<AudioClip> LoadLevelSoundAsync(string soundClipName)
        {
            return Addressables.LoadAssetAsync<AudioClip>(GetLevelSoundPath(soundClipName));
        }

        static string GetLevelSoundPath(string soundClipName)
        {
            return PathManager.SoundPathRoot + "LevelSound/" + soundClipName + ".ogg";
        }
        
        public async void PlaySound(string cName, bool isLoop, AudioSource source,bool isOneShot = true)
        {
         
            if (!audioClips.TryGetValue(cName, out AudioClip clip))
            {
                clip = await GetClip(cName);
            }
            if (clip != null)
            {
                if(source == null)
                    Instance.DoPlay(clip,isLoop,isOneShot);
                else
                {
                    Instance.DoPlayOnAudioSource(clip,isLoop,source);
                }
            }
        }
    
      
        void ClearData()
        {
            if (audioClips != null)
            {
                audioClips.Clear();
            }

            if (audioClipHandleDict != null)
            {
                foreach (var kv in audioClipHandleDict)
                {
                    if (kv.Value.IsValid())
                    {
                        Addressables.Release(kv.Value);
                    }
                }
                audioClipHandleDict.Clear();
            }
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

        ~SoundsOfModule()
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
