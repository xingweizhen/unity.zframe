using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZFrame.Tween
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TweenMenuAttribute : System.Attribute
    {
        public readonly string menu;
        public readonly string name;

        public TweenMenuAttribute(string menu, string name)
        {
            this.menu = menu;
            this.name = name;
        }
    }

    public abstract class TweenObject : MonoBehaviour
    {
        private static List<TweenGroup> _Groups = new List<TweenGroup>();

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
                if (loops < 0) return float.MaxValue;
                return duration * (loops == 0 ? 1 : loops) + delay;
            }
        }
        
        public bool tweenAutomatically = true;

        public abstract bool DoTween(bool reset, bool forward);
        
        /// <summary>
        /// 重置数值为当前值
        /// </summary>
        public abstract void ResetStatus();

        public TweenGroup FindGroup()
        {
            TweenGroup group = null;
            _Groups.Clear();
            GetComponents(_Groups);
            foreach (var grp in _Groups) {
                if (grp.Contains(this)) {
                    group = grp;
                    break;
                }
            }
            _Groups.Clear();
            return group;
        }

#if UNITY_EDITOR
        protected bool _foldout = true;

        internal abstract class Editor : UnityEditor.Editor
        {
            private SerializedProperty m_Duration, m_UpdateType, m_Ease, m_Delay, m_Loops, m_IgnoreTimescale, m_LoopType;
            private SerializedProperty tweenAutomatically;

            protected virtual void OnEnable()
            {
                m_Delay = serializedObject.FindProperty("delay");
                m_Duration = serializedObject.FindProperty("duration");
                m_Ease = serializedObject.FindProperty("ease");
                m_Loops = serializedObject.FindProperty("loops");
                m_LoopType = serializedObject.FindProperty("loopType");
                m_UpdateType = serializedObject.FindProperty("updateType");
                m_IgnoreTimescale = serializedObject.FindProperty("ignoreTimescale");
                tweenAutomatically = serializedObject.FindProperty("tweenAutomatically");
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
                    EditorGUILayout.PropertyField(tweenAutomatically);

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

                    TweenMenuAttribute attr = null;
                    System.Type selfType = self.GetType();
                    TweenGroup.Editor.allTypes.TryGetValue(selfType, out attr);
                    EditorGUI.LabelField(rt, attr != null ? attr.name : selfType.Name, EditorStyles.boldLabel);

                    rt.x = rt.xMax;
                    rt.width = 20;
                    if (GUI.Button(rt, "X", EditorStyles.miniButton)) {
                        Undo.RecordObject(this, "Remove Tweener");
                        Undo.DestroyObjectImmediate(self);
                        return;
                    }

                    rect = EditorGUILayout.GetControlRect(false, 3);                    
                    rect.xMin += EditorGUIUtility.singleLineHeight * 2;
                    rect.xMax -= EditorGUIUtility.singleLineHeight * 2;
                    var rtBar = rect;
                    EditorGUI.DrawRect(rtBar, TweenGroup.Editor.progressBgInvalid);

                    float delay = self.delay, duration = self.lifetime - self.delay, maxDura = self.duration;
                    var group = self.FindGroup();
                    if (group) {
                        maxDura = group.lifetime;
                        if (delay > maxDura) {
                            delay = maxDura;
                            duration = 0;
                        } else {
                            duration = Mathf.Min(duration, maxDura - self.delay);
                        }
                    }
                    if (maxDura > 0) {
                        //rtBar.width = rect.width * (self.delay / maxDura);
                        //EditorGUI.DrawRect(rtBar, TweenGroup.Editor.progressFgInvalid);

                        rtBar.xMin += rect.width * (delay / maxDura);
                        rtBar.width = rect.width * (duration / maxDura);
                        EditorGUI.DrawRect(rtBar, TweenGroup.Editor.progressBgValid);
                    }

                    GUILayout.Space(2);

                    if (self._foldout) {
                        GUILayout.Space(2);
                        OnContentGUI();
                        serializedObject.ApplyModifiedProperties();
                    }

                    EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);
                }
            }
        }
        
        [CustomPropertyDrawer(typeof(Ease))]
        internal class EaseDrawer : PropertyDrawer
        {
            private List<Vector3> m_SimplePoints = new List<Vector3>(64);
            private readonly Vector3[] _SegmentPoints = new Vector3[2];

            private void DrawEaseCurve(Rect rect, int easeValue, int segments)
            {
                if (m_SimplePoints.Count == 0) {
                    var simple = ZTweener.EaseAlgorithm[easeValue];
                    var min = float.MaxValue;
                    var max = float.MinValue;
                    for (var i = 0; i < segments; ++i) {
                        var value = simple.Invoke(0, 1, i / (float)segments);
                        if (value < min) min = value;
                        if (value > max) max = value;
                        m_SimplePoints.Add(new Vector3(i, value));
                    }
                    var range = max - min;
                    for (var i = 0; i < m_SimplePoints.Count; ++i) {
                        var pos = m_SimplePoints[i];
                        pos.y = (pos.y - min) / range;
                        m_SimplePoints[i] = pos;
                    }
                }

                if (m_SimplePoints.Count > 0) {
                    EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
                    rect.x += 1;
                    rect.y += 1;
                    rect.width -= 2;
                    rect.height -= 2;

                    var origin = new Vector3(rect.x, rect.yMax, 0f);
                    var scale = new Vector3(1, -rect.height);
                    var defColor = Handles.color;
                    Handles.color = Color.white;
                    var lastPos = origin + Vector3.Scale(m_SimplePoints[0], scale);

                    var lastIdx = 0;
                    for (var i = 1; i < rect.width; ++i) {
                        var index = (int)(i * m_SimplePoints.Count / rect.width);
                        Vector3 curr;
                        if (index != lastIdx) {
                            curr = origin + Vector3.Scale(m_SimplePoints[index], scale);
                            lastIdx = index;
                        } else {
                            curr = lastPos;
                        }
                        curr.x = origin.x + i;
                        _SegmentPoints[0] = lastPos;
                        _SegmentPoints[1] = curr;
                        Handles.DrawAAPolyLine(_SegmentPoints);

                        lastPos = curr;
                    }
                    Handles.color = defColor;
                }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUIUtility.singleLineHeight * 3 + 2;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                position = EditorGUI.PrefixLabel(position, label);
                DrawEaseCurve(position, property.intValue, Mathf.Min((int)position.width, 256));

                var rect = position;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                var ease = (Ease)EditorGUI.EnumPopup(rect, (Ease)property.intValue, EditorStyles.whiteMiniLabel);
                if (EditorGUI.EndChangeCheck()) {
                    m_SimplePoints.Clear();
                    property.intValue = (int)ease;
                }
            }
        }
#endif
    }
}
