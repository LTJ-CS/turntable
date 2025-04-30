using System;
using AccountProto;

public static class CommonUIEvents 
{
   #region 断网/资源加载失败确认

   /// <summary>
   /// OfflineDisplay 界面重试事件
   /// </summary>
   public static event Action OnOfflineTry;

   public static void RaiseOfflineTry()
   {
      OnOfflineTry?.Invoke();
   }
   
   /// <summary>
   /// OfflineDisplay 界面关闭事件
   /// </summary>
   public static event Action OnCloseOfflineDisplay;

   public static void RaiseCloseOfflineDisplay()
   {
      OnCloseOfflineDisplay?.Invoke();
   }
   
   /// <summary>
   /// FailUpLoadDisplay 界面确认放弃事件
   /// </summary>
   public static event Action OnGiveUpDataUpLoad;

   public static void RaiseGiveUpDataUpLoad()
   {
      OnGiveUpDataUpLoad?.Invoke();
   }
   
   /// <summary>
   /// FailUpLoadDisplay 界面关闭事件
   /// </summary>
   public static event Action OnCloseFailUpLoadDisplay;

   public static void RaiseCloseFailUpLoadDisplay()
   {
      OnCloseFailUpLoadDisplay?.Invoke();
   }
   /// <summary>
   /// FailUpLoadDisplay 界面关闭事件
   /// </summary>
   public static event Action<LoginAck> AfterReLogin;

   public static void RaiseReLogin(LoginAck loginAck)
   {
      AfterReLogin?.Invoke(loginAck);
   }
   #endregion
   
   /// <summary>
   ///  使用收藏
   /// </summary>
   public static event Action<int> OnCollectBeUsed;

   public static void SendUseCollect(int collectId)
   {
      OnCollectBeUsed?.Invoke(collectId);
   }

   #region 场景过渡处理
   
   /// <summary>
   /// 过渡到关卡场景的等待结束了
   /// </summary>
   public static event Action TranslationToLevelWaitingFinish;

   public static void SendLevelTranslationWaitingFinish()
   {
      TranslationToLevelWaitingFinish?.Invoke();
   }
   
   

   #endregion
}
