using System;
using System.Collections.Generic;
using DG.Tweening;
using GameScript.Runtime.GameLogic.Level;
using TMPro;
using UIFramework.UIScreen;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
namespace MyUI
{
    /// <summary>
    /// TurnTable 的 View 类   
    /// </summary>
    public sealed partial class TurnTableScreenView : UIScreenViewBase
    {
        [SerializeField] private Button          m_GoBtn;
        [SerializeField] private TextMeshProUGUI m_PackText;
        [SerializeField] private TextMeshProUGUI m_ScoreText;
        [SerializeField] private List<CardItem>  m_CardItems;
        [SerializeField] private CardItem        m_LuckyCard;
        [SerializeField] private List<Sprite>    m_CardSprites;

        private LevelController _levelController;
        private List<int>       _resultList; //转盘中所有颜色

        private void Start()
        {
            m_GoBtn.onClick.AddListener(NextRound);
            Initialization();
        }

        /// <summary>
        /// 初始化状态
        /// </summary>
        private void Initialization()
        {
            _levelController = new LevelController();
            _levelController.Init();
            m_PackText.text = _levelController.CardPackNum + "个卡包";
            m_ScoreText.text = _levelController.CardScoreNum + "分";
            m_LuckyCard.SetImage(m_CardSprites[_levelController.GetLuckyColor() - 1]);
        }

        /// <summary>
        /// 点击下一个卡包
        /// </summary>
        private void NextRound()
        {
            _levelController.NextRound();
            ShowAllCard();
            DOTween.Sequence()
                   .AppendInterval(1f)
                   .AppendCallback(() =>
                   {
                       _levelController.SettleTable();
                       ShowTable();
                   });
        }

        /// <summary>
        /// 展示结果
        /// </summary>
        private void ShowTable()
        {
            m_PackText.text = _levelController.CardPackNum + "个卡包";
            m_ScoreText.text = _levelController.CardScoreNum + "分";
            ShowAllCard();
        }

        /// <summary>
        /// 根据数据展示当前所有图案
        /// </summary>
        private void ShowAllCard()
        {
            _resultList = _levelController.GetResultList();
            for (int i = 0; i < 8; i++)
            {
                var id = _resultList[i];
                if (id == 0)
                {
                    m_CardItems[i].Hide();
                }
                else
                {
                    m_CardItems[i].SetImage(m_CardSprites[id - 1]);
                }
            }
        }
    }
}