using System;
using System.Collections.Generic;
using System.Linq;
using GameScript.Runtime.GameLogic;
using GameScript.Runtime.GameLogic.Events;
using Sdk.Runtime.Components;
using UnityEngine;
using UnityEngine.UI;

namespace GameScript.Runtime.UI
{
    public class RoleItem : MonoBehaviour
    {
        [SerializeField] private GameObject        m_Fat;
        [SerializeField] private GameObject        m_Thin;
        [SerializeField] private AsyncSpriteLoader m_Face;
        [SerializeField] private AsyncSpriteLoader m_Clothes;
        [SerializeField] private AsyncSpriteLoader m_ClothesColor;
        [SerializeField] private AsyncSpriteLoader m_ClothesCover;
        [SerializeField] private AsyncSpriteLoader m_Pants;
        [SerializeField] private AsyncSpriteLoader m_PantsColor;
        [SerializeField] private AsyncSpriteLoader m_PantsCover;
        [SerializeField] private AsyncSpriteLoader m_Shoes;
        [SerializeField] private AsyncSpriteLoader m_Shoes0;
        [SerializeField] private AsyncSpriteLoader m_Eyes;
        [SerializeField] private AsyncSpriteLoader m_Ears;
        [SerializeField] private AsyncSpriteLoader m_Mouth;
        [SerializeField] private AsyncSpriteLoader m_Noses;
        [SerializeField] private AsyncSpriteLoader m_Hair0;
        [SerializeField] private AsyncSpriteLoader m_Hair1;
        [SerializeField] private AsyncSpriteLoader m_Brow;
        [SerializeField] private AsyncSpriteLoader m_EyesSub;
        [SerializeField] private AsyncSpriteLoader m_NosesSub;
        [SerializeField] private AsyncSpriteLoader m_Lip;
        [SerializeField] private AsyncSpriteLoader m_Navel;
        [SerializeField] private AsyncSpriteLoader m_Hat;
        [SerializeField] private Sprite            m_Empty;

        private Dictionary<EBodyType, SpritePathType> clothesDict = new Dictionary<EBodyType, SpritePathType>()
                                                                    {
                                                                        { EBodyType.Fat, SpritePathType.ClothesFat },
                                                                        { EBodyType.Thin, SpritePathType.ClothesThin }
                                                                    };

        private Dictionary<EBodyType, SpritePathType> pantsDict = new Dictionary<EBodyType, SpritePathType>()
                                                                  {
                                                                      { EBodyType.Fat, SpritePathType.PantsFat },
                                                                      { EBodyType.Thin, SpritePathType.PantsThin }
                                                                  };

        private EBodyType _nowEBodyType = EBodyType.Fat;

        private List<int> _withHat0 = new List<int>()
                                      {
                                          3
                                      };

        private List<int> _withHat1 = new List<int>()
                                      {
                                          1, 3, 7, 8
                                      };

        private List<int> _clothesColor = new List<int>()
                                          {
                                              1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 16, 18, 19,20
                                          };


        private List<int> _pantsColor = new List<int>()
                                        {
                                            1, 6, 7, 10,11,14,15,18
                                        };

        private List<int> _hairSub = new List<int>()
                                     {
                                         1, 2, 3, 4, 5, 8, 9, 99
                                     };
        
        private void Start()
        {
            SetBody(GameInstance.Instance.SelectCollection[EDressType.Body]);
            MainUIEvents.RefreshRole += RefreshAll;
        }

        private void OnDestroy()
        {
            MainUIEvents.RefreshRole -= RefreshAll;
        }

        public void OnClickColorBtn(EDressType type, Color color)
        {
            if (type == EDressType.Clothes)
            {
                GameInstance.Instance.ClothesColor = color;
                m_ClothesCover.gameObject.SetActive(true);
                m_ClothesCover.GetComponent<Image>().color = color;
            }
            
            if (type == EDressType.Pants)
            {
                GameInstance.Instance.PantsColor = color;
                m_PantsCover.gameObject.SetActive(true);
                m_PantsCover.GetComponent<Image>().color = color;
            }
        }
        
        private void RefreshAll()
        {
            foreach (var pair in GameInstance.Instance.SelectCollection.ToList())
            {
                SetDress(pair.Key, pair.Value);
            }
        }      
        
        public void SetBody(int id)
        {
            GameInstance.Instance.SelectCollection[EDressType.Body] = id;
            _nowEBodyType = (EBodyType)id;
            if (_nowEBodyType == EBodyType.Fat)
            {
                m_Fat.SetActive(true);
                m_Thin.SetActive(false);
            }
            else
            {
                m_Fat.SetActive(false);
                m_Thin.SetActive(true);
            }
            RefreshAll();
        }

