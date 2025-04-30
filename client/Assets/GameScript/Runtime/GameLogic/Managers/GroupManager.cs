using System;
using System.Collections.Generic;
using Client2Server;
using Cysharp.Threading.Tasks;
using GameScript.Runtime.Platform;
using MyUI;
using Server2Client;
using Random = UnityEngine.Random;

namespace GameScript.Runtime.GameLogic.Managers
{
    public class GroupManager
    {
        private static GroupManager _instance;

        public static GroupManager Instance
        {
            get
            {
                if (_instance == null) _instance = new GroupManager();
                return _instance;
            }
        }

        public static event Action MemberUpdateEvent;

        readonly List<PlayerCard> _members = new List<PlayerCard>();

        /// <summary>
        /// 请求添加成员
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async UniTask<bool> ReqShareJoinGroup(string uid)
        {
            ReportInviteAction(3);
            var req = new C2LS_ShareJoinGroup
            {
                Uid = uid
            };
            var (success, retData) = await NetManager.PostLogicProtoAsync<C2LS_ShareJoinGroup, LS2C_ShareJoinGroup_Ack>(req);
            if (!success || retData is not { AckCode: LS2C_ShareJoinGroup_Ack.Types.AckCode.Success })
            {
                ToastScreenPresenter.Show("邀请伴舞失败，请重试！");
                return false;
            }
            ToastScreenPresenter.Show("邀请伴舞成功！");
            ReportInviteAction(4);
            _ = ReqGetGroupMembers();
            return true;
        }

        /// <summary>
        /// 请求成员数据
        /// </summary>
        /// <returns></returns>
        public async UniTask<bool> ReqGetGroupMembers()
        {
            var (success, retData) = await NetManager.PostLogicProtoAsync<C2LS_GetGroupMembers, LS2C_GetGroupMembers_Ack>(new C2LS_GetGroupMembers());
            if (!success || retData is not { AckCode: LS2C_GetGroupMembers_Ack.Types.AckCode.Success })
            {
                return false;
            }
            _members.Clear();
            foreach (var playerCard in retData.Players)
            {
                _members.Add(playerCard);
            }
            MemberUpdateEvent?.Invoke();
            return true;
        }

        /// <summary>
        /// 获得所有成员
        /// </summary>
        /// <returns></returns>
        public PlayerCard[] GetMembers()
        {
            return _members.ToArray();
        }

        /// <summary>
        /// 带参数的分享
        /// </summary>
        public void ShareMessage(Action<bool> callback = null)
        {
            string[] titleList =
            {
                "快来！和我一起成为大明星！", "热辣舞蹈，快来上台和我一决高下！", "惊呆了！好友居然在跳性感辣舞！",
            };
            int randomInt = Random.Range(0, 3);
            double curTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
            var data = new ShareMessageType(titleList[randomInt], "https://incubator-static.easygame2021.com/game-journey/share-img/share-dance.png", $"joinGroup=uid:{GameInstance.Instance.GetUid()}-time:{curTime}");
            PlatformHandler.Instance.Platform.ShareMessage(data, callback);
        }

        /// <summary>
        /// 移除一个成员
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async UniTask<bool> ReqRemoveMember(string uid)
        {
            var req = new C2LS_RemoveGroupMember
            {
                Uid = uid
            };
            (bool success, var retData) = await NetManager.PostLogicProtoAsync<C2LS_RemoveGroupMember, LS2C_RemoveGroupMember_Ack>(req);
            if (!success || retData is not { AckCode: LS2C_RemoveGroupMember_Ack.Types.AckCode.Success })
            {
                return false;
            }

            foreach (var member in _members)
            {
                if (member != null && member.UserBaseInfo.Uid == uid)
                {
                    _members.Remove(member);
                    _ = ReqGetGroupMembers();
                    break;
                }
            }
            return true;
        }

        public static void ReportInviteAction(int action)
        {
            
            TdReport.Report(Reportkey.InviteClick, new Dictionary<string, object>
            {
                {
                    "action", action
                }
            });
        }
    }
}