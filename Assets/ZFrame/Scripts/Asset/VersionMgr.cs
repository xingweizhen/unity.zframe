using UnityEngine;
using System.Collections;
using System.IO;

namespace ZFrame.Asset
{
    public static class VersionMgr
    {
        public static VersionInfo GetAppVer()
        {
            return Resources.Load("version", typeof(VersionInfo)) as VersionInfo;
        }

#if UNITY_EDITOR
        public static VersionInfo GetAssetVer()
        {
            // var assetVerPath = string.Format("Assets/{0}/version.asset", AssetBundleLoader.DIR_ASSETS);
            // return UnityEditor.AssetDatabase.LoadAssetAtPath<VersionInfo>(assetVerPath);
            return GetAppVer();
        }
#endif

        public class Info
        {
            public string version;
            public int code;
        }

        private static Info s_AppVer, s_AssetVer;

        public static Info AppVersion {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    s_AppVer = null;
                }
#endif
                if (s_AppVer == null) {
                    var vi = GetAppVer();
                    s_AppVer = new Info() { version = vi.version, code = vi.code };
                }
                return s_AppVer;
            }
        }

        public static void Reset()
        {
            s_AppVer = null;
            s_AssetVer = null;
        }

        public static Info AssetVersion {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    s_AssetVer = null;
                }
#endif
                if (s_AssetVer == null) {
                    if (AssetBundleLoader.I) {
                        var filelistPath = AssetBundleLoader.bundleRootPath + "/" + AssetBundleLoader.FILE_LIST;
                        if (File.Exists(filelistPath)) {
                            string jsonStr = File.ReadAllText(filelistPath);
                            var jo = TinyJSON.JSON.Load(jsonStr);
                            s_AssetVer = new Info() {
                                version = jo["version"], code = jo["code"]
                            };
                        }
                    } else {
#if UNITY_EDITOR
                        var vi = GetAssetVer();
                        s_AssetVer = new Info() { version = vi.version, code = vi.code };
#else
                            
#endif
                    }
                }

                return s_AssetVer;
            }
        }
    }
}
