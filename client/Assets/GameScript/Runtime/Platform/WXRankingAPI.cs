
using System;
using System.Collections.Generic;
using ClientCfg.cfg;
using GameScript.Runtime.GameLogic;
using UnityEngine;
#if PLATFORM_WEIXIN
using WeChatWASM;
#endif

namespace GameScript.Runtime.Platform
{
    public interface  IWxRankingAPI
    {
        /// <summary>
        /// 子域排行榜事件枚举
        /// </summary>
        public enum SubContextShowType
        {
            friendRank,   // 好友排行榜
            reset,        // 重置子域
        }
        
        /// <summary>
        /// 获取子域Event参数
        /// </summary>
        /// <param name="subType"></param>
        /// <returns></returns>
        private static string GetSubEventStr(SubContextShowType subType){
            var name = Enum.GetName(typeof(SubContextShowType), subType);
            return name;
        }
        
        /// <summary>
        /// 子域消息数据类
        /// </summary>
        public class OpenDataMessage
        {
            public string                         Event;
            public string                         ScoreObjString;
            public string                         Type;
            public string                         Uid;
            public LaunchSettings.EnvironmentType Env;
        }
        
        
        /// <summary>
        /// 朋友圈更新数据
        /// </summary>
        public class  RankFriendUploadData
        {
            public string Uid;
            public string DataTime;
            public int    SkinCount;
            public int    PassCount;
            public string SkinInfo;
        }
        
        /// <summary>
        /// 在子域中更新微信排行榜
        /// </summary>
        /// <param name="rankName">排行榜名字(可能会存在多种排行榜)</param>
        /// <param name="scoreObjString">分数等数据块</param>
        /// <param name="type"> type 为 addon 的时候，不判断历史最高分，直接累加数量</param>
        public static void WxRankScoreUpdate(SubContextShowType rankName)
        {
#if PLATFORM_WEIXIN
            // 创建消息结构体
            OpenDataMessage msgData = new OpenDataMessage();
            // 设置消息类型
            msgData.Event = "update-" + GetSubEventStr(rankName);
            msgData.Type = "";
            // 获取皮肤等游戏数据
            msgData.ScoreObjString = JsonUtility.ToJson(GenerateRankData());
            string msg = JsonUtility.ToJson(msgData);
            // 获取子域
            WXOpenDataContext openData = WeChatWASM.WX.GetOpenDataContext();
            // 发送消息
            openData.PostMessage(msg);
#endif
        }

        /// <summary>
        /// 显示子域
        /// </summary>
        /// <param name="rankName"></param>
        public static void WxShowFriendView(SubContextShowType rankName)
        {
#if PLATFORM_WEIXIN
            // 创建消息结构体
            OpenDataMessage msgData = new OpenDataMessage();
            // 设置消息类型
            msgData.Event = "show-" + GetSubEventStr(rankName);
            msgData.Uid =  GameInstance.Instance.GetUid();
            msgData.Env = LaunchSettings.environmentType;
            // 获取皮肤等游戏数据
            msgData.ScoreObjString = JsonUtility.ToJson(GenerateRankData());
            string msg = JsonUtility.ToJson(msgData);
            // 获取子域
            WXOpenDataContext openData = WeChatWASM.WX.GetOpenDataContext();
            // 发送消息
            openData.PostMessage(msg);
#endif
        }
        
        
        /// <summary>
        /// 获取一份最新的游戏数据
        /// </summary>
        private static  RankFriendUploadData GenerateRankData()
        {
            // 获取uid
            var uid = GameInstance.Instance.GetUid();
            // 皮肤数量
            var skinCount = 0;
            // 子域数据版本号
            var openDataVersion = 1;
            // 用于拼接皮肤字符串
            var skinInfo = openDataVersion + "-";
            // 去掉最后一个 -
            string skinInfoResult = skinInfo.Substring(0, skinInfo.Length - 1);
            // 通关次数
            var passCount = 0;
            
            var rankData = new  RankFriendUploadData();
            rankData.Uid = uid;
            rankData.SkinCount = skinCount;
            rankData.PassCount = passCount;
            rankData.DataTime = GameInstance.Instance.CurrentTimeStamp().ToString();
            rankData.SkinInfo = skinInfoResult;
            return rankData;
        }
        
        /// <summary>
        /// 在子域中设置要分享的礼物名
        /// </summary>
        /// <param name="giftName">排行榜名字(可能会存在多种排行榜)</param>
        public static void SetShareGiftName(string giftName)
        {
#if PLATFORM_WEIXIN
            // 创建消息结构体
            OpenDataMessage msgData = new OpenDataMessage();
            // 设置消息类型
            msgData.Event = "sharegift-" + giftName;
            msgData.Type = "";
            // 获取皮肤等游戏数据
            msgData.ScoreObjString = JsonUtility.ToJson(GenerateRankData());
            string msg = JsonUtility.ToJson(msgData);
            // 获取子域
            WXOpenDataContext openData = WeChatWASM.WX.GetOpenDataContext();
            // 发送消息
            openData.PostMessage(msg);
#endif
        }

        /// <summary>
        /// 显示好友分享列表
        /// </summary>
        public static void WxShowFriendShareList()
        {
#if PLATFORM_WEIXIN
            // 创建消息结构体
            OpenDataMessage msgData = new OpenDataMessage();
            // 设置消息类型
            msgData.Event = "showshare-somedata";
            msgData.Uid =  GameInstance.Instance.GetUid();
            msgData.Env = LaunchSettings.environmentType;
            string msg = JsonUtility.ToJson(msgData);
            // 获取子域
            WXOpenDataContext openData = WeChatWASM.WX.GetOpenDataContext();
            // 发送消息
            openData.PostMessage(msg);
#endif
        }
    }
}