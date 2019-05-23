using UnityEngine;
using System.IO;
using System.Collections;

namespace ZFrame
{
    using Asset;
    public class AssetsMgr : MonoSingleton<AssetsMgr>
    {
        public event System.Action<int, int> onResolutionChanged;

        [SerializeField, AssetRef(name: "启动预设", type: typeof(GameObject))]
        private string m_LaunchPrefab;

#if UNITY_EDITOR
        public bool printLoadedLuaStack {
            get { return UnityEditor.EditorPrefs.GetBool(Prefs.kPrintLuaLoading); }
        }

        public bool useLuaAssetBundle {
            get { return UnityEditor.EditorPrefs.GetBool(Prefs.kUseLuaAssetBundle); }
        }

        public bool useAssetBundleLoader {
            get { return useLuaAssetBundle || UnityEditor.EditorPrefs.GetBool(Prefs.kUseAssetBundleLoader); }
        }
#elif UNITY_STANDALONE
	public bool printLoadedLuaStack { get { return false; } }
    public bool useLuaAssetBundle { get; private set; }
	public bool useAssetBundleLoader { get { return true; } }
#else
    public bool printLoadedLuaStack { get { return false; } }
    public bool useLuaAssetBundle { get; private set; }
	public bool useAssetBundleLoader { get { return true; } }
#endif
        public int resHeight { get; private set; }

        [Description("原始分辨率")]
        public Resolution RawResolution { get; private set; }

        private void SetQuality()
        {
            int quality = 6;
            string[] names = QualitySettings.names;
            if (quality >= names.Length) quality = names.Length - 1;
            Debug.Log("Quality set to " + names[quality]);
            QualitySettings.SetQualityLevel(quality);
        }

        public void SetResolution(int height)
        {
            if (height == 0) height = RawResolution.height;
            if (resHeight == height) return;

            resHeight = height;

#if !UNITY_STANDALONE
// 要设置的分辨率不能高于原始分辨率
            if (height > RawResolution.height) return;
#endif

            var width = (int)(height * (float)RawResolution.width / RawResolution.height);

#if UNITY_EDITOR

#else
        Screen.SetResolution(width, height, Screen.fullScreen);
#endif
            if (onResolutionChanged != null) onResolutionChanged.Invoke(width, height);

            Debug.LogFormat("Screen: {0} x {1}, fullScreen:{2}", width, height, Screen.fullScreen);
        }

        /*************************************************
         * 启动后加载Lua脚本
         *************************************************/
        public const string LUA_SCRIPT = "lua/script/";
        public const string LUA_CONFIG = "lua/config/";
        public const string KEY_MD5_STREAMING_LUA = "Streaming-Lua";
        public const string KEY_DATE_STREAMING_LUA = "Streaming-Lua-Date";
        public const string KEY_MD5_USING_LUA = "Using-Lua";
        public const string KEY_DATE_USING_LUA = "Using-Lua-Date";

