using System;

/// <summary>
/// 定义路径的管理器. 不要添加全名空间, 名称短一点, 减少 AA 打包时的 Catalog 文件的体积
/// </summary>
/// <remarks>路径管理器放在 SDK 中, 方便统一各个项目的资源加载路径</remarks>
// ReSharper disable once CheckNamespace
public static class PathManager
{
    /// <summary>
    /// 资源加载的根路径
    /// </summary>
    public const string ResRoot = "Assets/GameRes/Runtime/";

    /// <summary>
    /// 场景的根路径
    /// </summary>
    public const string ScenePathRoot = ResRoot + "Scenes/";

    /// <summary>
    /// 预制件的根路径
    /// </summary>
    public const string PrefabPathRoot = ResRoot + "Prefabs/";
    
    /// <summary>
    /// 关卡相关资源的根路径
    /// </summary>
    public const string LevelPathRoot = PrefabPathRoot + "Level/";
    
    /// <summary>
    /// 地图相关资源的根路径
    /// </summary>
    public const string MapPathRoot = DataPathRoot + "Maps/";
    
    /// <summary>
    /// 声音的根路径
    /// </summary>
    public const string SoundPathRoot = ResRoot + "Sound/";

    /// <summary>
    /// UI 声音的根路径
    /// </summary>
    public const string UISoundPathRoot = SoundPathRoot + "UISound/";
    
    /// <summary>
    /// 纪念品的根路径
    /// </summary>
    public const string SouvenirPathRoot = TexturePathRoot + "Souvenir/";
    
    /// <summary>
    /// icon的根路径
    /// </summary>
    public const string IconPathRoot = TexturePathRoot + "Icon/";    
    
    /// <summary>
    /// 章节背景的根路径
    /// </summary>
    public const string ChapterPathRoot = TexturePathRoot + "Chapter/";
  
    /// <summary>
    /// 数据的根路径
    /// </summary>
    public const string DataPathRoot = ResRoot + "Data/";
    
    /// <summary>
    /// 图片的根路径
    /// </summary>
    public const string TexturePathRoot = ResRoot + "Textures/";

    /// <summary>
    /// 配置表的根路径
    /// </summary>
    public const string CfgPathRoot = ResRoot + "ConfigData/";

    /// <summary>
    /// 由给定的 UISound Clip 名称, 获取其路径
    /// </summary>
    /// <param name="clipName">UI Sound Clip 名称</param>
    /// <returns></returns>
    public static string GetUISoundClipPath(string clipName)
    {
        return UISoundPathRoot + clipName + ".ogg";
    }
    

    /// <summary>
    /// 给定章节名称，获取背景路径
    /// </summary>
    /// <param name="chapter"></param>
    /// <returns></returns>
    public static string GetChapterPath(string chapter)
    {
        return ChapterPathRoot + chapter + ".png";
    }  
    
    /// <summary>
    /// 给定的纪念品名称，获取路径
    /// </summary>
    /// <param name="souvenirPath"></param>
    /// <returns></returns>
    public static string GetSouvenirPath(string souvenirPath)
    {
        return SouvenirPathRoot + souvenirPath + ".png";
    }
    
    /// <summary>
    /// 由给定的icon名称, 获取其路径
    /// </summary>
    /// <param name="iconName">icon名称</param>
    /// <returns></returns>
    public static string GetIconPath(string iconName)
    {
        return IconPathRoot + iconName + ".png";
    }
    
    /// <summary>
    /// 由给定的 cfg 名称, 获取其路径
    /// </summary>
    /// <param name="cfgName">cfg 名称</param>
    /// <returns></returns>
    public static string GetCfgPath(string cfgName)
    {
        return CfgPathRoot + cfgName + ".json";
    }

    /// <summary>
    /// 由给定的场景名称, 获取其路径
    /// </summary>
    /// <param name="sceneName">指定要获取路径的场景名称</param>
    /// <returns></returns>
    public static string GetScenePath(string sceneName)
    {
        return ScenePathRoot + sceneName + ".unity";
    }

    /// <summary>
    /// 根据关卡ID获取关卡数据文件的路径。
    /// </summary>
    /// <param name="levelId">关卡的唯一标识符。</param>
    /// <returns>返回一个字符串，表示关卡数据文件的路径。</returns>
    public static string GetLevelDataFilePath(string levelId)
    {
        return $"{DataPathRoot}LevelData/{levelId}.json";
    }

    /// <summary>
    /// 由地图 Id 来获取官方地图数据文件的路径。
    /// </summary>
    /// <param name="mapId">指定要获取文件路径的地图 Id</param>
    /// <returns></returns>
    /// <remarks>临时的实现, 以后需要放到 CDN 上</remarks>
    public static string GetOfficialMapFilePath(string mapId)
    {
        // TODO: 暂时的命名规范
        return $"{MapPathRoot}Official_{mapId}.bytes";
    }

    /// <summary>
    /// 获得bgm文件地址
    /// </summary>
    /// <param name="bgmName">bmg文件名字</param>
    /// <returns></returns>
    public static string GetBgmFilePath(string bgmName)
    {
        return PathManager.SoundPathRoot + bgmName + ".ogg";
    }
}