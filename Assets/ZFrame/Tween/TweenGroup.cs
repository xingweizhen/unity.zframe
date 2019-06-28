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
        [SerializeField] private float m_Duration = 1;
        [SerializeField] protected UpdateType m_UpdateType = UpdateType.Normal;
        [SerializeField] protected bool m_IgnoreTimescale = false;

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
            if (m_Tweens != null) {
                for (var i = 0; i < m_Tweens.Count; ++i) {
                    m_Tweens[i].updateType = m_UpdateType;
                    m_Tweens[i].ignoreTimescale = m_IgnoreTimescale;
                    m_Tweens[i].DoTween(true, forward);
                }
                ZTween.Tween(this, Get, Set, 0, m_Duration)
                    .SetUpdate(m_UpdateType, m_IgnoreTimescale)
                    .CompleteWith(StopTween);
            }
        }

        public void StopTween()
        {
            if (m_Tweens != null) {
                for (var i = 0; i < m_Tweens.Count; ++i) {
                    ZTween.Stop(m_Tweens[i]);
                }
            }
        }

        private void OnEnable()
        {
            if (tweenAutomatically) DoTween(true);
        }

        private void OnDisable()
        {
            StopTween();
        }

        private static void StopTween(ZTweener tw)
        {
            var group = tw.target as TweenGroup;
            if (group) group.StopTween();
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(TweenGroup))]
        internal class Editor : UnityEditor.Editor
        {
            private static readonly List<System.Type> _AllTypes = new List<System.Type>();
            private static GenericMenu _Menu;

            [InitializeOnLoadMethod]
            private static void Init()
            {
                var tweenType = typeof(TweenObject);
                var types =
                    System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                            t => t.GetTypes()).Where(t => t.IsSubclassOf(tweenType) && !t.IsAbstract);

                _AllTypes.Clear();
                _AllTypes.AddRange(types);


            }

            private List<UnityEditor.Editor> editors = new List<UnityEditor.Editor>();

            private void OnEnable()
            {
                var self = target as TweenGroup;
                _Menu = new GenericMenu();
                foreach (var t in _AllTypes) {
                    _Menu.AddItem(new GUIContent(t.Name), false, () => {
                        var tw = Undo.AddComponent(self.gameObject, t) as TweenObject;
                        Undo.RecordObject(tw, "AddComponent");
                        tw.tweenAutomatically = false;
                        tw.ResetStatus();

                        Undo.RecordObject(target, "AddComponent");
                        self.AddTweener(tw);

                        tw.hideFlags |= HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                    });
                }
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

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
                    _Menu.DropDown(buttonRect);
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
