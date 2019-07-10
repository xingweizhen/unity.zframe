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
    public class ShowFPS : MonoBehaviour
    {
        private static Texture2D _backTex;
        public static Texture2D backTex {
            get {
                if (_backTex == null) {
                    _backTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    _backTex.SetPixel(0, 0, Color.black);
                    _backTex.Apply();
                }

                return _backTex;
            }
        }

        private const string fpsLabel = "FPS: {0:F1}</color>";

        [Range(0.1f, 1f)]
        public float UpdateInterval = 1.0f;

        public float ms { get; private set; }
        private float _fps;
        public float fps {
            get { return _fps; }
            private set {
                _fps = value;
                ms = 1000f / Mathf.Max(value, 0.0001f);
                if (value < 10)
                    m_HtmlColorTag = "<color=red>";
                else if (value < 20)
                    m_HtmlColorTag = "<color=yellow>";
                else
                    m_HtmlColorTag = "<color=green>";
            }
        }

        private float m_LastInterval = 0;
        private int m_Frames = 0;
        private string m_HtmlColorTag = "<color=white>";

        private void OnEnable()
        {
            fps = 1f / Time.unscaledDeltaTime;
            m_Frames = 0;
            m_LastInterval = Time.realtimeSinceStartup;
        }

        // Update is called once per frame
        private void Update()
        {
            m_Frames += 1;
            float timeNow = Time.realtimeSinceStartup;

            if (timeNow > m_LastInterval + UpdateInterval) {
                fps = m_Frames / (timeNow - m_LastInterval);
                m_Frames = 0;
                m_LastInterval = timeNow;
            }
        }

        private void OnGUI()
        {
            var showRt = new Rect(Screen.width - 83, 5, 78, 20);
            GUI.DrawTexture(showRt, backTex);

            showRt.x += 5;
            GUI.Label(showRt, string.Format(m_HtmlColorTag + fpsLabel, fps, ms));
        }
    }
}
