using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ZFrame.Editors
{
    public static class CustomEditorStyles
    {
        private static GUIStyle m_titleStyle;

        public static GUIStyle titleStyle {
            get {
                if (m_titleStyle == null) {
                    m_titleStyle = new GUIStyle(EditorStyles.helpBox) {
                        fontSize = 12,
                        richText = true
                    };
                }

                return m_titleStyle;
            }
        }

        private static GUIStyle m_richText;

        public static GUIStyle richText {
            get {
                if (m_richText == null) {
                    m_richText = new GUIStyle(EditorStyles.label) {richText = true};
                }

                return m_richText;
            }
        }

        private static GUIStyle m_richTextBtn;

        public static GUIStyle richTextBtn {
            get {
                if (m_richTextBtn == null) {
                    m_richTextBtn = new GUIStyle(EditorStyles.miniButton) {
                        richText = true,
                        fontSize = 12
                    };
                }

                return m_richTextBtn;
            }
        }

        private static GUIStyle m_ToggleTitle;

        public static GUIStyle toggleTitle {
            get {
                if (m_ToggleTitle == null) {
                    m_ToggleTitle = new GUIStyle(EditorStyles.toggle) {fontSize = 12};
                }

                return m_ToggleTitle;
            }
        }

        private static GUIStyle m_MidLabel;

        public static GUIStyle midLabel {
            get {
                if (m_MidLabel == null) {
                    m_MidLabel = new GUIStyle(richText) {alignment = TextAnchor.MiddleCenter};
                }

                return m_MidLabel;
            }
        }

        private static GUIStyle m_RightLabel;

        public static GUIStyle rightLabel {
            get {
                if (m_MidLabel == null) {
                    m_MidLabel = new GUIStyle(richText) {alignment = TextAnchor.MiddleRight};
                }

                return m_MidLabel;
            }
        }

        private static GUIStyle m_ItalicLabel;

        public static GUIStyle ItalicLabel {
            get {
                if (m_ItalicLabel == null) {
                    m_ItalicLabel = new GUIStyle(richText) {fontStyle = FontStyle.Italic};
                }

                return m_ItalicLabel;
            }
        }

        private static GUIStyle m_LeftToolbar;

        public static GUIStyle LeftToolbar {
            get {
                if (m_LeftToolbar == null) {
                    m_LeftToolbar = new GUIStyle(EditorStyles.toolbarButton) {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 11,
                    };
                }

                return m_LeftToolbar;
            }
        }

        private static GUIStyle m_BoldText;

        public static GUIStyle boldText {
            get {
                if (m_BoldText == null) {
                    m_BoldText = new GUIStyle(EditorStyles.textField) {
                        fontStyle = FontStyle.Bold,
                    };
                }

                return m_BoldText;
            }
        }

        private static GUIStyle __NormalButton;
        public static GUIStyle NormalButton(TextAnchor alignment, FontStyle fontStyle)
        {
            if (__NormalButton == null) {
                __NormalButton = new GUIStyle("button");
            }
            __NormalButton.alignment = alignment;
            __NormalButton.fontStyle = fontStyle;
            return __NormalButton;
        }

        private static GUIStyle __ActiveButton;
        public static GUIStyle FocusedButton(TextAnchor alignment, FontStyle fontStyle)
        {
            if (__ActiveButton == null) {
                __ActiveButton = new GUIStyle("button");
                __ActiveButton.normal = __ActiveButton.onActive;
            }
            __ActiveButton.alignment = alignment;
            __ActiveButton.fontStyle = fontStyle;
            return __ActiveButton;
        }


        public static GUIContent
            addContent = new GUIContent("+", "添加一个元素"),
            rmContent = new GUIContent("-", "移除一个元素");
    }
}