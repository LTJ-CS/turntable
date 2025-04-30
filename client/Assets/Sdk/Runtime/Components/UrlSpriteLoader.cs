using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Sdk.Runtime.Components
{
    [RequireComponent(typeof(Image))]
    public class UrlSpriteLoader : MonoBehaviour
    {
        private Image _targetImg;

        private string         _url;
        private Action<string> _errorCallback;
        Action                 _successCallback;

        private Texture2D _curTexture;

        /// <summary>
        /// 使用远程 url 地址设置图片 image 的显示内容
        /// </summary>
        /// <param name="imgUrl">图片url</param>
        /// <param name="errorCallback">加载失败的时候触发这个回调</param>
        /// <param name="successCallback">加载成功回调</param>
        public async UniTask SetUrl(string imgUrl, Action<string> errorCallback, Action successCallback = null)
        {
            if (_targetImg == null)
                _targetImg = GetComponent<Image>();

            if (imgUrl.Equals(_url))
                return;
            if (_curTexture != null)
            {
                Destroy(_curTexture);
                _curTexture = null;
            }

            // 加载前隐藏
            var color = _targetImg.color;
            color.a = 0f;
            _targetImg.color = color;
            _errorCallback = errorCallback;
            _successCallback = successCallback;
            _url = imgUrl;
            await DownloadImage(imgUrl); // 进行图片下载
        }


        /// <summary>
        /// 执行图片下载
        /// </summary>
        /// <param name="url"></param>
        async UniTask DownloadImage(string url)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            request.SendWebRequest();
            while (!request.isDone)
            {
                await UniTask.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                _errorCallback?.Invoke(request.error);
            }
            else
            {
                if (gameObject == null || !url.Equals(_url))
                {
                    return;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                _curTexture = texture;
                Sprite sprite = TextureToSprite(texture);
                _targetImg.sprite = sprite;
                // 加载完毕后显示
                var color = _targetImg.color;
                color.a = 1f;
                _targetImg.color = color;
                _successCallback?.Invoke();
            }
        }


        /// <summary>
        /// texture 转 sprite
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        private Sprite TextureToSprite(Texture2D texture)
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f); // 中心点
            Sprite sprite = Sprite.Create(texture, rect, pivot);
            return sprite;
        }

        /// <summary>
        /// 卸载图片
        /// </summary>
        private void UnloadImage()
        {
            if (_targetImg != null && _targetImg.sprite != null)
            {
                Destroy(_targetImg.sprite);
                _targetImg.sprite = null;
            }

            if (_curTexture != null)
            {
                Destroy(_curTexture);
                _curTexture = null;
            }
        }

        void OnDestroy()
        {
            UnloadImage();
        }
    }
}