//
//  FrameRateCounter.cs
//  survive
//
//  Created by xingweizhen on 10/17/2017.
//
//

using UnityEngine;

namespace ZFrame
{
    public class FrameRateCounter : MonoBehaviour
    {
        private const string fpsLabel = "{0:N0}FPS {1:N0}MS</color>";

        public float UpdateInterval = 1.0f;
        public float fps { get; private set; }
        public float ms { get; private set; }

        private float m_LastInterval = 0;
        private int m_Frames = 0;
        private string m_HtmlColorTag = "<color=white>";

        private GUIStyle m_Style;
        
        private void Awake()
        {
             m_Style = new GUIStyle() { fontSize = 24, alignment = TextAnchor.UpperRight };
        }

        // Use this for initialization
        private void Start()
        {
            m_LastInterval = Time.realtimeSinceStartup;
            m_Frames = 0;
        }

        // Update is called once per frame
        private void Update()
        {
            m_Frames += 1;
            float timeNow = Time.realtimeSinceStartup;

            if (timeNow > m_LastInterval + UpdateInterval) {
                // display two fractional digits (f2 format)
                fps = m_Frames / (timeNow - m_LastInterval);
                ms = 1000.0f / Mathf.Max(fps, 0.00001f);

                if (fps < 20)
                    m_HtmlColorTag = "<color=yellow>";
                else if (fps < 10)
                    m_HtmlColorTag = "<color=red>";
                else
                    m_HtmlColorTag = "<color=green>";

                m_Frames = 0;
                m_LastInterval = timeNow;
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(Screen.width - 200, 0, 200, 30), string.Format(m_HtmlColorTag + fpsLabel, fps, ms), m_Style);
        }
    }
}
