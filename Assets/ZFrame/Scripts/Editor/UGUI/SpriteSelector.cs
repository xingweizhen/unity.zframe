//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;

/// <summary>
/// Editor component used to display a list of sprites.
/// </summary>

namespace ZFrame.UGUI
{
    public class SpriteSelector : ScriptableWizard
    {
        public static UIAtlas atlas;
        private static SpriteAtlas spriteAtlas;
        public static string partialSprite;
        public static string selectedSprite;
        private static List<Sprite> sprites;

        static public SpriteSelector instance;

        void OnEnable()
        {
            instance = this;
        }

        void OnDisable()
        {
            instance = null;
        }

        public delegate void Callback(string sprite);

        SerializedObject mObject;

        UISprite mSprite;
        Vector2 mPos = Vector2.zero;
        Callback mCallback;
        float mClickTime = 0f;

        /// <summary>
        /// Draw the custom wizard.
        /// </summary>

        void OnGUI()
        {
            EditorGUIUtility.labelWidth = 80;

            if (spriteAtlas == null) {
                if (atlas != null) spriteAtlas = atlas.atlas;
            }

            if (spriteAtlas == null) {
                GUILayout.Label("No Atlas selected.", "LODLevelNotifyText");
            } else {
                bool close = false;
                GUILayout.Label(spriteAtlas.name + " Sprites", "LODLevelNotifyText");
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

                if (sprites == null) {
                    sprites = new List<Sprite>();
                    var spritePaths = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(spriteAtlas));
                    foreach (var path in spritePaths) {
                        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (sp) sprites.Add(sp);
                    }
                }

                float size = 80f;
                float padded = size + 10f;

                int columns = Mathf.FloorToInt(position.width / padded);
                if (columns < 1) columns = 1;

                int offset = 0;
                Rect rect = new Rect(10f, 0, size, size);

                GUILayout.Space(10f);
                mPos = GUILayout.BeginScrollView(mPos);
                int rows = 1;

                while (offset < sprites.Count) {
                    GUILayout.BeginHorizontal();
                    {
                        int col = 0;
                        rect.x = 10f;

                        for (; offset < sprites.Count; ++offset) {
                            var sprite = sprites[offset];
                            if (sprite == null) continue;

                            var spriteName = sprite.name;

                            if (!string.IsNullOrEmpty(partialSprite) && !spriteName.Contains(partialSprite)) continue;

                            // Button comes first
                            if (GUI.Button(rect, "", GUIStyle.none)) {
                                if (Event.current.button == 0) {
                                    float delta = Time.realtimeSinceStartup - mClickTime;
                                    mClickTime = Time.realtimeSinceStartup;

                                    if (selectedSprite != spriteName) {
                                        if (mSprite != null) {
                                            Undo.RecordObject(mSprite, "Atlas Selection");
                                            EditorUtility.SetDirty(mSprite.gameObject);
                                        }

                                        SpriteSelector.selectedSprite = spriteName;
                                        if (mCallback != null) mCallback(spriteName);
                                    } else if (delta < 0.5f) close = true;
                                } else {
                                    //NGUIContextMenu.AddItem("Edit", false, EditSprite, sprite);
                                    //NGUIContextMenu.AddItem("Delete", false, DeleteSprite, sprite);
                                    //NGUIContextMenu.Show();
                                }
                            }

                            if (Event.current.type == EventType.Repaint) {
                                UISpriteEditor.DrawSprite(sprite, rect, Color.white, GUIStyle.none);

                                // Draw the selection
                                if (SpriteSelector.selectedSprite == spriteName) {
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

        /// <summary>
        /// Show the selection wizard.
        /// </summary>

        static public void Show(Callback callback)
        {
            if (instance != null) {
                instance.Close();
                instance = null;
            }

            SpriteSelector comp = ScriptableWizard.DisplayWizard<SpriteSelector>("Select a Sprite");
            comp.mSprite = null;
            comp.mCallback = callback;
        }

        public static void Show(SpriteAtlas spriteAtlas, string selectedSprite, Callback callback)
        {
            SpriteSelector.spriteAtlas = spriteAtlas;
            SpriteSelector.selectedSprite = selectedSprite;
            sprites = null;
            Show(callback);
        }
    }
}
