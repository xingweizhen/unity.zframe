using System.Collections;
using System.Collections.Generic;
using TinyJSON;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif
using NameFuncPair = LuaMethod;
using ILuaState = System.IntPtr;

namespace ZFrame
{
    using Asset;
    public class LuaScriptMgr : MonoSingleton<LuaScriptMgr>
    {
        public const string SCRIPT_FILE = "framework/main";
        public const string F_ON_LEVEL_LOADED = "on_level_loaded";
        public const string F_ON_ASSET_DOWNLOAD_START = "on_asset_download_start";
        public const string F_ON_ASSET_DOWNLOAD_ERROR = "on_asset_download_error";
        public const string F_ON_ASSET_DOWNLOAD_VERIFY_FAILED = "on_asset_download_verifyfailed";
        public const string F_ON_ASSET_DOWNLOADED = "on_asset_downloaded";
        public const string F_ON_ASSET_DOWNLOADING = "on_asset_downloading";
        public const string S_APP_PERSISTENTDATA_PATH = "app_persistentdata_path";
        public const string S_APP_STREAMINGASSETS_PATH = "app_streamingassets_Path";
        public const string B_USING_ASSETBUNDLE = "using_assetbundle";

        private LuaTable m_Tb;
        private LuaEnv m_Env;
        public LuaEnv Env {
            get {
                if (m_Env == null) {
                    m_Env = new LuaEnv();
                    m_Env.AddLoader(ChunkAPI.__Loader);
                    m_Env.AddBuildin(LibUnity.LIB_NAME, LibUnity.OpenLib);
                    m_Env.AddBuildin(LibAsset.LIB_NAME, LibAsset.OpenLib);
                    m_Env.AddBuildin(LibUGUI.LIB_NAME, LibUGUI.OpenLib);
                    m_Env.AddBuildin(LibSystem.LIB_NAME, LibSystem.OpenLib);
                    m_Env.AddBuildin(LibNetwork.LIB_NAME, LibNetwork.OpenLib);
                    m_Env.AddBuildin(LibTool.LIB_NAME, LibTool.OpenLib);
                    m_Env.AddBuildin(LibCSharpIO.LIB_NAME, LibCSharpIO.OpenLib);
                    // m_Env.AddBuildin(LibGame.LIB_NAME, LibGame.OpenLib);
                    // m_Env.AddBuildin(LibGVoice.LIB_NAME, LibGVoice.OpenLib);

                    var L = m_Env.L;
                    L.GetGlobal("_G");
                    L.SetDict("loadfile", StaticLuaCallbacks.loadfile);
                    L.SetDict("dofile", StaticLuaCallbacks.dofile);
                    L.SetDict("lsetmetatable", StaticLuaCallbacks.lsetmetatable);
                    L.SetDict("lgetmetatable", StaticLuaCallbacks.lgetmetatable);
                    L.Pop(1);

                    UnityEngine_Vector2.Wrap(L);
                    UnityEngine_Vector3.Wrap(L);
                    UnityEngine_Vector4.Wrap(L);
                    UnityEngine_Quaternion.Wrap(L);
                    UnityEngine_Bounds.Wrap(L);
                    UnityEngine_Color.Wrap(L);

                    var translator = m_Env.translator;
                    translator.RegisterPushAndGetAndUpdate(UnityEngine_Vector2.PushX, 
                        new ObjectTranslator.GetFunc<Vector2>(I2V.ToVector2), UnityEngine_Vector2.Update);
                    translator.RegisterPushAndGetAndUpdate(UnityEngine_Vector3.PushX, 
                        new ObjectTranslator.GetFunc<Vector3>(I2V.ToVector3), UnityEngine_Vector3.Update);
                    translator.RegisterPushAndGetAndUpdate(UnityEngine_Vector4.PushX, 
                        new ObjectTranslator.GetFunc<Vector4>(I2V.ToVector4), UnityEngine_Vector4.Update);
                    translator.RegisterPushAndGetAndUpdate(UnityEngine_Color.PushX, 
                        new ObjectTranslator.GetFunc<Color>(I2V.ToColor), UnityEngine_Color.Update);
                    translator.RegisterPushAndGetAndUpdate(UnityEngine_Quaternion.PushX, 
                        new ObjectTranslator.GetFunc<Quaternion>(I2V.ToQuaternion), UnityEngine_Quaternion.Update);
                    translator.RegisterPushAndGetAndUpdate(UnityEngine_Bounds.PushX, 
                        new ObjectTranslator.GetFunc<Bounds>(I2V.ToBounds), UnityEngine_Bounds.Update);
                    translator.RegisterPushAndGetAndUpdate(LuaIndexPush.PushX,
                        new ObjectTranslator.GetFunc<Variant>(I2V.ToJsonObj), null);

                    NoBoxingBool.Instance = new BoolToLua();
                    NoBoxingInt.Instance = new IntToLua();
                    NoBoxingFloat.Instance = new FloatToLua();
                    NoBoxingVector2.Instance = new NoBoxingVector2();
                }
                return m_Env;
            }
        }
        public ILuaState L { get { return Env.L; } }
        public bool IsLua { get { return Env != null; } }

