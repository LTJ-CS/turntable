using System.Collections.Generic;
using UnityEngine;

namespace GameScript.Runtime.GameLogic.Performance
{
// 挂载脚本后可以自动添加LineRenderer
    [RequireComponent(typeof(LineRenderer))]
    public class MoveTrack : MonoBehaviour
    {
        // LineRenderer组件
        LineRenderer lineRenderer;

        // 使用List储存前几个位置的坐标点
        // 由于绘制过程中需要遍历坐标,所以不能直接使用队列
        // 而是用List进行模拟
        List<Vector3> pointList = new List<Vector3>();

        // 需要记录的坐标点的数量
        [SerializeField] int pointSize = 20;

        void Start()
        {
            // 初始化坐标点列表
            InitPointList();
            // 获取LineRenderer组件
            lineRenderer = GetComponent<LineRenderer>();
            // 初始化LineRenderer的坐标点数量
            lineRenderer.positionCount = pointSize;
        }

        void FixedUpdate()
        {
            // 更新坐标点列表
            UpdatePointList();
            // 根据记录的坐标点绘制尾迹
            DrawLine();
        }

        // 初始化坐标点列表
        void InitPointList()
        {
            // 用起始坐标填满列表进行初始化
            for (int i = 0; i < pointSize; i++)
                pointList.Add(transform.position);
        }

        // 更新坐标点列表 (模拟队列方式)
        void UpdatePointList()
        {
            // 移除最后一个坐标点
            pointList.RemoveAt(pointSize - 1);
            // 添加当前坐标点到表头
            pointList.Insert(0, transform.position);
        }

        // 绘制尾迹
        void DrawLine()
        {
            // 遍历坐标点并添加到LineRenderer中
            for (int i = 0; i < pointSize; i++)
                lineRenderer.SetPosition(i, pointList[i]);
        }
    }
}