using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZFrame.Tween
{
    /// <summary>
    /// 数值属性缓动组件
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    public abstract class TweenFloat<T> : TweenComponent<T, float> where T : Object
    {
#if UNITY_EDITOR
        internal abstract class TweenValueEditor : TweenEditor
        {
            protected override void OnValueGUI(SerializedProperty property, Rect rect, string content)
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 14;
                property.floatValue = EditorGUI.FloatField(rect, content, property.floatValue);
                EditorGUIUtility.labelWidth = labelWidth;
            }
        }
#endif
    }
}
