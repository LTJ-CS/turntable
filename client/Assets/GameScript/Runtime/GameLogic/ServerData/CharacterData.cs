using System;
using System.Collections.Generic;
using System.Linq;
using ClientCfg.cfg;
using ClientProto;
using GameScript.Runtime.GameLogic.Events;
using Protocol;
using Server2Client;
using UnityEngine;

namespace GameScript.Runtime.GameLogic.ServerData
{
    /// <summary>
    /// 角色的数据
    /// </summary>
    public class CharacterData
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; private set; }

        public string Avatar { get; private set; }

        public string AdCode { get; private set; }
        
        
        /// <summary>
        /// 用户是否已经同意了授权
        /// </summary>
        private bool _isAuthorized = false;

        public bool IsAuthorized => _isAuthorized;

        /// <summary>
        /// 设置用户是否已经同意了授权
        /// </summary>
        /// <param name="isAuthorized">是否同意了授权</param>
        public void SetAuthorized(bool isAuthorized)
        {
            _isAuthorized = isAuthorized;
        }

        public void SetNickNameAndAvatar(string nickName, string avatar)
        {
            Avatar = avatar;
            NickName = nickName;
        }

        public void SetAdCode(string adCode)
        {
            AdCode = adCode;
        }
        
    }
}