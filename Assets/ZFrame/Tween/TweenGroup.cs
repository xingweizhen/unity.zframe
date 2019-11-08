using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

namespace ZFrame.Tween
{
    public class TweenGroup : MonoBehaviour
    {
        [SerializeField] private int m_GroupId = 0;
        [SerializeField] private float m_Lifetime = 1;
        [SerializeField] protected UpdateType m_UpdateType = UpdateType.Normal;
        [SerializeField] protected bool m_IgnoreTimescale = false;

        public float lifetime { get { return m_Lifetime; } }

        public event System.Action<TweenGroup> onComplete;

        public int groupId { get { return m_GroupId; } }

        public bool tweenAutomatically = true;

        [SerializeField, HideInInspector]
        private List<TweenObject> m_Tweens = new List<TweenObject>();

        private float Get() { return 0f; }
        private void Set(float value) { }

        public void AddTweener(TweenObject tweener)
        {
            m_Tweens.Add(tweener);
        }

        public void DoTween(bool forward)
        {
            onComplete = null;

            if (m_Tweens != null) {
                for (var i = 0; i < m_Tweens.Count; ++i) {
                    m_Tweens[i].updateType = m_UpdateType;
                    m_Tweens[i].ignoreTimescale = m_IgnoreTimescale;
                    m_Tweens[i].DoTween(forward);
                }

                if (m_Lifetime > 0) {
                    ZTween.TweenAny(this, Get, Set, 1f, 0f, m_Lifetime)
                        .SetUpdate(m_UpdateType, m_IgnoreTimescale)
                        .SetTag(gameObject)
                        .CompleteWith(StopTween);
                }
            }
        }

        public void StopTween()
        {
            if (m_Tweens != null) {
                for (var i = 0; i < m_Tweens.Count; ++i) {
                    if (m_Tweens[i].tweener != null)
                        m_Tweens[i].tweener.Stop();
                }
            }
        }

        public bool Contains(TweenObject tween)
        {
            return m_Tweens.Contains(tween);
        }

        protected virtual void OnEnable()
        {
            if (tweenAutomatically) DoTween(true);
        }

        protected virtual void OnDisable()
        {
            StopTween();
        }

        private void OnDestroy()
        {
            if (m_Tweens != null) {
                foreach (var tw in m_Tweens) {
#if UNITY_EDITOR
                    if (!Application.isPlaying) {
                        // ... 无法命中。
                        DestroyImmediate(tw);
                    } else
#endif
                    {
                        Destroy(tw);
                    }
                }
            }
        }

        private static void StopTween(ZTweener tw)
        {
            var group = tw.target as TweenGroup;
            if (group) {
                group.StopTween();
                if (group.onComplete != null) {
                    group.onComplete.Invoke(group);
                    group.onComplete = null;
                }
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(TweenGroup), true)]
        [CanEditMultipleObjects]
        internal class SelfEditor : Editor
        {
            internal static Color progressBgInvalid  = new Color(0, 0, 0, 0.6f); 
            internal static Color progressBgValid = new Color(0, 1, 0.2f, 0.5f); 
            internal static Color progressFgInvalid = new Color(0.8f, 0.8f, 1, 0.6f);
            internal static Color progressFgValid = new Color(0.2f, 1f, 0, 1);

            public static readonly Dictionary<System.Type, TweenMenuAttribute> allTypes = new Dictionary<System.Type, TweenMenuAttribute>();
            private GenericMenu _Menu;

            [InitializeOnLoadMethod]
            private static void Init()
            {
                var tweenType = typeof(TweenObject);
                var types = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                    t => t.GetTypes()).Where(t => t.IsSubclassOf(tweenType) && !t.IsAbstract);

                allTypes.Clear();
                foreach (var t in types) {
                    var attrs = t.GetCustomAttributes(typeof(TweenMenuAttribute), false);
                    if (attrs != null && attrs.Length > 0) {
                        allTypes.Add(t, attrs[0] as TweenMenuAttribute);
                    } else {
                        allTypes.Add(t, null);
                    }
                }
            }

            private List<Editor> editors = new List<Editor>();

            private void AddTweenMenu(object udata)
            {
                var self = target as TweenGroup;
                var type = udata as System.Type;

                var tw = Undo.AddComponent(self.gameObject, type) as TweenObject;
                Undo.RecordObject(tw, "AddComponent");
                tw.tweenAutomatically = false;
                tw.duration = self.m_Lifetime;
                tw.ResetStatus();

                Undo.RecordObject(target, "AddComponent");
                self.AddTweener(tw);

                tw.hideFlags |= HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }

            private void ShowAddMenu(Rect rect)
            {
                if (_Menu == null) {
                    _Menu = new GenericMenu();
                    foreach (var kv in allTypes) {
                        var type = kv.Key;
                        var menu = kv.Value != null ? kv.Value.menu : type.Name;
                        _Menu.AddItem(new GUIContent(menu), false, AddTweenMenu, type);
                    }
                }
                _Menu.DropDown(rect);
            }

            protected virtual void OnEnable()
            {
                _Menu = null;

                var self = target as TweenGroup;
                foreach (var tw in self.m_Tweens) {
                    if (tw) tw.hideFlags |= HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                }
            }

            protected virtual void PropertiesGUI()
            {
                DrawDefaultInspector();
            }

            public override void OnInspectorGUI()
            {
                PropertiesGUI();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                var self = target as TweenGroup;
                for (int i = 0; i < self.m_Tweens.Count;) {
                    var tw = self.m_Tweens[i];
                    if (tw) {
                        if (editors.Count <= i) editors.Add(null);

                        if (editors[i] == null || editors[i].target != tw) {
                            DestroyImmediate(editors[i]);
                            editors[i] = CreateEditor(tw);
                        }

                        editors[i].OnInspectorGUI();

                        i++;
                    } else {
                        self.m_Tweens.RemoveAt(i);
                    }
                }

                for (int i = editors.Count - 1; i >= self.m_Tweens.Count; --i) {
                    DestroyImmediate(editors[i]);
                    editors.RemoveAt(i);
                }

                GUILayout.Space(4);
                var buttonRect = EditorGUILayout.GetControlRect();
                if (GUI.Button(buttonRect, "Add Tween...", EditorStyles.miniButton)) {
                    ShowAddMenu(buttonRect);
                }
                GUILayout.Space(4);

                EditorGUILayout.EndVertical();
            }

            private void OnDestroy()
            {
                foreach (var editor in editors) {
                    DestroyImmediate(editor);
                }
                editors.Clear();
            }
        }
#endif

    }
}
