using System.Collections.Generic;
using UnityEngine;

public static class GameConstant
{
   public const string BgmSaveKey = "gameSettings_musicSetting";
   
   
   public const string SoundSaveKey = "gameSettings_effectSetting";

   public const string ShakeSaveKey = "gameSettings.ShakeSaveKey";

   public const int SaveInvalidValue = -1;

   public const int SoundMuteValue = -80;

   public const int TipsValidValue = -1;
   
   public const int InvalidValue = -1;
   
   //活动阵营里的红队颜色
   public static Color RedColor = new Color(1,0,0,1);
   //活动阵营里的蓝队颜色
   public static Color BlueColor = new Color(0,0.164f,1,1);

   /// <summary>
   /// 振动开启时的值
   /// </summary>
   public const int ShakeOpenValue = 1;

   public static List<string> Frame60SceneNames = new List<string>()
                                                  {
                                                     "DailyMatchLevelNewScene"
                                                  };
}
