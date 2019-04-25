using UnityEngine;
using System.Collections;

/// <summary>
/// Mono单例
/// </summary>
public abstract class MonoSingleton<T> : MonoBehavior where T : MonoBehavior
{
    public static T Instance { get; private set; }

    protected virtual void Awaking() { }
    protected virtual void Destroying() { }

    private void Awake()
    {
        if (!Instance) {
            Instance = this as T;
            Awaking();
        } else {
            LogMgr.E(string.Format("出现重复的单例脚本<{0}>，我帮你删掉了！", typeof(T).FullName));
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (this == Instance) {
            Destroying();
            Instance = null;
        }
    }
}

/// <summary>
/// 非线程安全，只可在Unity主线程中访问
/// </summary>
public abstract class Singleton<T> where T : class, new()
{
    protected Singleton() { }

    private static T m_Inst;
    public static T Instance {
        get {
            if (m_Inst == null) {
                m_Inst = new T();
            }
            return m_Inst;
        }
    }
    public static void Release() { m_Inst = null; }
}
