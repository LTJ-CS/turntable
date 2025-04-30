using UnityEngine;

namespace GameScript.Runtime.Render.Components
{
    /// <summary>
    /// 测试罐子的静态合指渲染, 把本组件挂到 ScrewStaticBatchRenderer 所在的 GameObject 上运行即可测试
    /// </summary>
    [RequireComponent(typeof(ScrewStaticBatchRenderer))]
    public class TestScrewStaticBatchRender : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("测试缩放")]
        [Range(0, 3)]
        private float m_Scale = 1f;

        private void Start()
        {
            var screws = new ScrewStaticBatchRenderer.ScrewRenderData[2];
            screws[0] = new ScrewStaticBatchRenderer.ScrewRenderData()
                        {
                            Id            = 1,
                            ColorId       = 1,
                            LocalPosition = new Vector2(0, 0),
                            IsActive      = true
                        };
            screws[1] = new ScrewStaticBatchRenderer.ScrewRenderData()
                        {
                            Id            = 2,
                            ColorId       = 2,
                            LocalPosition = new Vector2(1, 0),
                            IsActive      = false
                        };

            var render = GetComponent<ScrewStaticBatchRenderer>();
            render.AddScrews(screws);

            render.SetScrewScale(m_Scale);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;
            var render = GetComponent<ScrewStaticBatchRenderer>();
            render.SetScrewScale(m_Scale);
        }
#endif
    }
}