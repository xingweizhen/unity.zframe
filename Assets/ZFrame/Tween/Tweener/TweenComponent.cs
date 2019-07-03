using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZFrame.Tween
{
    /// <summary>
    /// 缓动动画启动组件
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <typeparam name="V">值类型</typeparam>
    public abstract class TweenComponent<T, V> : TweenObject where T : Object
    {
        [SerializeField] private T m_Target;
        public virtual T target { get { if (!m_Target) m_Target = gameObject.GetComponent<T>(); return m_Target; } }

        [SerializeField] protected V m_From, m_To;

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (tweenAutomatically) DoTween(true, true);
        }

        protected abstract ZTweener StartTween(bool reset, bool forward);

        /// <summary>
        /// 启动缓动动画
        /// </summary>
        /// <param name="reset">是否从初始位置开始</param>
        /// <param name="forward">是否正向播放</param>
        /// <returns></returns>
        public sealed override bool DoTween(bool reset, bool forward)
        {
            if (enabled) {
                m_Tweener = StartTween(reset, forward);
                if (m_Tweener != null) {
                    m_Tweener.DelayFor(delay).EaseBy(ease).LoopFor(loops, loopType)
                        .SetUpdate(updateType, ignoreTimescale).SetTag(gameObject);
                    return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        internal abstract class TweenEditor : Editor
        {
            protected virtual float m_ValueHeight { get { return EditorGUIUtility.singleLineHeight; } }

            private SerializedProperty m_Target;
            private SerializedProperty m_From, m_To;

            protected override void OnEnable()
            {
                base.OnEnable();
                m_Target = serializedObject.FindProperty("m_Target");
                m_From = serializedObject.FindProperty("m_From");
                m_To = serializedObject.FindProperty("m_To");
            }

            protected virtual void OnValueNameGUI(Rect rect)
            {
                EditorGUI.LabelField(rect, typeof(V).Name);
            }

            protected abstract void OnValueGUI(SerializedProperty property, Rect rect, string content);

            protected virtual void OnPropertiesGUI()
            {

            }

            protected void OnPairValueGUI(SerializedProperty prop1, string name1, SerializedProperty prop2, string name2)
            {
                var rect = EditorGUILayout.GetControlRect(false, m_ValueHeight);
                float labelWidth = EditorGUIUtility.labelWidth;

                var fromRect = new Rect(rect.x + labelWidth, rect.y, (rect.width - labelWidth) / 2 - 2, rect.height);
                var toRect = new Rect(rect.xMax - fromRect.width, fromRect.y, fromRect.width, fromRect.height);
                rect.width = labelWidth - 8;

                EditorGUI.indentLevel++;
                OnValueNameGUI(rect);
                EditorGUI.indentLevel--;
                OnValueGUI(prop1, fromRect, name1);
                OnValueGUI(prop2, toRect, name2);
            }

            protected sealed override void OnContentGUI()
            {
                EditorGUILayout.PropertyField(m_Target);
                EditorGUI.indentLevel++;
                OnPropertiesGUI();
                EditorGUI.indentLevel--;
                OnPairValueGUI(m_From, "F", m_To, "T");                
                EditorGUILayout.Separator();
                base.OnContentGUI();                
            }
        }
#endif
    }
}
