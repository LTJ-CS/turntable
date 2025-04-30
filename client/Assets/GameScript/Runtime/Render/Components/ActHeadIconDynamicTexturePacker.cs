using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GameScript.Runtime.Render.Components
{
    //将活动头像动态打入图集
    public class ActHeadIconDynamicTexturePacker : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("默认头像")]
        private Texture2D m_LocalTexture; // 默认头像

        private readonly int targetWidth  = 32;  // 目标宽度
        private readonly int targetHeight = 32;  // 目标高度
        private readonly int atlasWidth   = 256; // 图集的宽度
        private readonly int atlasHeight  = 128; // 图集的高度

        private RenderTexture atlasRenderTexture; // 用于存储生成的图集

        private int currentX = 0, currentY = 0; // 当前绘制位置 从左到右从下到上绘制的图集

        public static readonly string DefaultHeadIconName = "head_icon_default"; //默认头像名字

        private readonly Dictionary<string, Rect> textureUVRectDict = new Dictionary<string, Rect>(); // 存储生成的 texture 的uvRect

        private readonly List<UnityWebRequest> activeDownloads = new List<UnityWebRequest>(); // 活跃的下载请求，方便取消和释放

        private CommandBuffer _commandBuffer; //生成atlasRenderTexture 的cb

        private float widthPercent;
        private float heightPercent;
        private bool  isAddDefault = false;

        private void Awake()
        {
            // 创建一个新的 RenderTexture
            atlasRenderTexture = RenderTexture.GetTemporary(atlasWidth, atlasHeight, 0, RenderTextureFormat.ARGB32);
            isAddDefault = false;
            currentX = 0;
            currentY = 0; //从下到上绘制
            widthPercent = targetWidth / (float)atlasWidth;
            heightPercent = targetHeight / (float)atlasHeight;
        }

        public void AddDefaultTexture()
        {
            if (isAddDefault)
                return;
            isAddDefault = true;
            AddTexture(m_LocalTexture, DefaultHeadIconName);
        }


        //从远端下载图片，下载完成后设置RawImage
        public void AddDownloadImg(string url, RawImage targetImage)
        {
            AddDefaultTexture();
            DoDownloadImg(url, targetImage);
        }

        async void DoDownloadImg(string url, RawImage rawImgComponent)
        {
            if (textureUVRectDict.TryGetValue(url, out var rect))
            {
                if (rawImgComponent != null)
                {
                    SetRawImage(rawImgComponent, rect);
                }

                return;
            }

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url, true);
            request.disposeDownloadHandlerOnDispose = true;
            try
            {
                activeDownloads.Add(request);
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    if (this == null || rawImgComponent == null)
                    {
                        return;
                    }

                    await UniTask.DelayFrame(2);
                }
                
                if (this == null || rawImgComponent == null)
                {
                    return;
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    if (texture != null)
                    {
                        bool isAdd = AddTexture(texture, url);
                        if (isAdd)
                        {
                            textureUVRectDict.TryGetValue(url, out var rectValue);
                            SetRawImage(rawImgComponent, rectValue);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (activeDownloads.Contains(request))
                {
                    activeDownloads.Remove(request);
                    if (request != null && request.isDone)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        if (texture != null)
                        {   
                            DelayDestroyTexture(texture).Forget();
                        }
                    }
                   
                        
                    request.Dispose();
                }
            }
            
            void SetRawImage(RawImage rawImage, Rect uvRect)
            {
                if (rawImage == null)
                    return;
                rawImage.texture = atlasRenderTexture;
                rawImage.uvRect = uvRect;
                rawImage.color = new Vector4(uvRect.x, uvRect.y, uvRect.width, uvRect.height);

                if (!rawImage.enabled)
                    rawImage.enabled = true;
            }
        }
        
        // 因为  AddTexture() 中的复制纹理还在进行中，必须延迟删除纹理
        async UniTask DelayDestroyTexture(Texture2D textureToDestroy)
        {
            await UniTask.DelayFrame(2);
            Destroy(textureToDestroy);
        }

        private bool AddTexture(Texture2D texture, string url)
        {
            try
            {
                if (currentX + targetWidth > atlasWidth)
                {
                    currentX = 0;
                    currentY += targetHeight;
                    if (currentY + targetHeight > atlasHeight)
                    {
                        Debug.LogWarning("Texture atlas overflow.");
                        return false;
                    }
                }
                else
                {
                    if (currentY + targetHeight > atlasHeight)
                    {
                        Debug.LogWarning("Texture atlas overflow.");
                        return false;
                    }
                }

                if (_commandBuffer == null)
                    _commandBuffer = new CommandBuffer();

                _commandBuffer.SetRenderTarget(atlasRenderTexture);
                var tRect = new Rect(currentX, currentY, targetWidth, targetHeight);
                _commandBuffer.SetViewport(tRect);
                _commandBuffer.Blit(texture, BuiltinRenderTextureType.CurrentActive);
                Graphics.ExecuteCommandBuffer(_commandBuffer);
                _commandBuffer.Clear();
                int column = currentX / targetWidth;
                int row = currentY / targetHeight;
                var rect = new Rect(column * targetWidth / (float)atlasWidth, row * targetHeight / (float)atlasHeight, widthPercent, heightPercent);
                textureUVRectDict.TryAdd(url, rect);

                // 更新绘制位置
                currentX += targetWidth;
           
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        private void ClearRenderTexture()
        {
            if (_commandBuffer == null)
                return;
            // 清除纹理
            _commandBuffer.Clear();
            _commandBuffer.SetRenderTarget(atlasRenderTexture);
            _commandBuffer.ClearRenderTarget(true, true, Color.clear);
            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }

        private void ResetData()
        {
            isAddDefault = false;
            CancelActiveDownloads();
            UnloadRemoteSprite();
            currentX = 0;
            currentY = 0; //从下到上绘制
            ClearRenderTexture();
        }


        private void CancelActiveDownloads()
        {
            if (activeDownloads == null)
                return;
            foreach (var request in activeDownloads)
            {
                if (request != null)
                {
                    if (request.isDone)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        if(texture != null)
                            DelayDestroyTexture(texture).Forget();
                    }
                    request.Dispose();
                }
            }

            activeDownloads.Clear();
        }

        private void UnloadRemoteSprite()
        {
            try
            {
                // 卸载远端生成的 Sprite
                List<string> keysToRemove = new List<string>();
                foreach (var key in textureUVRectDict.Keys)
                {
                    if (key != null && !key.Equals(DefaultHeadIconName))
                    {
                        keysToRemove.Add(key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    textureUVRectDict.Remove(key);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("UnloadRemoteTextures failed: " + ex.Message);
            }
        }

        private void OnDestroy()
        {
            ResetData();
            textureUVRectDict.Clear();
            if (_commandBuffer != null)
                _commandBuffer.Dispose();
            if (atlasRenderTexture != null)
            {
                RenderTexture.ReleaseTemporary(atlasRenderTexture);
                atlasRenderTexture = null;
            }
        }
    }
}