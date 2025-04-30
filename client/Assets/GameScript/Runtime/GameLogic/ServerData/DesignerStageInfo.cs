using ClientCfg;
using ClientCfg.cfg;
using Protocol;

namespace GameScript.Runtime.GameLogic.ServerData
{
    /// <summary>
    /// 关卡设计师的信息
    /// </summary>
    public class DesignerStageInfo
    {
        /// 设计师被点赞数量
        public int likeCount;

        /// 关卡 Id
        public string mapId;

        /// 关卡 MD5
        public string md5;
        
        /// 关卡类型
        public EDesignerType designerType;

        /// 今日通关人数
        public int dailyPassedCount;

        /// 今日挑战次数
        public int dailyChallengeCount;
    }
}