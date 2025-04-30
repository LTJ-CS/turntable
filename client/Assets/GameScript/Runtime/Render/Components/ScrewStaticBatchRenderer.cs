using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.U2D;

namespace GameScript.Runtime.Render.Components
{
    /// <summary>
    /// 实现罐子的批量渲染, 并保持高光点始终保持向上, 实现这个类是为了减少为了实现高光向上带来的性能消耗, 并提升性能优化
    /// </summary>
    [AddComponentMenu("2D Object/Sprites/ScrewStaticBatchRenderer")]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class ScrewStaticBatchRenderer : MonoBehaviour
    {
        /// <summary>
        /// 每个罐子图片所在的图集, 注意名称必须匹配 Cup[id]_0 这种格式, 如 Cup3_0
        /// </summary>
        [FormerlySerializedAs("_spriteAtlas")]
        [SerializeField]
        [Tooltip("罐子图片所在的图集")]
        private SpriteAtlas m_SpriteAtlas;

        /// <summary>
        /// 罐子的缩放倍数
        /// </summary>
        private float _screwScale;

        /// <summary>
        /// 存储静态的罐子的名称列表, 减少gc
        /// </summary>
        private static readonly string[] SpriteNames =
        {
            "Hole1", // 孔的图片名称
            "Cup1_0",
            "Cup2_0",
            "Cup3_0",
            "Cup4_0",
            "Cup5_0",
            "Cup6_0",
            "Cup7_0",
        };

        /// <summary>
        /// 孔对应的颜色 Id, 对应 SpriteNames 的第0个
        /// </summary>
        private const int HoleColorId = 0;

        /// <summary>
        /// 罐子的信息
        /// </summary>
        public struct ScrewRenderData
        {
            /// <summary>
            /// 罐子的 Id
            /// </summary>
            public int Id;

            /// <summary>
            /// 罐子的颜色 Id
            /// </summary>
            public int ColorId;

            /// <summary>
            /// 罐子的本地坐标
            /// </summary>
            public Vector2 LocalPosition;

            /// <summary>
            /// 是否激活, 只有激活才会被显示
            /// </summary>
            public bool IsActive;
        }

        /// <summary>
        /// 保存当前渲染的罐子的列表
        /// </summary>
        private ScrewRenderData[] _screwDataList;

        /// <summary>
        /// 保存合并了所有罐子的顶点数据的 Mesh
        /// </summary>
        private Mesh _mesh;
        
        private void Awake()
        {
            _mesh = new Mesh();

            // 设置 Mesh
            GetComponent<MeshFilter>().mesh = _mesh;

            // 设置纹理
            var sprite = m_SpriteAtlas.GetSprite(SpriteNames[1]);
            var mainTexture = sprite?.texture;
            if (mainTexture == null)
            {
                Debug.LogError($"[ScrewStaticBatchRenderer] {gameObject.name}: 罐子图片不存在, Id: {SpriteNames[1]}, 无法渲染罐子");
                return;
            }

            var meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material.mainTexture = mainTexture;
            if (meshRenderer.renderingLayerMask == 0)
            {
                Debug.LogError($"[ScrewStaticBatchRenderer] {gameObject.name}: 罐子图片没有设置渲染层级, 无法渲染罐子");
            }
        }

        /// <summary>
        /// 设置罐子的缩放值
        /// </summary>
        /// <param name="scale">缩放值, [0 ~ 100]</param>
        public void SetScrewScale(float scale)
        {
            if (scale < 0 || scale > 100)
                return;

            _screwScale = scale;

            // 设置材质中罐子的缩放值
            GetComponent<MeshRenderer>().material.SetFloat("_Scale", _screwScale);
        }

        /// <summary>
        /// 添加罐子的数组到渲染器
        /// </summary>
        /// <param name="screws">指定要添加的罐子信息列表</param>
        public void AddScrews(IReadOnlyList<ScrewRenderData> screws)
        {
            if (screws.Count > 100)
            {
                Debug.LogError($"[ScrewStaticBatchRenderer] {gameObject.name}: 要渲染罐子数量({screws.Count})太多, 超过100个, 跳过");
                return;
            }

            _screwDataList = screws.ToArray();

            // 更新一下顶点数据
            UpdateMesh();
        }

        /// <summary>
        /// 移除指定的 Screw
        /// </summary>
        /// <param name="screwId">指定要移除的 Screw</param>
        public void RemoveScrews(int screwId)
        {
            for (int i = 0; i < _screwDataList.Length; i++)
                if (_screwDataList[i].Id == screwId)
                {
                    _screwDataList[i].IsActive = false;
                    UpdateMesh();
                    break;
                }
        }

        /// <summary>
        /// 顶点数据
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct VertexData
        {
            /// <summary>
            /// 顶点坐标(本地坐标)
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// 纹理坐标 
            /// </summary>
            public Vector4 TexCoord;
        }

        /// <summary>
        /// 罐子的大小(世界坐标系)
        /// </summary>
        private const float ScrewSize = 0.25f;

        /// <summary>
        /// 孔的大小(世界坐标系)
        /// </summary>
        private const float HoleSize = 0.115f;

        /// <summary>
        /// 罐子4个顶点的偏移量, 用于在 shader 中计算顶点的真正世界坐标, 见 shader
        /// </summary>
        /// 填充4顶点的标识, 下面的顶点的索引
        ///  0 ---- 1
        ///  |      |
        ///  |      |
        ///  2 ---- 3
        private static readonly Vector2[] ScrewVertexOffset =
        {
            new(-1, 1),
            new(1, 1),
            new(-1, -1),
            new(1, -1),
        };

        /// <summary>
        /// 更新 Mesh 的顶点数据
        /// </summary>
        private void UpdateMesh()
        {
            if (_mesh)
                _mesh.Clear();

            if (_screwDataList == null || _screwDataList.Length == 0)
                return; // 没有要渲染的罐子, 直接返回

            // 每个 Screw 的顶点数量
            const int vertexCountPerScrew = 4;
            // 每个 Screw 的三角形索引数量
            const int indicesCountPerScrew = 6;

            // 计算总的顶点数量
            int numVertices = 0;
            int numIndices = 0;
            for (var i = 0; i < _screwDataList.Length; i++)
            {
                ref var screwData = ref _screwDataList[i];
                
                // 无论罐子显不显示都计算孔的顶点数量, 孔总是需要渲染的
                numVertices += vertexCountPerScrew;
                numIndices  += indicesCountPerScrew;
                
                if (!screwData.IsActive)
                    continue; // 已经被移除的罐子, 跳过罐子的顶点计算

                if (screwData.ColorId >= SpriteNames.Length || screwData.ColorId <= 0)
                {
                    screwData.IsActive = false;
                    Debug.LogError($"[ScrewStaticBatchRenderer] {gameObject.name}: 罐子 ColorId 不正确, Id: {screwData.ColorId}, 跳过");
                    continue;
                }

                var sprite = m_SpriteAtlas.GetSprite(SpriteNames[screwData.ColorId]);
                if (sprite == null)
                {
                    screwData.IsActive = false;
                    Debug.LogError($"[ScrewStaticBatchRenderer] {gameObject.name}: 罐子图片不存在, ColorId: {screwData.ColorId}, 跳过");
                    continue;
                }

                var spriteVertexCount = sprite.GetVertexCount();
                if (spriteVertexCount != vertexCountPerScrew)
                {
                    screwData.IsActive = false;
                    Debug.LogError(
                        $"[ScrewStaticBatchRenderer] {gameObject.name}: 罐子Sprite顶点数量不匹配, 期望 {vertexCountPerScrew}, 实际 {spriteVertexCount}, 无法实现罐子的渲染, Id: {screwData.Id}, ColorId: {screwData.ColorId}");
                    continue;
                }

                numVertices += vertexCountPerScrew;
                numIndices  += indicesCountPerScrew;
            }

            var vertexBuffer = new NativeArray<VertexData>(numVertices, Allocator.Temp);
            var trisBuffer = new NativeArray<ushort>(numIndices, Allocator.Temp);

            var totalVertexIndex = 0;
            var totalIndexIndex = 0;

            // 填充顶点数据
            for (var screwSpriteIndex = 0; screwSpriteIndex < _screwDataList.Length; screwSpriteIndex++)
            {
                ref var screwData = ref _screwDataList[screwSpriteIndex];
                
                // 无论如何都需要添加孔的顶点数据
                AddSpriteVertices(HoleColorId, screwData.LocalPosition, HoleSize);
                
                if (!screwData.IsActive)
                    continue;

                // 添加罐子的顶点数据
                AddSpriteVertices(screwData.ColorId, screwData.LocalPosition, ScrewSize);
            }

            // 顶点格式描述
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
            vertexAttributes[1] = new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4); // x,y 表示 uv 坐标, z,w 表示是罐子4个顶点相对于中心点的标准化偏移量, 具体见 shader
            _mesh.SetVertexBufferParams(numVertices, vertexAttributes);
            vertexAttributes.Dispose();

            // 写入顶点缓冲
            _mesh.SetVertexBufferData(vertexBuffer, 0, 0, numVertices);
            vertexBuffer.Dispose();

            // 写入顶点索引
            _mesh.SetIndexBufferParams(numIndices, IndexFormat.UInt16);
            _mesh.SetIndexBufferData(trisBuffer, 0, 0, numIndices);

            // 设置 SubMesh
            _mesh.subMeshCount = 1;
            var subMeshDescriptor = new SubMeshDescriptor(0, numIndices);
            _mesh.SetSubMesh(0, subMeshDescriptor);

            // 高级方法由于跳过Unity检查，缺失Bounds信息，当任意三角面超过相机的裁剪区域时，整个模型会被裁剪掉（消失不见）
            _mesh.RecalculateBounds();

            return;
            
            // ========== 本地函数
            // 添加指定图片的顶点数据到顶点缓冲
            void AddSpriteVertices(int colorId, Vector3 localPosition, float size)
            {
                //  0 ---- 1
                //  |      |
                //  |      |
                //  2 ---- 3
                // 顶点索引的数据
                trisBuffer[totalIndexIndex++] = (ushort)(totalVertexIndex + 0);
                trisBuffer[totalIndexIndex++] = (ushort)(totalVertexIndex + 1);
                trisBuffer[totalIndexIndex++] = (ushort)(totalVertexIndex + 2);
                trisBuffer[totalIndexIndex++] = (ushort)(totalVertexIndex + 2);
                trisBuffer[totalIndexIndex++] = (ushort)(totalVertexIndex + 1);
                trisBuffer[totalIndexIndex++] = (ushort)(totalVertexIndex + 3);

                var sprite = m_SpriteAtlas.GetSprite(SpriteNames[colorId]);
                var uv = sprite.uv;

                // 计算4个顶点的数据
                for (var vertexIndex = 0; vertexIndex < vertexCountPerScrew; vertexIndex++)
                {
                    vertexBuffer[totalVertexIndex++] = new VertexData()
                                                       {
                                                           Position = localPosition,
                                                           TexCoord = new Vector4(uv[vertexIndex].x,
                                                                                  uv[vertexIndex].y,
                                                                                  ScrewVertexOffset[vertexIndex].x * size,
                                                                                  ScrewVertexOffset[vertexIndex].y * size),
                                                       };
                }
            }
        }
    }
}