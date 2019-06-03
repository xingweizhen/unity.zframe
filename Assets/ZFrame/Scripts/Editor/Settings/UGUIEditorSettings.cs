using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Settings
{
    public class UGUIEditorSettings : ScriptableObject
    {
        [SerializeField, AssetPath("UI预设根目录", typeof(UnityEditor.DefaultAsset))]
        private string m_UItFolder;
        public string uiFolder { get { return m_UItFolder; } }

        [SerializeField, AssetRef("标准图片", typeof(Sprite))]
        private string m_StandardSpritePath;

        [SerializeField, AssetRef("背景图片", typeof(Sprite))]
        private string m_BackgroundSpritePath;

        public string kStandardSpritePath { get { return m_StandardSpritePath; } }

        public string kBackgroundSpritePath { get { return m_BackgroundSpritePath; } }

        public static UGUIEditorSettings Get()
        {
            var guids = AssetDatabase.FindAssets("t:UGUIEditorSettings");
            if (guids != null && guids.Length > 0) {
                return AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0])) as UGUIEditorSettings;
            }
            return null;
        }
    }
}
