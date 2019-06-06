using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    public class LogViewer : MonoSingleton<LogViewer>
    {
        private class Log
        {
            private int m_Begin, m_End;
            public string text { get; private set; }
            private int m_Count;

            public Log(string text)
            {
                this.text = text;
                m_Count = 1;
                m_Begin = Mathf.RoundToInt(Time.realtimeSinceStartup * 1000);
                m_End = m_Begin;
            }

            public void Increase()
            {
                m_Count += 1;
                m_End = Mathf.RoundToInt(Time.realtimeSinceStartup * 1000);
            }

            public override string ToString()
            {
                return m_Count > 1 ?
                    string.Format("{0}-{1}|({2}) {3}", m_Begin, m_End, m_Count, text):
                    string.Format("{0}|{1}", m_Begin, text);
            }
        }

        // 顺序不能变
        private static readonly string[] ColorFmt = {
            "<color=#FF0000>{0}\n{1}</color>", // Error
		    "<color=#FF00FF>{0}\n{1}</color>", // Assert
		    "<color=#FFFF00>{0}</color>", // Warning
		    "{0}", // Log
		    "<color=#FF00FF>{0}\n{1}</color>", //Exception
        };

        [NamedProperty("根节点")]
        public Canvas root;

        private ScrollRect scrollRect;
        private int hasUpdate;
                
        private List<Log> m_Logs = new List<Log>(9999);
        private int m_Index;

        private ILoopLayout m_Loop;

        private bool IsRootActive()
        {
            return root && root.gameObject.activeSelf;
        }

        private void SetRootActive(bool active)
        {
            if (root) root.gameObject.SetActive(active);
        }

        protected override void Awaking()
        {
            Application.logMessageReceived += logMessageReceived;

            scrollRect = GetComponentInChildren<ScrollRect>();
            hasUpdate = 0;

            m_Loop = scrollRect.content.GetComponent(typeof(ILoopLayout)) as ILoopLayout;
            m_Loop.onItemUpdate += LoopGrid_onItemUpdate;
        }

        private void LoopGrid_onItemUpdate(GameObject ent, int index)
        {
            m_Loop.group.SetIndex(ent, index);
            var lb = ent.GetComponentInChildren(typeof(Text)) as Text;
            lb.text = m_Logs[index].ToString();
        }

        private void Start()
        {            
            root.worldCamera = UIManager.Instance.GetCanvas(0).worldCamera;

            var drop = GetComponentInChildren(typeof(Dropdown)) as Dropdown;
            if (drop) drop.value = (int)LogMgr.logLevel;

            SetRootActive(false);
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= logMessageReceived;
        }

        private void ShowContent()
        {
            SetRootActive(true);
            hasUpdate = 1;
            scrollRect.verticalNormalizedPosition = 0;
        }

        private bool multiTouched = false;
        private Vector2 m_TouchBegan, m_TouchEnd;
        /// <summary>
        /// 手势：三个手指同时向下或向上滑动
        /// </summary>
        private void processTouch()
        {
            if (multiTouched) {
                if (Input.touchCount > 0) {
                    var touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Ended) {
                        m_TouchEnd = touch.position;
                    }
                } else {
                    var vector = m_TouchEnd - m_TouchBegan;
                    var distance = vector.magnitude;
                    if (distance > 100) {
                        var dot = Vector3.Dot(Vector3.up, vector.normalized);
                        if (dot < -0.8f) {
                            ShowContent();
                        } else if (dot > 0.8f) {
                            SetRootActive(false);                        }
                    }
                    multiTouched = false;
                }
            } else if (Input.touchCount == 3) {
                multiTouched = true;
                m_TouchBegan = Input.GetTouch(0).position;
            }
        }

        private void LateUpdate()
        {
            if (hasUpdate > 0 && IsRootActive()) {
                m_Loop.SetTotalItem(m_Logs.Count, true);
            }
          
            hasUpdate -= 1;

            if (Input.GetKeyUp(KeyCode.Tab)) {
                if (IsRootActive()) {
                    SetRootActive(false);
                } else {
                    ShowContent();
                }
                return;
            }
            
#if UNITY_IOS || UNITY_ANDROID
            processTouch();
#endif
        }

        private void logMessageReceived(string condition, string stackTrace, LogType logType)
        {
            if (m_Logs.Count == m_Logs.Capacity)
                m_Logs.RemoveAt(0);

            var last = m_Logs.Count > 0 ? m_Logs[m_Logs.Count - 1] : null;

            string toAppend = null;
            switch (logType) {
                case LogType.Log:
                case LogType.Warning:
                    toAppend = string.Format(ColorFmt[(int)logType], condition);
                    break;
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    toAppend = string.Format(ColorFmt[(int)logType], condition, stackTrace);
                    break;
                default:
                    return;
            }

            if (last == null || last.text != toAppend) {
                m_Logs.Add(new Log(toAppend));
            } else {
                last.Increase();
            }
            hasUpdate = 1;
        }

        public void OnLogLevelChanged(int logLevel)
        {
            LogMgr.Instance.SetLevel((LogMgr.LogLevel)logLevel);
        }

        public void OnCmdLineLineClick()
        {
            UIManager.Instance.SendKey(KeyCode.F1);
        }
    }
}
