using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(ILabel))]
    public class Localize : MonoBehaviour
    {
        public static string DEF_LANG = "enUS";
        
        [SerializeField]
        private Graphic m_Target;
        private ILabel m_Label { get {
                ILabel lb = null;
                if (m_Target) lb = m_Target.GetComponent(typeof(ILabel)) as ILabel;
                return lb;
            }
        }
        
        private string m_Lang { get { return DEF_LANG; } }
        private int m_LangIdx;

        private void Awake()
        {
            var label = m_Label;
            if (label != null) label.onTextChanged += OnTextChanged;
        }

        private void OnDestroy()
        {
            var label = m_Label;
            if (label != null) label.onTextChanged -= OnTextChanged;
        }

        private void OnEnable()
        {
            var lang = m_Lang;
            if (UILabel.LOC && lang != null && lang.Length > 0) {
                m_LangIdx = UILabel.LOC.FindLangIndex(m_Lang);
            } else {
                m_LangIdx = -1;
            }

            var label = m_Label;
            if (label != null) OnTextChanged(label.rawText);
        }

        private void OnTextChanged(string text)
        {
            if (!this.isActiveAndEnabled) return;
                
            var locText = string.Empty;
            if (m_LangIdx > -1) {
                locText = UILabel.LOC.Get(text, m_LangIdx);
                if (locText == null) {
                    LogMgr.W("本地化获取失败：Lang = {0}, Key = {1} @ {2}",
                        m_Lang, text, transform.GetHierarchy(null));
                }
            }

            (GetComponent(typeof(ILabel)) as ILabel).text = locText;
        }
    }
}
