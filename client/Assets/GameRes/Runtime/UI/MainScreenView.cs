using System;
using System.Collections.Generic;
using GameScript.Runtime.GameLogic;
using GameScript.Runtime.GameLogic.Events;
using GameScript.Runtime.UI;
using Sdk.Runtime.Components;
using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using UIFramework.UIScreen;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace MyUI
{
    /// <summary>
    /// Main 的 View 类   
    /// </summary>
    public sealed partial class MainScreenView : UIScreenViewBase
    {
        [SerializeField] private GridView  m_GridSkinList;
        [SerializeField] private DressItem m_DressPrefab;

        [SerializeField] private Button m_BodyBtn;
        [SerializeField] private Button m_HairBtn;
        [SerializeField] private Button m_FaceBtn;
        [SerializeField] private Button m_ClothesBtn;
        [SerializeField] private Button m_PantsBtn;
        [SerializeField] private Button m_ShoesBtn;

        [SerializeField] private Button        m_HatBtn;
        [SerializeField] private Button        m_EyesBtn;
        [SerializeField] private Button        m_NosesBtn;
        [SerializeField] private Button        m_MouthBtn;
        [SerializeField] private Button        m_BrowBtn;
        [SerializeField] private Button        m_EyesSubBtn;
        [SerializeField] private Button        m_NosesSubBtn;
        [SerializeField] private Button        m_LipBtn;
        [SerializeField] private Button        m_EarsBtn;
        [SerializeField] private Button        m_NavelBtn;
        [SerializeField] private List<Button>  m_ColorBtns;
        [SerializeField] private GameObject    m_ColorPart;
        [SerializeField] private GameObject    m_Other;
        [SerializeField] private RectTransform m_ContentTransform;
        [SerializeField] private RoleItem      m_Role;
        [SerializeField] private Button        m_ReturnBtn;

        private EDressType _currListType   = EDressType.None;
        private List<int>  _currCollection = new List<int>();

        private List<int> _body = new List<int>()
                                  {
                                      1, 2
                                  };

        private List<int> _shoes = new List<int>()
                                   {
                                       1, 2, 3, 4, 5, 6, 7, 8, 9, 10,11, 12, 13, 14,16 ,18, 19,20
                                   };

        private List<int> _clothes = new List<int>()
                                     {
                                         1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 16, 18, 19, 20
                                     };

        private List<int> _pants = new List<int>()
                                   {
                                       1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 20
                                   };

        private List<int> _face = new List<int>()
                                  {
                                      1, 2, 3, 4, 5, 6, 7
                                  };

        private List<int> _eyes = new List<int>()
                                  {
                                      1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18
                                  };

        private List<int> _ears = new List<int>()
                                  {
                                      1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 13, 14, 15, 16, 17, 18
                                  };

        private List<int> _mouth = new List<int>()
                                   {
                                       1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18
                                   };

        private List<int> _noses = new List<int>()
                                   {
                                       1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18
                                   };

        private List<int> _brow = new List<int>()
                                  {
                                      1, 3, 4, 11, 16, 18
                                  };

        private List<int> _eyesSub = new List<int>()
                                     {
                                         1, 6, 9, 13, 16
                                     };

        private List<int> _nosesSub = new List<int>()
                                      {
                                          2, 6, 10
                                      };

        private List<int> _lip = new List<int>()
                                 {
                                     1, 2, 4, 7, 12, 13
                                 };

        private List<int> _navel = new List<int>()
                                   {
                                       1, 2
                                   };

        private List<int> _hair = new List<int>()
                                  {
                                      1, 2, 3, 4, 5, 6, 7, 8, 9, 10
                                  };


        private List<int> _hat = new List<int>()
                                 {
                                     4, 5, 6, 9, 11, 12, 14
                                 };

        private bool isInit;

        private int _currSelectIndex;

        private void Start()
        {
            SoundManager.Instance.StopCurPlayBgm();
            SoundManager.Instance.PlayBGM(SoundDirConst.Bgm + SoundNameUtil.Bgm, true);
            m_GridSkinList.onGridItem = OnGridItem;
            m_BodyBtn.onClick.AddListener(OnBodyBtnClick);
            m_ShoesBtn.onClick.AddListener(OnShoesBtnClick);
            m_FaceBtn.onClick.AddListener(OnFaceBtnClick);
            m_ClothesBtn.onClick.AddListener(OnClothesBtnClick);
            m_PantsBtn.onClick.AddListener(OnPantsBtnClick);
            m_EyesBtn.onClick.AddListener(OnEyesBtnClick);
            m_EarsBtn.onClick.AddListener(OnEarsBtnClick);
            m_MouthBtn.onClick.AddListener(OnMouthBtnClick);
            m_NosesBtn.onClick.AddListener(OnNosesBtnClick);
            m_BrowBtn.onClick.AddListener(OnBrowBtnClick);
            m_EyesSubBtn.onClick.AddListener(OnEyesSubBtnClick);
            m_NosesSubBtn.onClick.AddListener(OnNosesSubBtnClick);
            m_LipBtn.onClick.AddListener(OnLipBtnClick);
            m_NavelBtn.onClick.AddListener(OnNavelBtnClick);
            m_HairBtn.onClick.AddListener(OnHairBtnClick);
            m_HatBtn.onClick.AddListener(OnHatBtnClick);
            m_ReturnBtn.onClick.AddListener(
                () =>
                {
                    UIManager.CloseScreen<MainScreenPresenter>();
                    MainUIEvents.RaiseRefreshRole();
                });

            foreach (var btn in m_ColorBtns)
            {
                btn.onClick.AddListener(() => { OnClickColorBtn(btn.GetComponent<Image>().color); });
            }

            OnBodyBtnClick();
            m_Other.SetActive(false);
        }

        private void OnBodyBtnClick()
        {
            _currListType = EDressType.Body;
            _currCollection = _body;
            OnSourceReady();
        }

        private void OnShoesBtnClick()
        {
            _currListType = EDressType.Shoes;
            _currCollection = _shoes;
            OnSourceReady();
        }

        private void OnFaceBtnClick()
        {
            _currListType = EDressType.Face;
            _currCollection = _face;
            OnSourceReady();
        }

        private void OnClothesBtnClick()
        {
            _currListType = EDressType.Clothes;
            _currCollection = _clothes;
            OnSourceReady();
        }

        private void OnPantsBtnClick()
        {
            _currListType = EDressType.Pants;
            _currCollection = _pants;
            OnSourceReady();
        }

        private void OnEyesBtnClick()
        {
            _currListType = EDressType.Eyes;
            _currCollection = _eyes;
            OnSourceReady();
        }

        private void OnEarsBtnClick()
        {
            _currListType = EDressType.Ears;
            _currCollection = _ears;
            OnSourceReady();
        }

        private void OnMouthBtnClick()
        {
            _currListType = EDressType.Mouth;
            _currCollection = _mouth;
            OnSourceReady();
        }

        private void OnNosesBtnClick()
        {
            _currListType = EDressType.Noses;
            _currCollection = _noses;
            OnSourceReady();
        }

        private void OnBrowBtnClick()
        {
            _currListType = EDressType.Brow;
            _currCollection = _brow;
            OnSourceReady();
        }

        private void OnEyesSubBtnClick()
        {
            _currListType = EDressType.EyesSub;
            _currCollection = _eyesSub;
            OnSourceReady();
        }

        private void OnNosesSubBtnClick()
        {
            _currListType = EDressType.NosesSub;
            _currCollection = _nosesSub;
            OnSourceReady();
        }

        private void OnLipBtnClick()
        {
            _currListType = EDressType.Lip;
            _currCollection = _lip;
            OnSourceReady();
        }

        private void OnNavelBtnClick()
        {
            _currListType = EDressType.Navel;
            _currCollection = _navel;
            OnSourceReady();
        }

        private void OnHairBtnClick()
        {
            _currListType = EDressType.Hair;
            _currCollection = _hair;
            OnSourceReady();
        }

        private void OnHatBtnClick()
        {
            _currListType = EDressType.Hat;
            _currCollection = _hat;
            OnSourceReady();
        }

        private void OnSourceReady()
        {
            var count = _currCollection.Count;
            if (_currListType != EDressType.None && _currListType != EDressType.Body && _currCollection[0] != 0)
            {
                //在_currCollection最前面添加
                _currCollection.Insert(0, 0);
            }

            if (!isInit)
            {
                m_GridSkinList.Init(m_DressPrefab, count);
                isInit = true;
            }
            else
            {
                m_GridSkinList.UpdateCount(count);
            }

            if (_currListType == EDressType.Clothes || _currListType == EDressType.Pants)
            {
                m_ColorPart.SetActive(true);
            }
            else
            {
                m_ColorPart.SetActive(false);
            }
        }

        private void OnClickColorBtn(Color color)
        {
            m_Role.OnClickColorBtn(_currListType, color);
        }

        private void OnGridItem(int index, MonoBehaviour itemComponent)
        {
            var script = (DressItem)itemComponent;
            script.Refresh(_currCollection[index], _currListType, OnClickItem);
        }

        private void OnClickItem(int id)
        {
            if (_currListType == EDressType.Body)
            {
                m_Other.SetActive(true);
                m_BodyBtn.gameObject.SetActive(false);
                m_Role.SetBody(id);
                OnFaceBtnClick();
            }
            else
            {
                m_Role.SetDress(_currListType, id);
            }
        }

        private void Update()
        {
            if (!m_Other.activeSelf) return;
            var index = (int)Mathf.Floor(-m_ContentTransform.anchoredPosition.x / 175);
            index = Mathf.Clamp(index, 0, 14);
            if (index != _currSelectIndex)
            {
                RefreshBtn(index);
                _currSelectIndex = index;
            }
        }

        private void RefreshBtn(int index)
        {
            switch (index)
            {
                case 0:
                    OnFaceBtnClick();
                    break;
                case 1:
                    OnHairBtnClick();
                    break;
                case 2:
                    OnClothesBtnClick();
                    break;
                case 3:
                    OnPantsBtnClick();
                    break;
                case 4:
                    OnShoesBtnClick();
                    break;
                case 5:
                    OnHatBtnClick();
                    break;
                case 6:
                    OnEyesBtnClick();
                    break;
                case 7:
                    OnNosesBtnClick();
                    break;
                case 8:
                    OnMouthBtnClick();
                    break;
                case 9:
                    OnBrowBtnClick();
                    break;
                case 10:
                    OnEyesSubBtnClick();
                    break;
                case 11:
                    OnNosesSubBtnClick();
                    break;
                case 12:
                    OnLipBtnClick();
                    break;
                case 13:
                    OnEarsBtnClick();
                    break;
                case 14:
                    OnNavelBtnClick();
                    break;
            }
        }

        private void RefreshPantsOrder()
        {
        }

        protected override void OnDestroy()
        {
            SoundManager.Instance.StopCurPlayBgm();
            SoundManager.Instance.PlayBGM(SoundDirConst.Bgm + SoundNameUtil.MainBgm, true);
            base.OnDestroy();
        }
    }
}