        // 初始化Lua脚本
        private IEnumerator InitScriptsFromAssetBunles()
        {
            if (useAssetBundleLoader) {
                if (useLuaAssetBundle) {
                    yield return AssetBundleLoader.I.LoadMD5();

                    var md5 = AssetBundleLoader.I.md5;
                    string streamingMD5 = PlayerPrefs.GetString(KEY_MD5_STREAMING_LUA);
                    string streamingDate = PlayerPrefs.GetString(KEY_DATE_STREAMING_LUA);
                    if (md5 != streamingMD5) {
                        streamingMD5 = md5;
                        streamingDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        PlayerPrefs.SetString(KEY_MD5_STREAMING_LUA, streamingMD5);
                        PlayerPrefs.SetString(KEY_DATE_STREAMING_LUA, streamingDate);
                        LogMgr.D("Update streaming lua to [{0}] at [{1}]", streamingMD5, streamingDate);
                    }

                    string bundleMD5 = PlayerPrefs.GetString(KEY_MD5_USING_LUA);
                    string bundleDate = PlayerPrefs.GetString(KEY_DATE_USING_LUA);
                    var newInstallApp = false;
                    if (!string.IsNullOrEmpty(bundleMD5)) {
                        if (streamingMD5 != bundleMD5) {
                            // 已存在lua脚本在bundleRootPath, 比较时间           
                            var dtStreaming = System.DateTime.Parse(streamingDate);
                            var dtUsing = System.DateTime.Parse(bundleDate);
                            if (dtUsing < dtStreaming) {
                                // Streaming 的Lua脚本比较新，这是个新包。
                                newInstallApp = true;

                                PlayerPrefs.SetString(KEY_MD5_USING_LUA, streamingMD5);
                                PlayerPrefs.SetString(KEY_DATE_USING_LUA, streamingDate);
                                LogMgr.D("Update using lua to [{0}] at [{1}]", streamingMD5, streamingDate);
                            } else {
                                LogMgr.D("Use bundle lua of [{0}] at [{1}]", bundleMD5, bundleDate);
                            }
                        } else {
                            LogMgr.D("Using origin lua of [{0}] at [{1}]", streamingMD5, streamingDate);
                        }
                    } else {
                        // 首次启动游戏
                        PlayerPrefs.SetString(KEY_MD5_USING_LUA, streamingMD5);
                        PlayerPrefs.SetString(KEY_DATE_USING_LUA, streamingDate);
                        LogMgr.D("First using lua to [{0}] at [{1}]", streamingMD5, streamingDate);
                    }

                    yield return AssetBundleLoader.I.LoadFileList(newInstallApp);
                    yield return AssetLoader.Instance.LoadingAsset(null, LUA_SCRIPT, null, LoadMethod.Forever);
                    //yield return Loader.LoadingAsset(null, LUA_CONFIG, null, LoadMethod.Forever);
                } else {
                    yield return AssetBundleLoader.I.LoadFileList();
                }
            }

            if (ZFrame.UIManager.Instance == null) {
                yield return AssetLoader.Instance.LoadingAsset(null, "Shaders/", null, LoadMethod.Always);
                var loaded = new LoadedBundle();
                yield return AssetLoader.Instance.LoadingAsset(typeof(GameObject), m_LaunchPrefab, loaded, LoadMethod.Forever);
                GoTools.AddForever(loaded.asset as GameObject);
            }
        }

        protected override void Awaking()
        {
            DontDestroyOnLoad(gameObject);
            VersionMgr.Reset();

#if !UNITY_EDITOR
            // 自定义lua代码位置
            if (File.Exists("lua.txt")) {
                useLuaAssetBundle = false;
                var path = File.ReadAllText("lua.txt").Trim();
                if (!string.IsNullOrEmpty(path)) {
                    ChunkAPI.LuaROOT = path;
                }
            } else {
                useLuaAssetBundle = true;
            }
#endif

#if UNITY_5_5_OR_NEWER
            UnityEngine.Assertions.Assert.raiseExceptions = true;
#endif

            LogMgr.D("[Lua] {0}", useLuaAssetBundle ? "AssetBundle" : "Source Code");
            LogMgr.D("[Assets] {0}", useAssetBundleLoader ? "AssetBundle" : "Assets Folder");

            if (useAssetBundleLoader) {
                gameObject.AddComponent<AssetBundleLoader>();
            } else {
#if UNITY_EDITOR
                gameObject.AddComponent<AssetsSimulate>();
#else
                LogMgr.E("非编辑器模式不支持模拟使用AssetBundle。");
#endif
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            RawResolution = new Resolution() {
                width = Screen.width, height = Screen.height,
                refreshRate = Screen.currentResolution.refreshRate
            };
#else
            RawResolution = Screen.currentResolution;
#endif
            resHeight = 0;

            //Vectrosity.VectorLine.layerName = "Default";
        }

        private void Start()
        {
#if UNITY_EDITOR
            SetQuality();
#endif
            StartCoroutine(InitScriptsFromAssetBunles());
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        // 监控分辨率发生变化
        private int m_LastResW, m_LastResH;
        private void OnGUI()
        {
            if (m_LastResW != Screen.width || m_LastResH != Screen.height) {
                m_LastResW = Screen.width;
                m_LastResH = Screen.height;

                if (onResolutionChanged != null) onResolutionChanged.Invoke(m_LastResW, m_LastResH);
            }
        }
#endif
    }
}