        /// <summary>
        /// 设置单个部位装扮
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        public void SetDress(EDressType type, int id)
        {
            if (id == 0)
            {
                SetEmpty(type);
                return;
            }

            var _selectCollection = GameInstance.Instance.SelectCollection;
            switch (type)
            {
                case EDressType.Hair:
                    _selectCollection[EDressType.Hair] = id;
                    RefreshHair();
                    break;
                case EDressType.Face:
                    _selectCollection[EDressType.Face] = id;
                    m_Face.SetLoadData(id.ToString(), SpritePathType.Face, true);
                    RefreshHair();
                    break;
                case EDressType.Clothes:
                    _selectCollection[EDressType.Clothes] = id;
                    m_ClothesCover.GetComponent<Image>().color = Color.white;
                    m_ClothesCover.gameObject.SetActive(false);
                    if (_clothesColor.Contains(id))
                    {
                        m_ClothesColor.gameObject.SetActive(true);
                        m_ClothesCover.SetLoadData($"{id}-3", clothesDict[_nowEBodyType], true);
                        m_ClothesColor.SetLoadData($"{id}-2", clothesDict[_nowEBodyType], true);
                        m_Clothes.SetLoadData($"{id}-1", clothesDict[_nowEBodyType], true);
                        if (GameInstance.Instance.ClothesColor!=Color.white)
                        {
                            m_ClothesCover.gameObject.SetActive(true);
                            m_ClothesCover.GetComponent<Image>().color = GameInstance.Instance.ClothesColor;
                        }
                    }
                    else
                    {
                        m_ClothesColor.gameObject.SetActive(false);
                        m_Clothes.SetLoadData(id.ToString(), clothesDict[_nowEBodyType], true);
                    }

                    break;
                case EDressType.Pants:
                    _selectCollection[EDressType.Pants] = id;
                    m_PantsCover.GetComponent<Image>().color = Color.white;
                    m_PantsCover.gameObject.SetActive(false);
                    if (_pantsColor.Contains(id))
                    {
                        m_PantsCover.SetLoadData($"{id}-3", pantsDict[_nowEBodyType], true);
                        m_PantsColor.SetLoadData($"{id}-2", pantsDict[_nowEBodyType], true);
                        m_Pants.SetLoadData($"{id}-1", pantsDict[_nowEBodyType], true);
                        m_PantsColor.gameObject.SetActive(true);
                        if (GameInstance.Instance.PantsColor!=Color.white)
                        {
                            m_PantsCover.gameObject.SetActive(true);
                            m_PantsCover.GetComponent<Image>().color = GameInstance.Instance.PantsColor;
                        }
                    }
                    else
                    {
                        m_PantsColor.gameObject.SetActive(false);
                        m_Pants.SetLoadData(id.ToString(), pantsDict[_nowEBodyType], true);
                    }

                    break;
                case EDressType.Hat:
                    _selectCollection[EDressType.Hat] = id;
                    RefreshHair();
                    m_Hat.SetLoadData(id.ToString(), SpritePathType.Hat, true);
                    break;
                case EDressType.Body:
                    break;
                case EDressType.Shoes:
                    _selectCollection[EDressType.Shoes] = id;
                    if (_nowEBodyType == EBodyType.Fat)
                    {
                        m_Shoes.SetLoadData(id.ToString(), SpritePathType.ShoesFat, true);
                    }
                    else
                    {
                        m_Shoes.SetLoadData(id.ToString(), SpritePathType.ShoesThin, true);
                    }

                    if (id == 11)
                    {
                        m_Shoes0.gameObject.SetActive(true);
                    if (_nowEBodyType == EBodyType.Fat)
                    {
                        m_Shoes0.SetLoadData(id+"-1", SpritePathType.ShoesFat, true);
                    }
                    else
                    {
                        m_Shoes0.SetLoadData(id+"-1", SpritePathType.ShoesThin, true);
                    }
                    }
                    else
                    {
                        m_Shoes0.gameObject.SetActive(false);
                    }
                    break;
                case EDressType.Eyes:
                    m_Eyes.SetLoadData(id.ToString(), SpritePathType.Eyes, true);
                    break;
                case EDressType.Ears:
                    m_Ears.SetLoadData(id.ToString(), SpritePathType.Ears, true);
                    break;
                case EDressType.Mouth:
                    m_Mouth.SetLoadData(id.ToString(), SpritePathType.Mouth, true);
                    break;
                case EDressType.Noses:
                    m_Noses.SetLoadData(id.ToString(), SpritePathType.Noses, true);
                    break;
                case EDressType.Brow:
                    m_Brow.SetLoadData(id.ToString(), SpritePathType.Brow, true);
                    break;
                case EDressType.Navel:
                    m_Navel.SetLoadData(id.ToString(), SpritePathType.Navel, true);
                    break;
                case EDressType.Lip:
                    m_Lip.SetLoadData(id.ToString(), SpritePathType.Lip, true);
                    break;
                case EDressType.NosesSub:
                    m_NosesSub.SetLoadData(id.ToString(), SpritePathType.NosesSub, true);
                    break;
                case EDressType.EyesSub:
                    m_EyesSub.SetLoadData(id.ToString(), SpritePathType.EyesSub, true);
                    break;
                case EDressType.None:
                    break;
            }
        }

