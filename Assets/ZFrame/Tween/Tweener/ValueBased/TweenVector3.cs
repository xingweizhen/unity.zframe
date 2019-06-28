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
    public abstract class TweenVector3<T> : TweenComponent<T, Vector3> where T : Object
    {
#if UNITY_EDITOR
        internal abstract class TweenValueEditor : TweenEditor
        {
            protected override float m_ValueHeight {
                get {
                    return EditorGUIUtility.singleLineHeight * 3 + 4;
                }
            }

            protected override void OnValueNameGUI(Rect rect)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, "X");

                rect.y = rect.yMax + 2;
                EditorGUI.LabelField(rect, "Y");

                rect.y = rect.yMax + 2;
                EditorGUI.LabelField(rect, "Z");
            }

            protected override void OnValueGUI(SerializedProperty property, Rect rect, string content)
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 14;

                EditorGUI.BeginChangeCheck();
                rect.height = EditorGUIUtility.singleLineHeight;
                var v3 = property.vector3Value;
                v3.x = EditorGUI.FloatField(rect, content, v3.x);

                rect.y = rect.yMax + 2;
                v3.y = EditorGUI.FloatField(rect, content, v3.y);

                rect.y = rect.yMax + 2;
                v3.z = EditorGUI.FloatField(rect, content, v3.z);

                if (EditorGUI.EndChangeCheck()) {
                    property.vector2Value = v3;
                }
                EditorGUIUtility.labelWidth = labelWidth;
            }
        }
#endif
    }
}