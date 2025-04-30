using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameScript.Runtime.Platform;
using HutongGames.PlayMaker.Actions;
using Sdk.Runtime.Base;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;
using LogType = GameScript.Runtime.Platform.LogType;


public partial class SoundManager : ComponentSingleton<SoundManager>, IInitializable
{
    [SerializeField]
    private GameObject bgmFsm;

    /// <summary>
    /// 播放的bgm的信息
    /// </summary>
    private class BgmData
    {
        public PlayBgmData PlayBgmData;

        //当前播放到到时间
        public float curPlayTime = 0;

        //加载的资源句柄
        public AsyncOperationHandle<AudioClip> BGMResHandle;

        public void Clear()
        {
            PlayBgmData = default;
            if (BGMResHandle.IsValid())
                Addressables.Release(BGMResHandle);
        }
    }

    public struct PlayBgmData
    {
        //bgm名字
        public string bgmName;

        //是否循环
        public bool isLoop;

        //是否立刻播放
        public bool isImmediately;

        //播放结束回调
        public Action playFinishCallback;

        //被打断回调
        public Action interruptCallback;

        //加载完成回调
        public Action loadFinishCallback;
    }


    //音效缓存的容量
    private const int AudioClipCapacity = 20;

    //音效缓存的字典
    private LRUCache<string, AsyncOperationHandle<AudioClip>> audioClipCache = new LRUCache<string, AsyncOperationHandle<AudioClip>>(AudioClipCapacity);

    //当前播放的bgm的资源句柄
    private BgmData _curBgmData;

    //下一个要播放的bgm的数据
    private BgmData _nextBgmData;

    //瞬时音效的声音组件
    private AudioSource oneShotAudioSource;

    //同时播放的中短音效的声音数量
    private const int MaxMidAudioSourceCount = 5;

    //中短音效的声音组件缓存,一些稍微长的音效，不能用瞬时音效组件，否则设置静音时，音效不会立刻停止，目前只有LevelGenerate 音效不能用瞬时音效
    private LRUCache<int, AudioSource> midLengthAudioSourceCache = new LRUCache<int, AudioSource>(MaxMidAudioSourceCount);

    //bgmFsm
    // private GameObject _bgmFsm;

    //bgm 的声音物体
    private GameObject musicAudioSourceGo;

    //bgm 的声音组件
    private AudioSource musicAudioSource;

    //音效是否打开状态
    private bool isSoundOpen;

    //bgm 是否打开状态
    private bool isBgmOpen;

    //音效音量
    private float soundVolume;

    //bgm音量
    private float bgmVolume;


    //是否初始化了参数
    private bool isInitParam;

    //是否失去焦点
    private bool isLoseFocus = false;

    /// <summary>
    /// 是否检查是否需要切换bgm
    /// </summary>
    private bool checkUpdateChangeBgm = false;

    private void InitParam()
    {
        if (isInitParam)
            return;
        int recodeBgm = PlatformHandler.GameSettingData.BgmVolumeOn;
        int recordSound = PlatformHandler.GameSettingData.SoundVolumeOn;
        SetBGMIsOn(recodeBgm);
        SetSoundIsOn(recordSound);
        soundVolume = PlatformHandler.GameSettingData.SoundVolume;
        bgmVolume = PlatformHandler.GameSettingData.BgmVolume;
        //初始化音乐的声音组件
        if (musicAudioSourceGo != null)
        {
            Destroy(musicAudioSourceGo);
        }

        CreateBGMAudioSource();

        //初始化音效的声音组件
        oneShotAudioSource = GetComponent<AudioSource>();
        if (oneShotAudioSource == null)
            oneShotAudioSource = gameObject.AddComponent<AudioSource>();

        // _bgmFsm = Instantiate(bgmFsm, transform, true);
        isInitParam = true;
    }

    //初始化音乐的声音组件,由于平台播放bgm没有声音的问题，所以当切换音乐或者从后台返回的时候，重建背景音乐的声音物体
    void CreateBGMAudioSource()
    {
        musicAudioSourceGo = new GameObject("bgmAudioSource", typeof(AudioSource));
        musicAudioSourceGo.transform.SetParent(transform);
        musicAudioSource = musicAudioSourceGo.GetComponent<AudioSource>();
    }

    /// <summary>
    /// 销毁音乐的声音物体
    /// </summary>
    void DestroyBGMAudioSource()
    {
        if (musicAudioSource != null)
        {
            Debug.Log($"销毁音乐的声音物体");
            musicAudioSource.Stop();
            if (musicAudioSource.clip != null)
            {
                musicAudioSource.clip.UnloadAudioData();
            }

            GameObject.Destroy(musicAudioSourceGo);
        }
    }

