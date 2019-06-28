using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZFrame.Tween
{
    /// <summary>
    /// 颜色属性缓动组件
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    public abstract class TweenColor<T> : TweenComponent<T, Color> where T : Object
    {  
#if UNITY_EDITOR        
        internal abstract class TweenValueEditor : TweenEditor
        {
            protected override void OnValueGUI(SerializedProperty property, Rect rect, string content)
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 14;
                property.colorValue = EditorGUI.ColorField(rect, content, property.colorValue);
                EditorGUIUtility.labelWidth = labelWidth;
            }
        }
#endif
    }
}