        protected override void Awaking()
        {
            AssetLoader.Instance.CollectGarbage += () => L.GC(LuaGCOptions.LUA_GCCOLLECT, 0);

            var ret = L.DoFile(SCRIPT_FILE);
            Assert.IsTrue(ret == 1);

            m_Tb = L.ToLuaTable(-1);
            L.Pop(1);
            Assert.IsNotNull(m_Tb);

            m_Tb.CallFunc(0, "awake");
        }

        private void OnUIClick(GameObject go)
        {
            m_Tb.CallFunc(0, "on_ui_click", go);
        }

        private void Start()
        {
            // 设定全局参数
            L.GetGlobal("ENV");
            L.SetDict(S_APP_PERSISTENTDATA_PATH, AssetBundleLoader.persistentDataPath);
            L.SetDict(S_APP_STREAMINGASSETS_PATH, AssetBundleLoader.streamingAssetsPath);
            L.SetDict(B_USING_ASSETBUNDLE, AssetBundleLoader.I != null);

#if DEVELOPMENT_BUILD || UNITY_EDITOR || UNITY_STANDALONE
            L.SetDict("development", true);
#endif
            L.Pop(1);

            // "none break space"
            L.GetGlobal("string");
            L.SetDict("nbs", "\u00A0");
            L.Pop(1);

            m_Tb.CallFunc(0, "start", gameObject);

            UGUI.UIButton.onButtonClick = OnUIClick;
            UGUI.UIToggle.onToggleClick = OnUIClick;
            UIManager.Instance.onKey += (key) => m_Tb.CallFunc(0, "on_key", key.ToString());

            if (AssetDownload.Instance) {
                AssetDownload.Instance.onDownloadStart += (bundleName) => {
                    var lua = Instance.L;
                    lua.GetGlobal("SCENE", F_ON_ASSET_DOWNLOAD_START);
                    var b = lua.BeginPCall();
                    lua.PushString(bundleName);
                    lua.ExecPCall(1, 0, b);
                };

                AssetDownload.Instance.onDownloadError += (bundleName, error) => {
                    var lua = Instance.L;
                    lua.GetGlobal("SCENE", F_ON_ASSET_DOWNLOAD_ERROR);
                    var b = lua.BeginPCall();
                    lua.PushString(bundleName);
                    lua.PushString(error);
                    lua.ExecPCall(2, 0, b);
                };

                AssetDownload.Instance.onVerifyFailed += (bundleName, md5) => {
                    var lua = Instance.L;
                    lua.GetGlobal("SCENE", F_ON_ASSET_DOWNLOAD_VERIFY_FAILED);
                    var b = lua.BeginPCall();
                    lua.PushString(bundleName);
                    lua.PushString(md5);
                    lua.ExecPCall(2, 0, b);
                };

                AssetDownload.Instance.onDownloaded += (bundleName, siz) => {
                    var lua = Instance.L;
                    lua.GetGlobal("SCENE", F_ON_ASSET_DOWNLOADED);
                    var b = lua.BeginPCall();
                    lua.PushString(bundleName);
                    lua.PushLong(siz);
                    lua.ExecPCall(2, 0, b);
                };

                AssetDownload.Instance.onDownloading += (bundleName, current, total) => {
                    var lua = Instance.L;
                    lua.GetGlobal("SCENE", F_ON_ASSET_DOWNLOADING);
                    var b = lua.BeginPCall();
                    lua.PushString(bundleName);
                    lua.PushLong(current);
                    lua.PushLong(total);
                    lua.ExecPCall(3, 0, b);
                };

            }

            // 执行预加载
            AssetLoader.Instance.ExecutePreload(OnAssetBundleLoaded);

#if !UNITY_EDITOR
            //GameSettings.Instance.GetSettings();
#endif
        }

        /*************************************************
         * Prepare Lua Scripts. <IMPORTANT>
         *************************************************/

        // Lua脚本全部准备就绪
		public static void OnScriptsFinish()
        {
            // var txt = Resources.Load<TextAsset>("luacoding");
            // CLZF2.Decrypt(txt.bytes, txt.bytes.Length);

            UIManager.Instance.gameObject.AddComponent<LuaScriptMgr>();

            Application.runInBackground = true;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private static void OnAssetBundleLoaded(string a, object o, object p)
        {
            // Launching Scene Loaded:
            //   SCENE.on_level_loaded(levelName, launching)
            //
            var lua = Instance.L;
            lua.GetGlobal("SCENE", F_ON_LEVEL_LOADED);
            var b = lua.BeginPCall();
            lua.PushString(SceneManager.GetActiveScene().name);
            lua.PushBoolean(true);
            lua.ExecPCall(2, 0, b);
        }
    }
}
