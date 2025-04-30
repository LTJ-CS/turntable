using UnityEngine;
using UnityEngine.UI;

namespace Sdk.Runtime.Components
{
    /// <summary>
    /// 直接加载 jpg 或 png 图片到 Sprite 上
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class RawSpriteLoader : MonoBehaviour
    {
        [SerializeField, Tooltip("Sprite 图片, 越小越好, 必须以 .bytes 为后缀, 规避 Unity 对图片的格式转换")]
        private TextAsset m_SpriteImage;
        
        [SerializeField]
        [Tooltip("Image 类型")]
        private Image.Type m_SpriteType = Image.Type.Simple;

        // Start is called before the first frame update
        void Start()
        {
            RefreshSprite();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            RefreshSprite();
        }
#endif

        /// <summary>
        /// 刷新图片显示
        /// </summary>
        private void RefreshSprite()
        {
            // 释放这张图片
            var image = GetComponent<Image>();
            if (image.overrideSprite != null)
            {
                DestroyImmediate(image.overrideSprite.texture);
                image.overrideSprite = null;
            }

            if (m_SpriteImage == null)
                return;

            // 加载背景图片
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, 1, false);
            if (!texture.LoadImage(m_SpriteImage.bytes, true))
            {
                Debug.LogError($"{gameObject.name} 加载图片失败");
                return;
            }

            image.type = m_SpriteType;
            
            // 参考: https://forum.unity.com/threads/issues-texturewrapmode-in-webgl.446453/
            // According to the GLES 2.0.24 $3.8.2: Non-power-of-two textures must have a wrap mode of CLAMP_TO_EDGE. Which means that repeat wrap mode can not be used in WebGL 1.0 if your texture is NPOT.
            #if UNITY_WEBGL
            if (image.type == Image.Type.Tiled && (Mathf.IsPowerOfTwo(texture.width) == false || Mathf.IsPowerOfTwo(texture.height) == false))
                texture.wrapMode = TextureWrapMode.Clamp;
            #endif

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(texture.width / 2f, texture.height / 2f), 100f, 1, SpriteMeshType.FullRect);

            // 我们需要使用 overrideSprite, 而不是 sprite, 因为在 Editor 中 sprite 会被 unity 序列化到场景文件中, 但我们希望不被序列化, 动态加载设置的 Sprite 只是在运行时有效
            image.overrideSprite = sprite;
        }

        private void OnDestroy()
        {
            // 释放这张图片
            var image = GetComponent<Image>();
            if (image.overrideSprite != null)
                Destroy(image.overrideSprite.texture);
        }
    }
}