namespace GameScript.Runtime.GameLogic.ServerData
{
    /// <summary>
    /// 存储用户的基础信息
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 用户的唯一 id
        /// </summary>
        private string _uid;

        public string Uid
        {
            get => _uid;
            set => _uid = value;
        }

        int _type;
        public int Type
        {
            get => _type;
            set => _type = value;
        }

        /// <summary>
        /// GM 设置的测试账号，控制特殊的功能开启列表，日志
        /// </summary>
        public bool TestAccount
        {
            get;
            set;
        }

    }
}