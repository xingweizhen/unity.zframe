using UnityEngine;
using System.Collections;

public static class GizmosTools
{

    public static void DrawCircle(Vector3 position, Quaternion rotation, float radius, Color color, bool dotted = false)
    {
        // 设置矩阵
        Matrix4x4 defaultMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

        // 设置颜色
        Color defaultColor = Gizmos.color;
        Gizmos.color = color;

        // 绘制圆环
        Vector3 beginPoint = Vector3.zero;
        Vector3 firstPoint = Vector3.zero;
        var i = 0;
        float step = Mathf.PI / 36f;
        for (float rad = 0; rad < 2 * Mathf.PI; rad += step, i++) {
            float x = radius * Mathf.Cos(rad);
            float z = radius * Mathf.Sin(rad);
            Vector3 endPoint = new Vector3(x, 0, z);
            if (!dotted || i % 2 == 0) {
                if (rad > 0) {
                    Gizmos.DrawLine(beginPoint, endPoint);
                } else {
                    firstPoint = endPoint;
                }
            }
            beginPoint = endPoint;
        }

        // 绘制最后一条线段
        Gizmos.DrawLine(firstPoint, beginPoint);

        // 恢复默认颜色
        Gizmos.color = defaultColor;

        // 恢复默认矩阵
        Gizmos.matrix = defaultMatrix;
    }

    public static void DrawRect(Vector3 position, Quaternion rotation, float width, float length, Color color)
    {
        // 设置矩阵
        Matrix4x4 defaultMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

        // 设置颜色
        Color defaultColor = Gizmos.color;
        Gizmos.color = color;
        var ext1 = new Vector3(width / 2, 0, length / 2);
        var ext2 = new Vector3(width / 2, 0, -length / 2);
        Gizmos.DrawLine(-ext1, -ext2);
        Gizmos.DrawLine(-ext2, ext1);
        Gizmos.DrawLine(ext1, ext2);
        Gizmos.DrawLine(ext2, -ext1);

        // 恢复默认颜色
        Gizmos.color = defaultColor;

        // 恢复默认矩阵
        Gizmos.matrix = defaultMatrix;
    }
}
