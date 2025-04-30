using System;
using ClientCfg.cfg;
using GameScript.Runtime.GameLogic.ServerData;
using Server2Client;
using UnityEngine;

namespace GameScript.Runtime.GameLogic.Events
{
    /// <summary>
    /// UI 的事件
    /// </summary>
    /// <remarks>
    /// These aren’t events in the strict C# sense – we want external objects to raise them – but they function as events for messaging purposes.
    /// 我们使用 MVP 的模式来开发 UI 相关的逻辑, 主要是因为 UI 是异步加载的, 有可能成功, 也有可能失败, 这无法完全保证. 也解耦相关的逻辑! 
    /// </remarks>
    public static class MainUIEvents
    {
        #region View -> Presenter: handle user input

        /// <summary>
        /// 开始日常模式的比赛的事件
        /// </summary>
        public static event Action StartDailyMatch;

        public static void RaiseStartDailyMatch()
        {
            StartDailyMatch?.Invoke();
        }

        /// <summary>
        /// 当选择了一个关卡设置师时触发
        /// </summary>
        public static event Action<DesignerStageInfo> SelectLevelDesigner;

        public static void RaiseSelectLevelDesigner(DesignerStageInfo designerStageInfo)
        {
            SelectLevelDesigner?.Invoke(designerStageInfo);
        }

        /// <summary>
        /// 取消选择关卡设置师时触发
        /// </summary>
        public static event Action SelectLevelDesignerCancel;

        public static void RiseSelectLevelDesignerCancel()
        {
            SelectLevelDesignerCancel?.Invoke();
        }

        /// <summary>
        /// 需要显示关卡设计师选择 Screen 时触发
        /// </summary>
        public static event Action ShowLevelDesignerSelectScreen;

        public static void RiseShowLevelDesignerSelectScreen()
        {
            ShowLevelDesignerSelectScreen?.Invoke();
        }

        /// <summary>
        /// 更新首页皮肤红点
        /// </summary>
        public static event Action UpdateSkinRedDot;

        public static void RiseUpdateSkinRedDot()
        {
            UpdateSkinRedDot?.Invoke();
        }

        /// <summary>
        /// 更新装扮中皮肤选中状态
        /// </summary>
        public static event Action<EDressType, int> UpdateSkinSelected;

        public static void RiseUpdateSkinSelected(EDressType type, int itemId)
        {
            UpdateSkinSelected?.Invoke(type, itemId);
        }

        /// <summary>
        /// 播放主界面退出效果
        /// </summary>
        public static Func<float> PlayMainMenuExitEff;

        public static float RaisePlayMainMenuExitEff()
        {
            var result = PlayMainMenuExitEff?.Invoke();
            return result ?? 0.1f;
        }

        /// <summary>
        /// 开始日常模式的比赛的事件
        /// </summary>
        public static event Action UpdateSkinDict;

        public static void RiseUpdateSkinDict()
        {
            UpdateSkinDict?.Invoke();
        }

        /// <summary>
        /// 主界面的引导，发送按钮数据
        /// </summary>
        public static event Action<RectTransform> GetMainGuideTarget;

        public static void SendMainGuideTarget(RectTransform target)
        {
            GetMainGuideTarget?.Invoke(target);
        }

        /// <summary>
        /// 主界面的引导结束
        /// </summary>
        public static event Action MainGuideFinish;

        public static void SendMainGuideFinish()
        {
            MainGuideFinish?.Invoke();
        }

        /// <summary>
        /// 准备触发主界面新手引导
        /// </summary>
        public static event Action ReadyMainGuide;

        public static void SendReadyMainGuide()
        {
            ReadyMainGuide?.Invoke();
        }

        /// <summary>
        /// 需要显示装扮界面 Screen 时触发
        /// </summary>
        public static event Action ShowCustomSkinScreen;

        public static void RiseShowCustomSkinScreen()
        {
            ShowCustomSkinScreen?.Invoke();
        }

        /// <summary>
        /// 装扮页面退出到主界面时触发
        /// </summary>
        public static event Action CustomSkinBackToMain;

        public static void RiseCustomSkinBackToMain()
        {
            CustomSkinBackToMain?.Invoke();
        }

        /// <summary>
        /// 显示话题奖励 Screen 时触发
        /// </summary>
        public static event Action ShowTopicRewardScreen;

        public static void RaiseShowTopicRewardScreen()
        {
            ShowTopicRewardScreen?.Invoke();
        }

