namespace GameScript.Runtime.GameLogic.ServerData
{
    /// <summary>
    /// 保存与服务器同步的玩家数据
    /// </summary>
    public class PlayerState
    {
        /// <summary>
        /// 后台返回的token
        /// </summary>
        private string _token;

        public string Token
        {
            get => _token;
            set => _token = value;
        }

        /// <summary>
        /// 用户信息
        /// </summary>
        private UserInfo _userInfo = new UserInfo();

        public UserInfo UserInfo
        {
            get => _userInfo;
            set => _userInfo = value;
        }

        public void SetUid(string uid)
        {
            _userInfo.Uid = uid;
        }

        public void SetUType(int type)
        {
            _userInfo.Type = type;
        }

        public string GetUid()
        {
            return _userInfo.Uid;
        }

        public int GetUType()
        {
            return _userInfo.Type;
        }

        public void SetAdCode(string adCode)
        {
            CharacterData.SetAdCode(adCode);
        }

        public void SetPlatformUserInfo(string nickName, string avatar)
        {
            CharacterData.SetNickNameAndAvatar(nickName, avatar);
        }

        /// <summary>
        /// 角色数据
        /// </summary>
        public CharacterData CharacterData { get; set; } = new();

        /// <summary>
        /// 角色数据
        /// </summary>
        public LevelData LevelData { get; private set; } = new();
    }
}