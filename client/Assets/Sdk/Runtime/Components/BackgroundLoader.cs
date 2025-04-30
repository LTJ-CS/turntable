using UnityEngine;
using UnityEngine.UI;

namespace Sdk.Runtime.Components
{
    /// <summary>
    /// 背景加载器, 去掉 Unity 对背景图片的转换, 直接以 jpg 的格式来加载背景图片, 减少首包的大小
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class BackgroundLoader : MonoBehaviour
    {
        [SerializeField, Tooltip("背景图片, 越小越好, 必须以 .bytes 为后缀, 规避 Unity 对图片的格式转换")]
        private TextAsset m_BackgroundImage;
        
        // Start is called before the first frame update
        void Start()
        {
            RefreshImage();
        }
        
#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_BackgroundImage == null)
            {
                // 释放这张图片
                var image = GetComponent<RawImage>();
                if (image.texture != null)
                {
                    Destroy(image.texture);
                    image.texture = null;
                }

                return;
            }
            RefreshImage();
        }
#endif

        private void RefreshImage()
        {
            // 加载背景图片
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, 1, false);
            if (!texture.LoadImage(m_BackgroundImage.bytes))
            {
                Debug.LogError($"{gameObject.name} 加载图片失败");
                return;
            }
            
            var image = GetComponent<RawImage>();
            image.texture = texture;
        }

        private void OnDestroy()
        {
            // 释放这张图片
            var image = GetComponent<RawImage>();
            Destroy(image.texture);
        }
    }
}

