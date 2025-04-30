using System;
using System.Collections.Generic;
using System.Linq;
using ClientCfg.cfg;
using ClientProto;
using Google.Protobuf.Collections;
using Protocol;
using Server2Client;
using UnityEngine;

namespace GameScript.Runtime.GameLogic.ServerData
{
    /// <summary>
    /// 关卡数据
    /// </summary>
    public class LevelData
    {
        public const int SceneNum = 10;
        

        public struct SingleLevelInfo
        {
            public int                  ID;
            public int                  Star;
            public long                 AccumulateScore;
            public long                 HistoryScore;
            public int                  SouvenirCount;
            public bool                 Lock;
            public List<RankPlayerInfo> Ranks;

            public SingleLevelInfo(int id, int star, long accumulateScore, long historyScore, int souvenirCount, bool locked, RepeatedField<RankPlayerInfo> ranks = null)
            {
                ID = id;
                Star = star;
                AccumulateScore = accumulateScore;
                HistoryScore = historyScore;
                SouvenirCount = souvenirCount;
                Lock = locked;
                Ranks = new List<RankPlayerInfo>();
                if (ranks != null)
                {
                    Ranks.AddRange(ranks);
                }
            }
        }

        public Dictionary<int, SingleLevelInfo> LevelInfos   { get; private set; }
        public List<int>                        ChapterInfos { get; private set; }
        
        public long SumScore { get; private set; }

        /// <summary>
        /// 初始化关卡数据
        /// </summary>
        /// <param name="serverLevelInfo"></param>
        public void InitLevelData(LevelsInfo serverLevelInfo)
        {
            LevelInfos = new Dictionary<int, SingleLevelInfo>();
            for (var i = 0; i < SceneNum; i++)
            {
                LevelInfos.Add(i + 1, new SingleLevelInfo(i + 1, 0, 0, 0, 0, true));
            }

            foreach (var t in serverLevelInfo.Levels)
            {
                SumScore += t.AccumulatedScore;
                UpdateLevelInfo(t);
            }
        }

        /// <summary>
        /// 初始化章节数据
        /// </summary>
        public void InitChapterData(RepeatedField<int> serverChapterInfo)
        {
            ChapterInfos = new List<int>();
            ChapterInfos.AddRange(serverChapterInfo);
            if (!ChapterInfos.Contains(1))
            {
                ChapterInfos.Add(1);
            }
        }

        /// <summary>
        /// 初始化章节数据
        /// </summary>
        public void UnlockNewChapter(int chapterID)
        {
            ChapterInfos.Add(chapterID);
        }

        /// <summary>
        /// 更新关卡信息
        /// </summary>
        /// <param name="info"></param>
        public void UpdateLevelInfo(LevelInfo info)
        {
            //更新总得分
            SumScore -= LevelInfos[info.Id].AccumulateScore;
            //刷新关卡信息数据
            LevelInfos[info.Id] = new SingleLevelInfo(info.Id, info.Star, info.AccumulatedScore, info.HistoryScore, info.SouvenirCount, false, info.Ranks);
            SumScore += LevelInfos[info.Id].AccumulateScore;
        }
    }
}