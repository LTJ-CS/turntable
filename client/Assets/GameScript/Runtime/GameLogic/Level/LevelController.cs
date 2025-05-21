using System.Collections.Generic;
using System.Linq;
using MyUI;
using UnityEngine;

namespace GameScript.Runtime.GameLogic.Level
{
    public class CardColor
    {
        public int Id;
        public int Weight;

        public CardColor(int id, int weight)
        {
            Id = id;
            Weight = weight;
        }
    }

    public class CardScore
    {
        public int Id;
        public int Pack;
        public int Score;

        public CardScore(int id, int pack, int score)
        {
            Id = id;
            Pack = pack;
            Score = score;
        }
    }

    public class ReplayLog
    {
        public int CardColorId;
        public int Pack;
        public int Number;

        public ReplayLog(int cardColorId, int pack, int number)
        {
            CardColorId = cardColorId;
            Pack = pack;
            Number = number;
        }
    }

    public class LevelController
    {
        //卡包数量
        public int CardPackNum { get; private set; } = 0;

        //总积分
        public int CardScoreNum { get; private set; } = 0;

        private readonly List<CardColor> _cardColors = new(); //颜色图鉴（包括权重和颜色）
        private readonly List<CardScore> _cardScores = new(); //得分图鉴（包括不同配对方式和奖励）
        private readonly List<ReplayLog> _cardLogs   = new(); //开包过程记录
        private          CardColor       _luckyColor;         //幸运色
        private          List<int>       _resultList = new(); //转盘中所有颜色

        /// <summary>
        /// 获取抽取日志
        /// </summary>
        /// <returns></returns>
        public List<ReplayLog> GetCardLogs()
        {
            return _cardLogs;
        }
        
        public List<int> GetResultList()
        {
            return _resultList;
        }

        public int GetLuckyColor()
        {
            return _luckyColor.Id;
        }

        /// <summary>
        /// 初始化关卡数据
        /// </summary>
        public void Init()
        {
            foreach (var config in ConfigDataManager.Instance.Tables.GDCardColor.DataList)
            {
                _cardColors.Add(new CardColor(config.Id, config.Weight));
            }

            foreach (var config in ConfigDataManager.Instance.Tables.GDCardScore.DataList)
            {
                _cardScores.Add(new CardScore(config.Id, config.AddCardBox, config.Score));
            }

            _luckyColor = _cardColors[Random.Range(0, _cardColors.Count)];
            CardPackNum = 20;
            ResetTable();
        }

        /// <summary>
        /// 打开一次卡包
        /// </summary>
        public void NextRound()
        {
            var cost = _resultList.Count(x => x == 0);

            if (CardPackNum - cost < 0)
            {
                ToastScreenPresenter.Show("卡包不足");
                return;
            }
            //减少卡包数量
            CardPackNum -= cost;
            //重置数据
            _cardLogs.Clear();
            //计算得出这次转盘所有颜色
            GetRandomIdListByWeight();
        }
        
        /// <summary>
        /// 结算转盘
        /// </summary>
        public void SettleTable()
        { 
            //计算正常多颜色奖励
            CalculateReward();
            //正式奖励
            foreach (var log in _cardLogs)
            {
                CardScoreNum += log.Number;
                CardPackNum += log.Pack;
            }
        }

        /// <summary>
        /// 结算幸运色
        /// </summary>
        private void CalculateLuck(int index)
        {
            var pack = 0;
            var score = 0;
            foreach (var data in _cardScores)
            {
                if (data.Id == 1)
                {
                    pack = data.Pack;
                    score = data.Score;
                }
            }

            _cardLogs.Add(new ReplayLog(index, pack, score));
        }

        /// <summary>
        /// 结算多个奖励
        /// </summary>
        private void CalculateReward()
        {
            //颜色-数量字典
            var colorDict = new Dictionary<int, int>();
            foreach (var data in _resultList)
            {
                if (!colorDict.TryAdd(data, 1))
                {
                    colorDict[data]++;
                }
            }

            //全家福
            if (colorDict.Count == _cardColors.Count)
            {
                var pack = 0;
                var score = 0;
                foreach (var data in _cardScores)
                {
                    if (data.Id == 9)
                    {
                        pack = data.Pack;
                        score = data.Score;
                    }
                }

                _cardLogs.Add(new ReplayLog(-1, pack, score));
                ResetTable();
            }
            //结算超过2个相同颜色的
            else
            {
                foreach (var pair in colorDict)
                {
                    if (pair.Value >= 2)
                    {
                        var pack = 0;
                        var score = 0;
                        foreach (var data in _cardScores)
                        {
                            if (data.Id == pair.Value)
                            {
                                pack = data.Pack;
                                score = data.Score;
                            }
                        }

                        _cardLogs.Add(new ReplayLog(pair.Key, pack, score));
                        //清除台面对应颜色
                        for (int i = 0; i < 8; i++)
                        {
                            if (_resultList[i] == pair.Key)
                            {
                                _resultList[i] = 0;
                            }
                        }
                    }
                }
            }
        }

        // 根据权重随机获取一个Id
        private int GetRandomIdByWeight()
        {
            int totalWeight = _cardColors.Sum(c => c.Weight);
            int randomNumber = Random.Range(0, totalWeight);

            int accumulatedWeight = 0;
            foreach (var cardColor in _cardColors)
            {
                accumulatedWeight += cardColor.Weight;
                if (randomNumber < accumulatedWeight)
                {
                    return cardColor.Id;
                }
            }

            return _cardColors.Last().Id;
        }

        // 生成包含8个随机Id的列表（每个Id都按权重随机选择）
        private void GetRandomIdListByWeight()
        {
            for (int i = 0; i < 8; i++)
            {
                if (_resultList[i] == 0)
                {
                    _resultList[i] = GetRandomIdByWeight();
                    //计算幸运色
                    if (_resultList[i] == _luckyColor.Id)
                    {
                        CalculateLuck(i);
                    }
                }
            }
        }

        /// <summary>
        /// 重置台面
        /// </summary>
        private void ResetTable()
        {
            _resultList = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0 };
        }
    }
}