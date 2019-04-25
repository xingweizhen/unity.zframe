using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    [Description("暂时未实现。\n可使用UILable的Tween代替。")]
    [RequireComponent(typeof(Text))]
    public class TypewriterEffect : MonoBehaviour
    {
        public float speed;
        private Text m_Text;

        // Use this for initialization
        private void Start()
        {
            m_Text = GetComponent<Text>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (m_Text) {

            }
        }
    }
}
