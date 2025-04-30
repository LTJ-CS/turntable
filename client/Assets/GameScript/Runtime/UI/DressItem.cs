using System;
using Sdk.Runtime.Components;
using UnityEngine;
using UnityEngine.UI;

namespace GameScript.Runtime.UI
{
    public class DressItem:MonoBehaviour
    {
        [SerializeField] private AsyncSpriteLoader m_Icon;
        [SerializeField] private Button            m_Button;
        [SerializeField] private GameObject         m_Default;
        private                  Action<int>       _callback;
        private                  int               itemId;

        private void Start()
        {
            m_Button.onClick.AddListener(OnClick);
        }

        public void Refresh(int id,EDressType type ,Action<int> callback)
        {
            itemId = id;
            _callback = callback;

            if (id == 0)
            {
                m_Default.SetActive(true);
                m_Icon.gameObject.SetActive(false);
                return;
            }
            else
            {
                m_Icon.gameObject.SetActive(true);
                m_Default.SetActive(false);
            }
            
            switch (type)
            {
                case EDressType.Body:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.BodyIcon,false);
                    break;
                case EDressType.Shoes:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.ShoesIcon,false);
                    break;
                case EDressType.Face:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.FaceIcon,false);
                    break;
                case EDressType.Clothes:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.ClothesIcon,false);
                    break;
                case EDressType.Pants:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.PantsIcon,false);
                    break;
                case EDressType.Eyes:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.EyesIcon,false);
                    break;
                case EDressType.Ears:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.EarsIcon,false);
                    break;
                case EDressType.Mouth:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.MouthIcon,false);
                    break;
                case EDressType.Noses:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.NosesIcon,false);
                    break;
                case EDressType.Brow:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.BrowIcon,false);
                    break;
                case EDressType.EyesSub:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.EyesSubIcon,false);
                    break;
                case EDressType.NosesSub:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.NosesSubIcon,false);
                    break;
                case EDressType.Lip:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.LipIcon,false);
                    break;
                case EDressType.Navel:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.NavelIcon,false);
                    break;
                case EDressType.Hair:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.HairIcon,false);
                    break;
                case EDressType.Hat:
                    m_Icon.SetLoadData(id.ToString(),SpritePathType.HatIcon,false);
                    break;
            }
            
        }

        private void OnClick()
        {
            _callback?.Invoke(itemId);
        }
    }
}