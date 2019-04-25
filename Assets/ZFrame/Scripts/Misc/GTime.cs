using UnityEngine;
using System.Collections;

public static class GTime
{
    private static int m_Value = 1;
    public static int scaleValue {
        get { return m_Value; }
        set {
            if (m_Value != value) {
                m_Value = value;
                if (!IsPaused()) Time.timeScale = value;
            }
        }
    }

    private static int m_Scale = 1;

    public static void PauseTime()
    {
        if (m_Scale == 1) {
            Time.timeScale = 0;
            //if (AudioMgr.Singleton) {
            //    AudioMgr.Singleton.PauseAllSfx();
            //}
        }

        m_Scale--;
    }

    public static void ResumeTime()
    {
        if (m_Scale == 0) {
            Time.timeScale = m_Value;
            //AudioMgr.Singleton.ResumeAllSfx();
        }

        if (m_Scale < 1) m_Scale++;
    }

    public static void ResetTime()
    {
        m_Scale = 1;
        m_Value = 1;

        if (Time.timeScale == 0) {
			//AudioMgr.Singleton.ResumeAllSfx();
        }
        Time.timeScale = 1;
    }

    public static bool IsPaused()
    {
        return Time.timeScale == 0;
    }
}
