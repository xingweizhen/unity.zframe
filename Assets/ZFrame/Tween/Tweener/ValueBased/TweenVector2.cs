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
    public abstract class TweenVector2<T> : TweenComponent<T, Vector2> where T : Object
    {
        public override void ResetStatus()
        {
            m_From = GetCurrentValue();
            m_To = m_From;
        }

#if UNITY_EDITOR
        internal abstract class TweenValueEditor : TweenEditor
        {
            protected override float m_ValueHeight {
                get {
                    return EditorGUIUtility.singleLineHeight * 2 + 2;
                }
            }

            protected override void OnValueNameGUI(Rect rect)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, "X");

                rect.y = rect.yMax + 2;
                EditorGUI.LabelField(rect, "Y");
            }

            protected override void OnValueGUI(SerializedProperty property, Rect rect, string content)
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 14;
                
                EditorGUI.BeginChangeCheck();
                rect.height = EditorGUIUtility.singleLineHeight;
                var v2 = property.vector2Value;
                v2.x = EditorGUI.FloatField(rect, content, v2.x);

                rect.y = rect.yMax;
                v2.y = EditorGUI.FloatField(rect, content, v2.y);
                if (EditorGUI.EndChangeCheck()) {
                    property.vector2Value = v2;
                }

                EditorGUIUtility.labelWidth = labelWidth;
            }
        }
#endif
    }
}