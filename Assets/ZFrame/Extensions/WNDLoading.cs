using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ZFrame;
using ZFrame.UGUI;
using ZFrame.Asset;
#if ULUA
using LuaInterface;
#else
using XLua;
#endif

public class WNDLoading : MonoSingleton<WNDLoading>, IAssetProgress, IPoolable
{
    public static string loadedLevelName { get; private set; }
    
    public float assetsProg = 0.5f;
    public float barStep = 0.066f;
    public UISlider sldLoading;
    private string sceneBundle, scenePath;
    private int m_FuncRef;

    System.Text.StringBuilder strTrace;
    float beginTime;
    float lastTime;

    // Use this for initialization
    IEnumerator Start()
    {
        var Loader = AssetLoader.Instance;

        sldLoading.value = 0f;
        yield return null;

        var ignoreList = new List<Transform>() { transform };
        if (LogViewer.Instance) ignoreList.Add(LogViewer.Instance.cachedTransform);
        UIManager.ClearAll(ignoreList);
        yield return null;

        Loader.StopLoading();
        yield return null;

        Loader.UnloadAll();

        var wnd = GetComponent(typeof(UIWindow)) as UIWindow;
        if (wnd) {
            wnd.SendEvent(wnd, UIEvent.Send, "start_loading");
        }

        strTrace = new System.Text.StringBuilder("\n");
        beginTime = Time.realtimeSinceStartup;
        lastTime = beginTime;

        Loader.CachedPreload(sceneBundle, LoadMethod.Default);
        Loader.CachedPreload(scenePath, LoadMethod.Default);
        yield return Loader.PreloadingBundles(this);

        var asynOpt = Loader.LoadLevelAsync(scenePath);
        while (!asynOpt.isDone) {
            yield return null;
            SetSliderValue(assetsProg + asynOpt.progress * (1 - assetsProg));
        }
        SetSliderValue(1f);
        LogTime("LoadLevel");

        yield return null;

        Loader.Unload(scenePath, false);        

        loadedLevelName = Path.GetFileNameWithoutExtension(scenePath);

        if (m_FuncRef != 0) {
            var L = LuaScriptMgr.Instance.L;
            L.GetRef(m_FuncRef);
            L.Func(0, loadedLevelName);
            L.L_Unref(LuaIndexes.LUA_REGISTRYINDEX, m_FuncRef);
            m_FuncRef = 0;
        }

        if (wnd) {
            wnd.SendEvent(wnd, UIEvent.Send, "level_loaded", loadedLevelName);
        }

        LogMgr.W(strTrace.ToString());
    }

    public void SetSliderValue(float value)
    {
        sldLoading.value = value;
    }

    void IAssetProgress.SetProgress(float progress)
    {
        SetSliderValue(progress * assetsProg);
    }

    void IAssetProgress.OnBundleLoaded(string bundleName, AbstractAssetBundleRef bundle)
    {
        var wnd = GetComponent(typeof(UIWindow)) as UIWindow;
        if (wnd) {
            wnd.SendEvent(wnd, UIEvent.Send, "bundle_loaded", bundleName);
        }
    }

    void LogTime(string str)
    {
        float curr = Time.realtimeSinceStartup;
        strTrace.AppendFormat("{0:F6}|{1:F6}={2}\n",
            curr - beginTime, curr - lastTime, str);
        lastTime = curr;
    }

    private static void GetSceneBundles(string levelName, out string sceneBundle, out string scenePath)
    {
        var sceneRoot = SystemTools.GetDirPath(levelName);
        scenePath = string.Format("{0}/{1}/{1}", SystemTools.GetDirPath(sceneRoot), Path.GetFileName(levelName));
        sceneBundle = sceneRoot + "/";
    }

    private static IEnumerator LoadLevel(string levelName, int funcRef)
    {
        var Loader = AssetLoader.Instance;
        // 可能处于资源加载的回调中，延后一帧再启动下一个资源加载
        Loader.StopLoading();
        yield return null;
        string sceneBundle, scenePath;
        GetSceneBundles(levelName, out sceneBundle, out scenePath);
        yield return Loader.LoadingAsset(null, sceneBundle);
        yield return Loader.LoadingAsset(null, scenePath);
        var asynOpt = Loader.LoadLevelAsync(scenePath, LoadSceneMode.Additive);

        while (!asynOpt.isDone) {
            yield return null;
        }
        Loader.Unload(scenePath, false);
        Loader.Unload(sceneBundle, false);

        if (funcRef != 0) {
            var L = LuaScriptMgr.Instance.L;
            L.GetRef(funcRef);
            L.Func(0, Path.GetFileNameWithoutExtension(scenePath));
            L.L_Unref(LuaIndexes.LUA_REGISTRYINDEX, funcRef);
        }
    }

    public static void LoadLevel(string levelName, LoadSceneMode mode, int funcRef)
    {
        if (mode == LoadSceneMode.Additive) {
            UIManager.Instance.StartCoroutine(LoadLevel(levelName, funcRef));
        } else {
            if (Instance == null || !Instance.isActiveAndEnabled) {
                // 场景资源加入预加载列表
                string sceneBundle, scenePath;
                GetSceneBundles(levelName, out sceneBundle, out scenePath);

                LuaComponent.ShowWindow("UI/WNDLoading", 101);
                Instance.sceneBundle = sceneBundle;
                Instance.scenePath = scenePath;
                Instance.m_FuncRef = funcRef;
            }
        }
    }

    void IPoolable.OnRestart()
    {
        this.enabled = true;
        StartCoroutine(Start());
    }

    void IPoolable.OnRecycle()
    {
        this.enabled = false;
    }
}