    /// <summary>
    /// 需要预加载的音效
    /// </summary>
    private async UniTask PreLoadData()
    {
        var task = GetUIClip(SoundNameUtil.UiClick);

        await task;
    }

    /// <summary>
    /// 设置音效是否是打开状态
    /// </summary>
    /// <param name="record"></param>
    public void SetSoundIsOn(int record)
    {
        isSoundOpen = record != GameConstant.SaveInvalidValue;
        if (!isSoundOpen)
        {
            foreach (var item in midLengthAudioSourceCache)
            {
                if (item.Value != null)
                    item.Value.Stop();
            }
        }
    }

    /// <summary>
    /// 设置音乐是否是打开状态
    /// </summary>
    /// <param name="record"></param>
    public void SetBGMIsOn(int record)
    {
        isBgmOpen = record != GameConstant.SaveInvalidValue;
        if (!isBgmOpen)
        {
            DestroyBGMAudioSource();
            if (_curBgmData != null)
            {
                _curBgmData.Clear();
                _curBgmData = null;
            }

            if (_nextBgmData != null)
            {
                _nextBgmData.Clear();
                _nextBgmData = null;
            }
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        musicAudioSource.volume = volume;
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = volume;
    }

    public void PlayBGM(string bgmName, bool isLoop = false, bool isImmediately = false, Action loadFinishCallback = null, Action playFinishCallback = null, Action interruptCallback = null)
    {
        if (!HandleBeforeChangeNextBgm(bgmName))
        {
            return;
        }

        PlayBGM(new PlayBgmData()
                {
                    bgmName = bgmName,
                    isLoop = isLoop,
                    isImmediately = isImmediately,
                    playFinishCallback = playFinishCallback,
                    interruptCallback = interruptCallback,
                    loadFinishCallback = loadFinishCallback
                }
        );
    }

    /// <summary>
    /// 播放背景音乐，公有接口
    /// </summary>
    /// <param name="playBgmData">要播放的bgm的数据</param>
    public void PlayBGM(PlayBgmData playBgmData)
    {
        if (!HandleBeforeChangeNextBgm(playBgmData.bgmName))
        {
            return;
        }

        var bgmHandle = ResLoaderHelper.LoadBgmAsync(playBgmData.bgmName);
        bgmHandle.Completed += OnBgmLoadComplete;
        _nextBgmData = new BgmData()
                       {
                           PlayBgmData = playBgmData,
                           BGMResHandle = bgmHandle,
                       };
    }

    /// <summary>
    /// 处理更换的下一个BGM是否有效
    /// </summary>
    /// <param name="bgmName"></param>
    /// <returns></returns>
    private bool HandleBeforeChangeNextBgm(string bgmName)
    {
        if (!isBgmOpen)
            return false;
        if (string.IsNullOrEmpty(bgmName))
        {
            Debug.LogError("播放背景音乐时，bgm名字为空");
            return false;
        }

        if (_nextBgmData != null)
        {
            //如果重复播放同一个bgm，直接返回
            if (_nextBgmData.PlayBgmData.bgmName.Equals(bgmName))
            {
                return false;
            }

            //如果不相同，打断之前的bgm
            _nextBgmData.PlayBgmData.interruptCallback?.Invoke();
            _nextBgmData.Clear();
            _nextBgmData = null;
        }

        //如果当前正在播放的bgm和要播放的bgm相同，则直接返回
        var curPlayingBgmName = GetCurPlayBgmName();
        //之所以使用contains, 是目前带有目录前缀，所以需要判断是否包含
        if (!string.IsNullOrEmpty(curPlayingBgmName) && bgmName.Contains(curPlayingBgmName))
            return false;
        return true;
    }

    void OnBgmLoadComplete(AsyncOperationHandle<AudioClip> handle)
    {
        if (_nextBgmData == null)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
            return;
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (handle.Result != null)
            {
                if (_nextBgmData.PlayBgmData.bgmName.Contains(handle.Result.name))
                {
                    handle.Result.LoadAudioData();
                    var nextBgmData = _nextBgmData;
                    checkUpdateChangeBgm = true;
                    if (nextBgmData.PlayBgmData.isImmediately)
                    {
                        ChangeBgm();
                    }

                    nextBgmData.PlayBgmData.loadFinishCallback?.Invoke();
                }
                else
                {
                    Addressables.Release(handle);
                }
            }
        }
        else
        {
            PlatformHandler.Instance.Log($"加载背景音乐失败: {_nextBgmData.PlayBgmData.bgmName}", LogType.Error);
            _nextBgmData.Clear();
            _nextBgmData = null;
        }
    }

