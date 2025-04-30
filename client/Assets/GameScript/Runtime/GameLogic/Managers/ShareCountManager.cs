using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client2Server;
using Cysharp.Threading.Tasks;
using Protocol;
using Server2Client;
using UnityEngine.ResourceManagement.Util;

namespace GameScript.Runtime.GameLogic.Managers
{
    internal class ShareRetryCache
    {
        public string             Token;
        public ECrushModeItemType Type;
        public int                RetryTimes;
    }

    /// <summary>
    /// 管理分享次数
    /// </summary>
    public class ShareCountManager : ComponentSingleton<ShareCountManager>
    {
        /// <summary>
        /// 分享次数
        /// </summary>
        private List<ModeItemSharedCount> sharedCounts;

        /// <summary>
        /// 分享重试缓存
        /// </summary>
        private List<ShareRetryCache> retryQueue = new();

        /// <summary>
        /// 等待请求结束
        /// </summary>
        private bool waitingReq = false;

        /// <summary>
        /// 等待重试
        /// </summary>
        private bool waitingRetry = false;

        public void SetShareCounts(List<ModeItemSharedCount> counts)
        {
            sharedCounts = counts;
        }

        /// <summary>
        /// 道具是否拥有分享次数
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public bool HasShareTimes(ECrushModeItemType itemType)
        {
            if (itemType == ECrushModeItemType.Invalid)
            {
                return false;
            }

            if (sharedCounts == null)
                return false;
            foreach (var count in sharedCounts)
            {
                if (count.ItemType == itemType)
                {
                    return count.LeftShareCounts - GetRetryCount(itemType) > 0;
                }
            }

            return false;
        }

        private int GetRetryCount(ECrushModeItemType itemType)
        {
            var typeList = retryQueue.FindAll(x => x.Type == itemType);
            return typeList.Count;
        }

        /// <summary>
        /// 通过分享获得了道具
        /// </summary>
        /// <param name="itemType"></param>
        public void GetSharedSkill(ECrushModeItemType itemType)
        {
            ReqUserSkill(GameInstance.Instance.gameToken, itemType, false);
        }

        private async void ReqUserSkill(string token, ECrushModeItemType itemType, bool isRetry)
        {
            if (waitingReq)
            {
                PushRetryQueue(token, itemType);
                return;
            }

            waitingReq = true;
            var (success, retData) = await NetManager.PostLogicProtoAsync<C2LS_Share, LS2C_Share_Ack>(new C2LS_Share() { MatchToken = token, ItemType = itemType }, showErrMsg: false);
            if (this == null)
            {
                return;
            }

            var code = success && retData != null ? retData.AckCode : LS2C_Share_Ack.Types.AckCode.Fail;
            switch (code)
            {
                case LS2C_Share_Ack.Types.AckCode.Success:
                    // 处理成功，更新数据，如果是重试，需要从队列中移除
                    SetShareCounts(retData?.ItemSharedInfo.ToList());
                    if (isRetry)
                    {
                        RemoveRetryFromQueue(token, itemType);
                    }

                    break;
                case LS2C_Share_Ack.Types.AckCode.InvalidToken:
                    // 失效token，不需要处理，如果是重试，需要从队列中移除
                    if (isRetry)
                    {
                        RemoveRetryFromQueue(token, itemType);
                    }

                    break;
                case LS2C_Share_Ack.Types.AckCode.Fail:
                    // 请求失败，或者服务端处理失败，如果是重试，计次，否则添加到重试队列中
                    if (!isRetry)
                    {
                        PushRetryQueue(token, itemType);
                    }
                    else
                    {
                        AddRetryTimes(token, itemType);
                    }

                    break;
                default:
                    break;
            }

            waitingReq = false;
            TryStartDelayRetry();
        }

        /// <summary>
        /// 添加重试队列
        /// </summary>
        /// <param name="token"></param>
        /// <param name="itemType"></param>
        private void PushRetryQueue(string token, ECrushModeItemType itemType)
        {
            var index = retryQueue.FindIndex(x => x.Token == token && x.Type == itemType);
            if (index < 0) retryQueue.Add(new ShareRetryCache() { Token = token, Type = itemType, RetryTimes = 0 });
        }

        /// <summary>
        /// 从队列中移除
        /// </summary>
        /// <param name="token"></param>
        /// <param name="itemType"></param>
        private void RemoveRetryFromQueue(string token, ECrushModeItemType itemType)
        {
            var index = retryQueue.FindIndex(x => x.Token == token && x.Type == itemType);
            if (index >= 0) retryQueue.RemoveAt(index);
        }

        /// <summary>
        /// 添加重试队列
        /// </summary>
        /// <param name="token"></param>
        /// <param name="itemType"></param>
        private void AddRetryTimes(string token, ECrushModeItemType itemType)
        {
            var index = retryQueue.FindIndex(x => x.Token == token && x.Type == itemType);
            if (index < 0) return;
            var shareRetryCache = retryQueue[index];
            shareRetryCache.RetryTimes++;
        }

        /// <summary>
        /// 尝试开始一个延迟重试请求
        /// </summary>
        private async void TryStartDelayRetry()
        {
            if (waitingReq || waitingRetry || retryQueue.Count == 0) return;
            waitingRetry = true;
            await UniTask.Delay(5000);
            if (this == null) return;
            waitingRetry = false;
            RetryFirstReq();
        }

        /// <summary>
        /// 重试队列中第一个请求
        /// </summary>
        private void RetryFirstReq()
        {
            if (waitingReq || retryQueue.Count == 0) return;
            var cache = retryQueue[0];
            ReqUserSkill(cache.Token, cache.Type, true);
        }
    }
}