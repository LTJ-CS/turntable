using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace GameScript.Runtime.Render
{
    /// <summary>
    /// RollBgEffect辅助类，用于矫正shader参数
    /// </summary>
    public class RollBgEffectAdjuster : MonoBehaviour
    {
        static readonly int HorizontalSpeed = Shader.PropertyToID("_HorizontalSpeed");
        static readonly int VerticalSpeed = Shader.PropertyToID("_VerticalSpeed");
        static readonly int ScreenPixel = Shader.PropertyToID("_ScreenPixel");
        [SerializeField] float m_Scale = 1f;
        [SerializeField] float m_HorizontalSpeed = 10f;
        [SerializeField] float m_VerticalSpeed = 5f;
        
        // Start is called before the first frame update
        void Start()
        {
            if (m_Scale == 0f)
            {
                m_Scale = 1f;
            }
            else if (m_Scale < 0f)
            {
                m_Scale = Mathf.Abs(m_Scale);
            }
            UpdateParam();
        }

        /// <summary>
        /// UpdateParam
        /// </summary>
        void UpdateParam()
        {
            var image = GetComponent<Image>();
            if (image == null || image.sprite == null)
            {
                return;
            }

            // 获取image的像素宽高
            float imagePixelWidth = image.rectTransform.rect.width;
            float imagePixelHeight = image.rectTransform.rect.height;
            // 获取纹理的真实像素宽高，乘以缩放比
            float texturePixelWidth = image.sprite.texture.width * m_Scale;
            float texturePixelHeight = image.sprite.texture.height * m_Scale;

            image.material.SetFloat(HorizontalSpeed, m_HorizontalSpeed);
            image.material.SetFloat(VerticalSpeed, m_VerticalSpeed);
            image.material.SetVector(ScreenPixel, new Vector4(imagePixelWidth, imagePixelHeight, texturePixelWidth, texturePixelHeight));
        }
    }
}