    /// <summary>
    /// 获取当前正在播放的bgm
    /// </summary>
    /// <returns></returns>
    public string GetCurPlayBgmName()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
            return musicAudioSource.clip.name;
        return string.Empty;
    }

    public void StopCurPlayBgm()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying) musicAudioSource.Stop();
    }
    
    public void StopCurPlaySound()
    {
        if (oneShotAudioSource != null && oneShotAudioSource.isPlaying) oneShotAudioSource.Stop();
    }


    /// <summary>
    /// 切换BGM检查
    /// </summary>
    void ChangeBgm()
    {
        if (!checkUpdateChangeBgm)
            return;

        //如果没有等待播放的bgm，或者musicAudioSourceGo是空，直接返回
        if (_nextBgmData == null)
            return;
        //如果资源句柄非法，直接返回
        if (!_nextBgmData.BGMResHandle.IsValid())
        {
            _nextBgmData = null;
            return;
        }

        //如果资源句柄没有加载完成，直接返回
        if (!_nextBgmData.BGMResHandle.IsDone)
            return;
        if (musicAudioSource != null)
        {
            //如果音效正在播放过程中，且不需要立刻切换，则等待bgmsource停止后再播放
            if (musicAudioSource.isPlaying)
            {
                if (!_nextBgmData.PlayBgmData.isImmediately)
                {
                    if (musicAudioSource.time <= musicAudioSource.clip.length - Time.deltaTime * 2)
                    {
                        return;
                    }

                    _curBgmData?.PlayBgmData.playFinishCallback?.Invoke();
                    musicAudioSource.Stop();
                }
                else
                {
                    _curBgmData?.PlayBgmData.interruptCallback?.Invoke();
                    musicAudioSource.Stop();
                }
            }
            else
            {
                _curBgmData?.PlayBgmData.playFinishCallback?.Invoke();
            }

            //清理之前的bgm声音组件
            if (musicAudioSource.clip != null)
            {
                var oldClip = musicAudioSource.clip;
                musicAudioSource.clip = null;
                oldClip.UnloadAudioData();
            }
        }

        //因为一些平台播放的问题，在切换新的bgm时，销毁老的，创建新的bgm声音组件
        if (musicAudioSourceGo != null)
        {
            GameObject.Destroy(musicAudioSourceGo);
            musicAudioSourceGo = null;
        }

        //释放当前正在播放的音乐资源句柄
        if (_curBgmData != null)
        {
            _curBgmData.Clear();
            _curBgmData = null;
        }

        //将要播放的句柄赋值给当前句柄
        _curBgmData = _nextBgmData;
        //如果当前句柄是合法的，播放声音
        if (_curBgmData.BGMResHandle.IsValid())
        {
            CreateBGMAudioSource();
            musicAudioSource.clip = _curBgmData.BGMResHandle.Result;
            musicAudioSource.loop = _curBgmData.PlayBgmData.isLoop;
            musicAudioSource.volume = bgmVolume;
            musicAudioSource.Play();
        }

        //将等待播放的数据设置为空
        _nextBgmData = null;

        checkUpdateChangeBgm = false;
    }

    private void Update()
    {
        ChangeBgm();
    }

    /// <summary>
    /// 播放ui声音
    /// </summary>
    /// <param name="cName"></param>
    /// <param name="isOneShot">是否是瞬时音效</param>
    public void PlayUISound(string cName, bool isOneShot = true)
    {
        //如果没有开启音效，不播放声音
        if (!isSoundOpen)
            return;

        DoPlayUISound(cName, isOneShot);
    }

    /// <summary>
    /// 获取ui clip 并播放声音
    /// </summary>
    /// <param name="cName"></param>
    /// <param name="isOneShot">是否是瞬时音效</param>
    async void DoPlayUISound(string cName, bool isOneShot)
    {
        AudioClip clip = await GetUIClip(cName);
        if (clip == null)
            return;
        DoPlay(clip, false, isOneShot);
    }

    /// <summary>
    /// 在所给的audioSource上播放音效
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="isLoop"></param>
    /// <param name="audioSource"></param>
    void DoPlayOnAudioSource(AudioClip clip, bool isLoop, AudioSource audioSource)
    {
        //如果没有开启音效，不播放声音
        if (!isSoundOpen)
            return;
        if (audioSource == null || clip == null)
            return;
        audioSource.clip = clip;
        audioSource.loop = isLoop;
        audioSource.Play();
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="isLoop"></param>
    /// <param name="isOneShot"></param>
    void DoPlay(AudioClip clip, bool isLoop, bool isOneShot)
    {
        //如果没有开启音效，不播放声音
        if (!isSoundOpen)
            return;
        if (isOneShot)
        {
            oneShotAudioSource.PlayOneShot(clip, soundVolume);
        }
        else
        {
            //最多缓存MaxMidAudioSourceCount 个音效组件，如果当前数量已经大于最大数量，则从缓存的第一个组件取，第一个组件是最早被使用的
            AudioSource audioSource = null;
            int count = midLengthAudioSourceCache.Count();
            if (count == MaxMidAudioSourceCount)
            {
                audioSource = midLengthAudioSourceCache.Get(1);
            }
            else
            {
                //获取最早被使用的缓存
                var firstNode = midLengthAudioSourceCache.Get(1);
                //如果最早被使用的组件正在播放，则新建一个组件，这个判断不精准，但是对于缓存来说，影响不大
                if (firstNode == null || firstNode.isPlaying)
                {
                    //如果当前数量不大于最大数量，新建一个组件
                    audioSource = gameObject.AddComponent<AudioSource>();
                    midLengthAudioSourceCache.Put(count + 1, audioSource);
                }
                else
                {
                    audioSource = firstNode;
                }
            }

            if (audioSource.isPlaying)
                audioSource.Stop();
            audioSource.loop = isLoop;
            audioSource.clip = clip;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }

    /// <summary>
    /// 异步获取ui 音效资源
    /// </summary>
    /// <param name="cName"></param>
    /// <returns></returns>
    async UniTask<AudioClip> GetUIClip(string cName)
    {
        var handle = audioClipCache.Get(cName);
        if (!handle.IsValid())
        {
            var handle1 = ResLoaderHelper.LoadUISoundAsync(cName);
            var oldHandle = audioClipCache.Put(cName, handle1);
            if (oldHandle.IsValid())
            {
                Addressables.Release(oldHandle);
            }

            handle = handle1;
        }

        while (!handle.IsDone)
        {
            await UniTask.Yield();
        }

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            audioClipCache.Remove(cName);
            Addressables.Release(handle);
            return null;
        }

        var clip = handle.Result;
        if (clip.loadState == AudioDataLoadState.Failed || clip.loadState == AudioDataLoadState.Unloaded)
        {
            clip.LoadAudioData();
        }

        return clip;
    }

    /// <summary>
    /// unity 销毁方法
    /// </summary>
    private void OnDestroy()
    {
        foreach (var kv in audioClipCache)
        {
            if (kv.Value.IsValid())
                Addressables.Release(kv.Value);
        }

        if (_curBgmData != null)
        {
            _curBgmData.Clear();
            _curBgmData = null;
        }

        if (_nextBgmData != null)
        {
            _nextBgmData.Clear();
            _nextBgmData = null;
        }

        audioClipCache.Clear();
        isInitParam = false;
    }

    /// <summary>
    /// 注册，初始化时调用一次
    /// </summary>
    public void Register()
    {
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 获取预加载列表ßß
    /// </summary>
    /// <param name="pathList"></param>
    public void GetPreloadPath(ref List<string> pathList)
    {
        pathList.Add(PathManager.GetUISoundClipPath(SoundNameUtil.UiClick));
    }

    /// <summary>
    /// 初始化操作，如果初始化出错，会多次重进该方法
    /// </summary>
    public async UniTask InitializeAsync()
    {
        await PreLoadData();

        InitParam();
    }

    /// <summary>
    /// 平台下切换前后台，bgm不播放的问题
    /// 销毁组件重新创建
    /// </summary>
    /// <param name="hasFocus"></param>
    private void OnApplicationFocus(bool hasFocus)
    {
#if !UNITY_EDITOR
        if (hasFocus)
        {
            if (isLoseFocus)
            {
                isLoseFocus = false;
                // //销毁之前的bgm声音组件, 重新创建, 以解决切换前后台时，bgm无法播放的问题
                // if (musicAudioSource != null)
                // {
                //     Debug.Log($"销毁音乐的声音物体");
                //     musicAudioSource.Stop();
                //     GameObject.Destroy(musicAudioSourceGo);
                // }
                // if (_curBgmData != null)
                // {
                //     CreateBGMAudioSource();
                //     musicAudioSource.clip = _curBgmData.BGMResHandle.Result;
                //     musicAudioSource.loop = _curBgmData.PlayBgmData.isLoop;
                //     musicAudioSource.time = _curBgmData.curPlayTime;
                //     musicAudioSource.Play();
                // }
                //销毁重建瞬时音效组件, 以解决切换前后台时，瞬时音效无法播放的问题
                if (oneShotAudioSource != null)
                {
                    Destroy(oneShotAudioSource);
                    oneShotAudioSource = null;
                }
                oneShotAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            isLoseFocus = true;
            // 离开游戏，记录当前播放的时间
            if (_curBgmData != null && musicAudioSource != null)
            {
                _curBgmData.curPlayTime = musicAudioSource.time;
            }
        }
#endif
    }
}