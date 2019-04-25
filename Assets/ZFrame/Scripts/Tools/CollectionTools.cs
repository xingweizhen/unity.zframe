using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CollectionTools
{
    public static void RemoveNull<T>(this List<T> list)
    {
        // 找出第一个空元素 O(n)
        int count = list.Count;
        for (int i = 0; i < count; i++) {
            if (list[i] == null || list[i].Equals(null)) {
                // 记录当前位置
                int newCount = i++;

                // 对每个非空元素，复制至当前位置 O(n)
                for (; i < count; i++)
                    if (list[i] != null && !list[i].Equals(null))
                        list[newCount++] = list[i];

                // 移除多余的元素 O(n)
                list.RemoveRange(newCount, count - newCount);
                break;
            }
        }
    }

    public static void AddOrReplace(this IDictionary self, object key, object value)
    {
        if (self.Contains(key)) {
            self[key] = value;
        } else {
            self.Add(key, value);
        }
    }

    public static void AddNotExist(this IDictionary self, object key, object value)
    {
        if (!self.Contains(key)) {
            self.Add(key, value);
        }
    }
}
