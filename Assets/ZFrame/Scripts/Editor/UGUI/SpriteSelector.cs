using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;

namespace ZFrame.Editors
{
    using UGUI;

    public class SpriteSelector : EditorWindow
    {
        private SpriteAtlas m_Atlas;
        public string partialSprite { get; private set; }
        public string selectedSprite { get; private set; }
        private List<Sprite> m_Sprites;

        public delegate void Callback(string sprite);

        Vector2 m_Scroll = Vector2.zero;
        Callback m_Callback;
        float m_ClickTime = 0f;

        /// <summary>
        /// Draw the custom wizard.
        /// </summary>

        void OnGUI()
        {
            EditorGUIUtility.labelWidth = 80;

            if (m_Atlas == null) {
                GUILayout.Label("No Atlas selected.", "LODLevelNotifyText");
            } else {
                bool close = false;
                GUILayout.Label(m_Atlas.name + " Sprites", "LODLevelNotifyText");
                EditorGUILayout.Separator();

                GUILayout.BeginHorizontal();
                GUILayout.Space(84f);

                string before = partialSprite;
                string after = EditorGUILayout.TextField("", before, "SearchTextField");
                if (before != after) partialSprite = after;

                if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f))) {
                    partialSprite = "";
                    GUIUtility.keyboardControl = 0;
                }

                GUILayout.Space(84f);
                GUILayout.EndHorizontal();

                if (m_Sprites == null) {
                    m_Sprites = new List<Sprite>();
                    var spritePaths = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(m_Atlas));
                    foreach (var path in spritePaths) {
                        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (sp) m_Sprites.Add(sp);
                    }
                }

                float size = 80f;
                float padded = size + 10f;

                int columns = Mathf.FloorToInt(position.width / padded);
                if (columns < 1) columns = 1;

                int offset = 0;
                Rect rect = new Rect(10f, 0, size, size);

                GUILayout.Space(10f);
                m_Scroll = GUILayout.BeginScrollView(m_Scroll);
                int rows = 1;

                while (offset < m_Sprites.Count) {
                    GUILayout.BeginHorizontal();
                    {
                        int col = 0;
                        rect.x = 10f;

                        for (; offset < m_Sprites.Count; ++offset) {
                            var sprite = m_Sprites[offset];
                            if (sprite == null) continue;

                            var spriteName = sprite.name;

                            if (!string.IsNullOrEmpty(partialSprite) && !spriteName.Contains(partialSprite)) continue;

                            // Button comes first
                            if (GUI.Button(rect, "", GUIStyle.none)) {
                                if (Event.current.button == 0) {
                                    float delta = Time.realtimeSinceStartup - m_ClickTime;
                                    m_ClickTime = Time.realtimeSinceStartup;

                                    if (selectedSprite != spriteName) {
                                        selectedSprite = spriteName;
                                        if (m_Callback != null) m_Callback(spriteName);
                                    } else if (delta < 0.5f) close = true;
                                }
                            }

                            if (Event.current.type == EventType.Repaint) {
                                UISpriteEditor.DrawSprite(sprite, rect, Color.white, GUIStyle.none);

                                // Draw the selection
                                if (selectedSprite == spriteName) {
                                    DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));
                                }
                            }

                            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
                            GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
                            GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), spriteName,
                                "ProgressBarBack");
                            GUI.contentColor = Color.white;
                            GUI.backgroundColor = Color.white;

                            if (++col >= columns) {
                                ++offset;
                                break;
                            }

                            rect.x += padded;
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(padded);
                    rect.y += padded + 26;
                    ++rows;
                }

                GUILayout.Space(rows * 26);
                GUILayout.EndScrollView();

                if (close) Close();
            }
        }

        public static void DrawOutline(Rect rect, Color color)
        {
            if (Event.current.type == EventType.Repaint) {
                var outlineWidth = 2f;
                Texture2D tex = EditorGUIUtility.whiteTexture;
                GUI.color = color;
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, outlineWidth, rect.height), tex);
                GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, outlineWidth, rect.height), tex);
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, outlineWidth), tex);
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, outlineWidth), tex);
                GUI.color = Color.white;
            }
        }

        public static void Show(SpriteAtlas spriteAtlas, string selectedSprite, Callback callback)
        {
#if UNITY_2018_2
            UnityEditor.U2D.SpriteAtlasUtility.PackAtlases(new[] { spriteAtlas }, EditorUserBuildSettings.activeBuildTarget);
#endif
            var comp = GetWindow<SpriteSelector>("Select a Sprite");
            comp.selectedSprite = selectedSprite;
            comp.m_Callback = callback;
            comp.m_Atlas = spriteAtlas;
            comp.m_Sprites = null;
        }
    }
}
