using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZFrame.Tween
{
    public abstract class TweenObject : MonoBehaviour
    {        
        public float delay = 0;
        public float duration = 1;
        public Ease ease = Ease.Linear;
        public int loops = 1;
        public LoopType loopType = LoopType.Restart;
        public UpdateType updateType = UpdateType.Normal;
        public bool ignoreTimescale = false;

        protected ZTweener m_Tweener;
        public ZTweener tweener { get { return m_Tweener; } }

        public float lifetime {
            get {
                if (loops < 0) return -1f;
                return duration * (loops == 0 ? 1 : loops) + delay;
            }
        }

        public abstract bool DoTween(bool reset, bool forward);

        public bool tweenAutomatically = true;

        /// <summary>
        /// 重置数值为当前值
        /// </summary>
        public abstract void ResetStatus();

#if UNITY_EDITOR
        protected bool _foldout = true;

        internal abstract class Editor : UnityEditor.Editor
        {
            public abstract string TweenName { get; }

            private SerializedProperty m_Duration, m_UpdateType, m_Ease, m_Delay, m_Loops, m_IgnoreTimescale, m_LoopType;

            protected virtual void OnEnable()
            {
                m_Delay = serializedObject.FindProperty("delay");
                m_Duration = serializedObject.FindProperty("duration");                
                m_Ease = serializedObject.FindProperty("ease");
                m_Loops = serializedObject.FindProperty("loops");
                m_LoopType = serializedObject.FindProperty("loopType");
                m_UpdateType = serializedObject.FindProperty("updateType");
                m_IgnoreTimescale = serializedObject.FindProperty("ignoreTimescale");                
            }

            protected virtual void OnContentGUI()
            {
                EditorGUILayout.PropertyField(m_Delay);
                EditorGUILayout.PropertyField(m_Duration);
                EditorGUILayout.PropertyField(m_Ease);
                EditorGUILayout.PropertyField(m_Loops);
                EditorGUILayout.PropertyField(m_LoopType);
            }

            public override void OnInspectorGUI()
            {
                var self = target as TweenObject;
                if ((self.hideFlags & HideFlags.HideInInspector) == 0) {
                    OnContentGUI();
                    EditorGUILayout.PropertyField(m_UpdateType);
                    EditorGUILayout.PropertyField(m_IgnoreTimescale);

                    serializedObject.ApplyModifiedProperties();
                } else {
                    var rect = EditorGUILayout.GetControlRect();
                    var rt = rect;

                    rt.width = rect.height;
                    self._foldout = GUI.Toggle(rt, self._foldout, GUIContent.none, EditorStyles.foldout);

                    rt.x = rt.xMax;
                    self.enabled = EditorGUI.ToggleLeft(rt, GUIContent.none, self.enabled);

                    rt.x = rt.xMax;
                    rt.xMax = rect.xMax - 20;
                    EditorGUI.LabelField(rt, TweenName, EditorStyles.boldLabel);

                    rt.x = rt.xMax;
                    rt.width = 20;
                    if (GUI.Button(rt, "X", EditorStyles.miniButton)) {
                        Undo.RecordObject(this, "Remove Tweener");
                        Undo.DestroyObjectImmediate(self);
                        return;
                    }

                    if (self._foldout) {
                        OnContentGUI();
                        serializedObject.ApplyModifiedProperties();
                    }

                    EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);
                }
            }
        }
#endif
    }
}