        public void SetEmpty(EDressType type)
        {
            var _selectCollection = GameInstance.Instance.SelectCollection;
            switch (type)
            {
                case EDressType.Hair:
                    SetDress(EDressType.Hair, 99);
                    break;

                case EDressType.Face:
                    _selectCollection[EDressType.Face] = 0;
                    m_Face.SetSprite(m_Empty);
                    break;

                case EDressType.Clothes:
                    SetDress(EDressType.Clothes, 99);
                    break;

                case EDressType.Pants:
                    SetDress(EDressType.Pants, 99);
                    break;

                case EDressType.Hat:
                    _selectCollection[EDressType.Hat] = 0;
                    m_Hat.SetSprite(m_Empty);
                    RefreshHair();

                    break;

                case EDressType.Shoes:
                    SetDress(EDressType.Shoes, 99);
                    break;

                case EDressType.Eyes:
                    SetDress(EDressType.Eyes, 99);
                    break;

                case EDressType.Ears:
                    _selectCollection[EDressType.Ears] = 0;
                    m_Ears.SetSprite(m_Empty);
                    break;

                case EDressType.Mouth:
                    SetDress(EDressType.Mouth, 99);
                    break;

                case EDressType.Noses:
                    SetDress(EDressType.Noses, 99);
                    break;

                case EDressType.Brow:
                    _selectCollection[EDressType.Brow] = 0;
                    m_Brow.SetSprite(m_Empty);
                    break;

                case EDressType.Navel:
                    _selectCollection[EDressType.Navel] = 0;
                    m_Navel.SetSprite(m_Empty);
                    break;

                case EDressType.Lip:
                    _selectCollection[EDressType.Lip] = 0;
                    m_Lip.SetSprite(m_Empty);
                    break;

                case EDressType.NosesSub:
                    _selectCollection[EDressType.NosesSub] = 0;
                    m_NosesSub.SetSprite(m_Empty);
                    break;

                case EDressType.EyesSub:
                    _selectCollection[EDressType.EyesSub] = 0;
                    m_EyesSub.SetSprite(m_Empty);
                    break;

                case EDressType.Body:
                    // 身体类型通常不能设置为空，保持当前状态
                    break;

                case EDressType.None:
                    break;
            }
        }

        /// <summary>
        /// 刷新头发
        /// </summary>
        private void RefreshHair()
        {
            var _selectCollection = GameInstance.Instance.SelectCollection;
            var id = _selectCollection[EDressType.Hair];
            var faceId = _selectCollection[EDressType.Face];
            var hatId = _selectCollection[EDressType.Hat];
            if (faceId is 6 or 7)
            {
            }
            else
            {
                faceId = 1;
            }

            if (_withHat1.Contains(_selectCollection[EDressType.Hair]) && hatId > 0)
            {
                m_Hair1.SetLoadData(id + "-" + faceId, SpritePathType.HatHair1, true);
            }
            else
            {
                m_Hair1.SetLoadData(id + "-" + faceId, SpritePathType.Hair1, true);
            }

            if (_hairSub.Contains(id))
            {
                if (_withHat0.Contains(_selectCollection[EDressType.Hair]) && hatId > 0)
                {
                    m_Hair0.SetLoadData(id + "-" + faceId, SpritePathType.HatHair0, true);
                }
                else
                {
                    m_Hair0.SetLoadData(id + "-" + faceId, SpritePathType.Hair0, true);
                }

                m_Hair0.gameObject.SetActive(true);
            }
            else
            {
                m_Hair0.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 刷新嘴部
        /// </summary>
        private void RefreshLip()
        {
            var _selectCollection = GameInstance.Instance.SelectCollection;
            var faceId = _selectCollection[EDressType.Face];
            if (faceId is 6 or 7)
            {
            }
            else
            {
                faceId = 1;
            }
        }
    }
}