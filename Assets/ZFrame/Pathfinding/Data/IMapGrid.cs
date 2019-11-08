using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Pathfinding
{
    public interface IMapGrid
    {
        /// <summary>
        /// 宽度，X轴增长方向
        /// </summary>
        int Width { get; }

        /// <summary>
        /// 长度，Y轴增长方向
        /// </summary>
        int Length { get; }

        bool IsWalkable(int x, int y);

        bool IsDestination(int nx, int ny, int ex, int ey);
    }
}