        /// <summary>
        /// 话题奖励页面退出到主界面时触发
        /// </summary>
        public static event Action TopicRewardScreenBackToMain;

        public static void RiseTopicRewardScreenBackToMain()
        {
            TopicRewardScreenBackToMain?.Invoke();
        }


        /// <summary>
        /// 关卡选择界面向服务器发起请求进入关卡
        /// </summary>
        public static event Action SelectLevelC2StartMatch;

        public static void RiseSelectLevelC2SStartMatch()
        {
            SelectLevelC2StartMatch?.Invoke();
        }

        /// <summary>
        /// 开始日常模式失败，用于处理StartDailyMatch对应的页面显示
        /// </summary>
        public static event Action StartDailyMatchFail;

        public static void RaiseStartDailyMatchFail()
        {
            StartDailyMatchFail?.Invoke();
        }

        /// <summary>
        /// 新装扮飞入按钮
        /// </summary>
        public static event Action NewSkinFlyToButton;

        public static void RiseNewSkinFlyToButton()
        {
            NewSkinFlyToButton?.Invoke();
        }

        /// <summary>
        /// 重载 Main Screen 时触发
        /// </summary>
        public static event Action ReEnterMainScreenEvent;

        public static void RiseReEnterMainScreenEvent()
        {
            ReEnterMainScreenEvent?.Invoke();
        }

        /// <summary>
        /// 打开装扮收藏页
        /// </summary>
        public static event Action OpenSkinCollection;

        public static void RiseOpenSkinCollection()
        {
            OpenSkinCollection?.Invoke();
        }

        /// <summary>
        /// 关闭装扮收藏页
        /// </summary>
        public static event Action CloseSkinCollection;

        public static void RiseCloseSkinCollection()
        {
            CloseSkinCollection?.Invoke();
        }

        /// <summary>
        /// 打开装扮详情页
        /// </summary>
        public static event Action<SkinInfo, Action, bool> OpenCollectionInfo;

        public static void RiseOpenCollectionInfo(SkinInfo skinInfo, Action needUpdateAc, bool isSendingGift = false)
        {
            OpenCollectionInfo?.Invoke(skinInfo, needUpdateAc, isSendingGift);
        }

        /// <summary>
        /// 关闭装扮详情页
        /// </summary>
        public static event Action CloseCollectionInfo;

        public static void RiseCloseCollectionInfo()
        {
            CloseCollectionInfo?.Invoke();
        }

        /// <summary>
        /// 礼物收取结束事件
        /// </summary>
        public static event Action GiftTakeOverEvent;

        public static void RiseGiftTakeOver()
        {
            GiftTakeOverEvent?.Invoke();
        }

        /// <summary>
        /// 礼物回赠事件
        /// </summary>
        public static event Action GiftReSendOverEvent;

        public static void RiseGiftReSendOver()
        {
            GiftReSendOverEvent?.Invoke();
        }

        /// <summary>
        /// 礼物回执结束事件
        /// </summary>
        public static event Action GiftReceiptOverEvent;

        public static void RiseGiftReceiptOver()
        {
            GiftReceiptOverEvent?.Invoke();
        }

        /// <summary>
        /// 检查礼物分享码事件，主要用于热启动
        /// </summary>
        public static event Action CheckGiftShareCodeEvent;

        /// <summary>
        /// 检查热启动流程，最初用于检查礼物分享码，现已扩展到公众号和邮件系统的检查
        /// </summary>
        public static void RiseCheckGiftShareCodeEvent()
        {
            CheckGiftShareCodeEvent?.Invoke();
        }

        #endregion

        #region Presenter -> View: sync UI sliders to Model

        #endregion


        #region Presenter -> Model: update volume settings

        #endregion

        #region Model -> Presenter: model values changed (e.g. loading saved values)

        #endregion
        
        /// <summary>
        /// 首页Display显示事件，由关闭全屏界面触发
        /// </summary>
        public static event Action MainRestartEvent;

        public static void RaiseMainRestartEvent()
        {
            MainRestartEvent?.Invoke();
        }
        
        /// <summary>
        /// 碰到星星事件
        /// </summary>
        public static event Action RefreshRole;

        public static void RaiseRefreshRole()
        {
            RefreshRole?.Invoke();
        }

        
        /// <summary>
        /// 镜头缩放开始事件
        /// </summary>
        public static event Action CameraZoomBeginEvent;

        public static void RaiseCameraZoomBeginEvent()
        {
            CameraZoomBeginEvent?.Invoke();
        }
        
    }
}