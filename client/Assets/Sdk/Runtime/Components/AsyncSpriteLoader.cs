using System;
using System.Collections.Generic;
using Sdk.Runtime.Base;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Sdk.Runtime.Components
{
    public enum SpritePathType
    {
        None,
        Profile,
        Chapter,
        BodyIcon,
        ShoesIcon,
        ClothesIcon,
        PantsIcon,
        FaceIcon,
        EyesIcon,
        EarsIcon,
        MouthIcon,
        NosesIcon,
        HairIcon,
        BrowIcon, 
        HatIcon,
        EyesSubIcon,
        NosesSubIcon,
        LipIcon,
        NavelIcon,
        Icon,
        Body,
        ShoesFat,
        ShoesThin,  
        ClothesFat,
        ClothesThin,  
        PantsFat,
        PantsThin,
        Face,
        Eyes,
        Ears,
        Mouth,
        Noses,
        Brow,
        EyesSub,
        NosesSub,
        Lip,
        Navel,
        Hat,
        HatHair0,
        Hair0,
        HatHair1,
        Hair1,
        Common,
    }

    [RequireComponent(typeof(Image))]
    public class AsyncSpriteLoader : MonoBehaviour
    {
        private Image target;

        private bool  nativeSizeState;
        private float alphaColor;

        private string spriteLoadPath; //图片的完整加载路径

        private SpritePathType spriteType = SpritePathType.None;

        private AsyncResourceLoadHandle<Sprite> _handler;
        public event Action<bool>               OnLoadCompleteEvent;

        /// <summary>
        /// 设置图片加载信息
        /// </summary>
        /// <param name="spriteName">图片名字</param>
        /// <param name="spritePathType">图片类型，根据该类型获取加载路径</param>
        /// <param name="nativeSize">是否设置图片nativeSize</param>
        public void SetLoadData(string spriteName, SpritePathType spritePathType, bool nativeSize = false)
        {
            SpritePathType = spritePathType;
            nativeSizeState = nativeSize; 
            SpriteLoadPath = GetLoadPath(spritePathType) + spriteName + ".png"; //必须在最后赋值   
        }

        public void SetSprite(Sprite sprite)
        {
            target.sprite = sprite;
        }

        private string SpriteLoadPath
        {
            get => this.spriteLoadPath;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    OnLoadCompleteEvent?.Invoke(false);
                    return;
                }

                if (this.spriteLoadPath == value)
                {
                    OnLoadCompleteEvent?.Invoke(true);
                    return;
                }

                this.spriteLoadPath = value;
                if (this.target != null)
                {
                    this.OnSpriteChanged();
                }
            }
        }

        private SpritePathType SpritePathType
        {
            get => this.spriteType;
            set
            {
                if (spriteType != value)
                {
                    spriteType = value;
                }
            }
        }

        protected virtual void Awake()
        {
            this.target = this.GetComponent<Image>();
            if (target == null)
            {
                return;
            }

            alphaColor = this.target.color.a;

            if (!string.IsNullOrEmpty(spriteLoadPath))
            {
                this.OnSpriteChanged();
            }
        }

        protected virtual void OnSpriteChanged()
        {
            var curColor = target.color;
            this.target.color = new Color(curColor.r, curColor.g, curColor.b, 0);
            LoadSpriteAsync();
        }


        async void LoadSpriteAsync()
        {
            Release();
            _handler = ResLoaderHelper.EnsureLoadSpriteAsync(SpriteLoadPath);
            _handler.Completed += OnCompleted;
        }

        private void OnCompleted(AsyncResourceLoadHandle<Sprite> obj)
        {
            target.sprite = obj.Result;
            var curColor = target.color;
            this.target.color = new Color(curColor.r, curColor.g, curColor.b, alphaColor);
            SetNativeSize();
        }

        void Release()
        {
            if (_handler != null && _handler.IsValid())
            {
                _handler.Release();
            }
        }

        void SetNativeSize()
        {
            if (nativeSizeState)
            {
                this.target.SetNativeSize();
            }
        }


        string GetLoadPath(SpritePathType pathType)
        {
            switch (pathType)
            {
                case SpritePathType.Profile:
                    return PathManager.TexturePathRoot + "Profile/";
                case SpritePathType.Chapter:
                    return PathManager.ChapterPathRoot;       
                case SpritePathType.BodyIcon:
                    return PathManager.TexturePathRoot + "Body/Icon/"; 
                case SpritePathType.ShoesIcon:
                    return PathManager.TexturePathRoot + "Shoes/Icon/"; 
                case SpritePathType.ClothesIcon:
                    return PathManager.TexturePathRoot + "Clothes/Icon/"; 
                case SpritePathType.PantsIcon:
                    return PathManager.TexturePathRoot + "Pants/Icon/";           
                case SpritePathType.FaceIcon:
                    return PathManager.TexturePathRoot + "Face/Icon/";  
                case SpritePathType.EyesIcon:
                    return PathManager.TexturePathRoot + "Eyes/Icon/";      
                case SpritePathType.EarsIcon:
                    return PathManager.TexturePathRoot + "Ears/Icon/";    
                case SpritePathType.MouthIcon:
                    return PathManager.TexturePathRoot + "Mouth/Icon/";     
                case SpritePathType.NosesIcon:
                    return PathManager.TexturePathRoot + "Noses/Icon/";    
                case SpritePathType.HairIcon:
                    return PathManager.TexturePathRoot + "Hair/Icon/";   
                case SpritePathType.BrowIcon:
                    return PathManager.TexturePathRoot + "Brow/Icon/";               
                case SpritePathType.NosesSubIcon:
                    return PathManager.TexturePathRoot + "NosesSub/Icon/";            
                case SpritePathType.EyesSubIcon:
                    return PathManager.TexturePathRoot + "EyesSub/Icon/";              
                case SpritePathType.LipIcon:
                    return PathManager.TexturePathRoot + "Lip/Icon/";               
                case SpritePathType.NavelIcon:
                    return PathManager.TexturePathRoot + "Navel/Icon/";    
                case SpritePathType.HatIcon:
                    return PathManager.TexturePathRoot + "Hat/Icon/"; 
                case SpritePathType.Icon:
                    return PathManager.TexturePathRoot + "Icon/";    
                case SpritePathType.Body:
                    return PathManager.TexturePathRoot + "Body/";     
                case SpritePathType.ShoesFat:
                    return PathManager.TexturePathRoot + "Shoes/Fat/";             
                case SpritePathType.ShoesThin:
                    return PathManager.TexturePathRoot + "Shoes/Thin/";           
                case SpritePathType.ClothesFat:
                    return PathManager.TexturePathRoot + "Clothes/Fat/";             
                case SpritePathType.ClothesThin:
                    return PathManager.TexturePathRoot + "Clothes/Thin/";           
                case SpritePathType.PantsFat:
                    return PathManager.TexturePathRoot + "Pants/Fat/";             
                case SpritePathType.PantsThin:
                    return PathManager.TexturePathRoot + "Pants/Thin/";        
                case SpritePathType.Face:
                    return PathManager.TexturePathRoot + "Face/";  
                case SpritePathType.Eyes:
                    return PathManager.TexturePathRoot + "Eyes/";      
                case SpritePathType.Ears:
                    return PathManager.TexturePathRoot + "Ears/";      
                case SpritePathType.Mouth:
                    return PathManager.TexturePathRoot + "Mouth/";     
                case SpritePathType.Noses:
                    return PathManager.TexturePathRoot + "Noses/";            
                case SpritePathType.Brow:
                    return PathManager.TexturePathRoot + "Brow/";      
                case SpritePathType.EyesSub:
                    return PathManager.TexturePathRoot + "EyesSub/";         
                case SpritePathType.NosesSub:
                    return PathManager.TexturePathRoot + "NosesSub/";          
                case SpritePathType.Lip:
                    return PathManager.TexturePathRoot + "Lip/";          
                case SpritePathType.Navel:
                    return PathManager.TexturePathRoot + "Navel/";
                case SpritePathType.Hat:
                    return PathManager.TexturePathRoot + "Hat/";    
                case SpritePathType.Hair0:
                    return PathManager.TexturePathRoot + "Hair/Hair0/NoHat/";     
                case SpritePathType.Hair1:
                    return PathManager.TexturePathRoot + "Hair/Hair1/NoHat/";       
                case SpritePathType.HatHair0:
                    return PathManager.TexturePathRoot + "Hair/Hair0/WithHat/";     
                case SpritePathType.HatHair1:
                    return PathManager.TexturePathRoot + "Hair/Hair1/WithHat/";   
                case SpritePathType.Common:
                    break;
                case SpritePathType.None:
                default:
                    return "";
            }

            return "";
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}