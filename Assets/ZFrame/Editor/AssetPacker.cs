using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
//using Assets.Editor.Utils;
using TinyJSON;

namespace ZFrame.Editors
{
    using Asset;
    public static class AssetPacker
    {
        [System.Flags]
        public enum PackSymbol
        {
            SHOW_FPS = 1,
            XWZ_DEBUG = 2,
            RELEASE = 4,
        }

        public enum AssetMode
        {
            Full,
            Split,
            Micro,
        }

        public static string EditorStreamingAssetsPath {
            get {
                string streamingPath = AssetBundleLoader.streamingRootPath;
                if (!Directory.Exists(streamingPath)) {
                    SystemTools.NeedDirectory(streamingPath);
                }

                return streamingPath;
            }
        }

        public static string EditorPersistentAssetsPath {
            get {
                string persistentPath = AssetBundleLoader.bundleRootPath;
                if (!Directory.Exists(persistentPath)) {
                    SystemTools.NeedDirectory(persistentPath);
                }

                return persistentPath;
            }
        }

        public static string EditorDownloadAssetsPath {
            get {
                string downloadPath = AssetBundleLoader.CombinePath(
                    AssetBundleLoader.streamingAssetsPath, AssetBundleLoader.DOWNLOAD_FOLDER);
                if (!Directory.Exists(downloadPath)) {
                    SystemTools.NeedDirectory(downloadPath);
                }

                return downloadPath;
            }
        }

        private static string m_StreamingAssetsPath {
            get {
                return string.Format("{0}/{1}",
                    Application.streamingAssetsPath, AssetBundleLoader.ASSETBUNDLE_FOLDER);
            }
        }

        public static string StreamingAssetsPath {
            get {
                string path = m_StreamingAssetsPath;
                if (!Directory.Exists(path)) {
                    SystemTools.NeedDirectory(path);
                }

                return path;
            }
        }

        public static BuildTarget buildTarget;
        public static PackSymbol symbol;
        public static BuildOptions options = BuildOptions.Development;
        public static AssetMode assetMode = AssetMode.Full;

        public static int assetCode {
            set { VersionMgr.GetAppVer().code = value; }
            get { return VersionMgr.GetAppVer().code; }
        }
        
        private const string OTHERS = "Others";

        public static readonly List<string> LaunchBundles = new List<string> {
            "lua/script",
            "shaders", "launch", "game", "dynamicfont", "ui",
            "fmod/master bank-strings", "fmod/master bank", "fmod/ui", //"fmod/worldmapbgm"
            "scenes/login",
            "atlas/common", "atlas/login", "atlas/commonicon", "atlas/itemicon",
            "shared/animation",
            "rawtex/bg_com_002", "rawtex/bg_com_005", "rawtex/bg_com_006", "rawtex/bg_com_big", "rawtex/bg_com_small",
        };

        public static void Log(string fmt, params object[] Args)
        {
            Debug.LogFormat("<color=blue><b>[PACK] " + fmt + "</b></color>", Args);
        }

        /// <summary>
        /// 压缩和打包Lua脚本/配置
        /// </summary>
        public static void EncryptLua()
        {
            CLZF2.Decrypt(null, 260769);
            CLZF2.Decrypt(new byte[1], 3);

            string CodeRoot = Path.Combine(Application.dataPath, "LuaCodes");
            string scriptDir = Path.Combine(CodeRoot, "Script");
            string configDir = Path.Combine(CodeRoot, "Config");
            if (!Directory.Exists(scriptDir)) {
                SystemTools.NeedDirectory(scriptDir);
                AssetDatabase.Refresh();
            }

            var ai = AssetImporter.GetAtPath("Assets/LuaCodes/Script");
            ai.assetBundleName = "lua/script";

            if (!Directory.Exists(configDir)) {
                SystemTools.NeedDirectory(configDir);
                AssetDatabase.Refresh();
            }

            ai = AssetImporter.GetAtPath("Assets/LuaCodes/Config");
            ai.assetBundleName = "lua/config";

            var scripts = new DirectoryInfo(scriptDir).GetFiles("*.bytes");
            var configs = new DirectoryInfo(configDir).GetFiles("*.bytes");
            var listExists = new List<string>();
            foreach (var f in scripts) listExists.Add("Script/" + f.Name);
            foreach (var f in configs) listExists.Add("Config/" + f.Name);

            DirectoryInfo dirLua = new DirectoryInfo(Application.dataPath + "/../Essets/LuaRoot");
            FileInfo[] files = dirLua.GetFiles("*.lua", SearchOption.AllDirectories);
            int startIndex = dirLua.FullName.Length + 1;
            var nFiles = 0;
            foreach (FileInfo f in files) {
                string fullName = f.FullName.Substring(startIndex).Replace('/', '%').Replace('\\', '%');
                if (fullName.StartsWith("debug")) continue;
                string fileName = fullName.Remove(fullName.Length - 4) + ".bytes";

                string[] lines = File.ReadAllLines(f.FullName);
                // 以"--"开头的注释以换行符代替
                List<string> liLine = new List<string>();
                foreach (var l in lines) {
                    string ltim = l.Trim();
                    if (ltim.StartsWith("--") && !ltim.StartsWith("--[[") && !ltim.StartsWith("--]]")) {
                        liLine.Add("\n");
                    } else {
                        liLine.Add(l + "\n");
                    }
                }

                string codes = string.Concat(liLine.ToArray());
                byte[] nbytes = System.Text.Encoding.UTF8.GetBytes(codes);
                if (nbytes.Length > 0) {
                    nbytes = CLZF2.DllCompress(nbytes);
                    CLZF2.Encrypt(nbytes, nbytes.Length);
                } else {
                    Debug.LogWarning("Compress Lua: " + fileName + " is empty!");
                }

                string path;
                if (fileName.StartsWith("config")) {
                    listExists.Remove("Config/" + fileName);
                    path = Path.Combine(configDir, fileName);
                } else {
                    listExists.Remove("Script/" + fileName);
                    path = Path.Combine(scriptDir, fileName);
                }

                if (File.Exists(path)) {
                    using (var file = File.OpenWrite(path)) {
                        file.Seek(0, SeekOrigin.Begin);
                        file.Write(nbytes, 0, nbytes.Length);
                        file.SetLength(nbytes.Length);
                    }
                } else {
                    File.WriteAllBytes(path, nbytes);
                }

                nFiles++;
            }

            foreach (var n in listExists) {
                var path = Path.Combine(CodeRoot, n);
                File.Delete(path);
                Log("Delete: {0}", n);
            }

            Log("Compress {0} files success. => {1}", nFiles, CodeRoot);
        }

        private static bool HasAssetInBuild(List<AssetBundleBuild> list, string abName)
        {
            foreach (var abb in list) {
                if (abb.assetBundleName == abName) return true;
            }

            return false;
        }

        public static void GetPackAssetNames(out List<string> inAssets, out List<string> dlAssets)
        {
            inAssets = new List<string>();
            dlAssets = new List<string>();

            if (assetMode != AssetMode.Full) {
                // 尝试读取内部资源配置
                var jsonObj = GetSubAssets("major");
                if (jsonObj != null) {
                    foreach (var jo in jsonObj) {
                        ProxyArray joArray = jo.Value as ProxyArray;
                        foreach (var name in joArray) {
                            inAssets.Add(name);
                        }
                    }
                }
            }

            if (assetMode == AssetMode.Split) {
                // 尝试读取下载资源配置
                var jsonObj = GetSubAssets("minor");
                if (jsonObj != null) {
                    foreach (var jo in jsonObj) {
                        int minLevel = (int)jo.Value.ConvTo("minLevel", 0);
                        ProxyArray joArray = null;
                        if (minLevel > 0) {
                            joArray = jo.Value["Assets"] as ProxyArray;
                        } else {
                            joArray = jo.Value as ProxyArray;
                        }

                        foreach (var name in joArray) {
                            dlAssets.Add(name);
                        }
                    }
                }
            }

            var AllAssetBundles = AssetDatabase.GetAllAssetBundleNames();
            if (inAssets.Count == 0) {
                foreach (var name in AllAssetBundles) {
                    if (!dlAssets.Contains(name)) {
                        inAssets.Add(name);
                    }
                }
            } else {
                // 把未管理的资源加入需下载资源
                foreach (var name in AllAssetBundles) {
                    if (!inAssets.Contains(name) && !dlAssets.Contains(name)) {
                        dlAssets.Add(name);
                    }
                }
            }
        }

        public static void PackAssets()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetBundleMenu.AutoMarkAssetBundle();

            AssetDatabase.Refresh();

            // 打包内部资源 - ChunkBasedCompression
            BuildPipeline.BuildAssetBundles(EditorStreamingAssetsPath,
                BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);

            if (assetMode == AssetMode.Split) {
                // 额外打包下载资源 - UncompressedAssetBundle
                BuildPipeline.BuildAssetBundles(EditorDownloadAssetsPath,
                    BuildAssetBundleOptions.UncompressedAssetBundle, buildTarget);
            }

            // 更新资源版本号
            // VersionMgr.SaveAssetVersion(GitTools.getVerInfo());
            AssetDatabase.Refresh();

            Log("BuildAssetBundles success. => {0}", EditorStreamingAssetsPath);

            // 提取完整资源
            var allAssetsPath = AssetBundleLoader.streamingAssetsPath + "/AllAssets";
            if (Directory.Exists(allAssetsPath)) {
                Directory.Delete(allAssetsPath, true);
            }

            SystemTools.NeedDirectory(allAssetsPath);

            SystemTools.CopyDirectory(EditorStreamingAssetsPath, allAssetsPath, "*", (path) => {
                // 忽略无关文件
                return path[0] == '.' || path.EndsWith(".manifest");
            });
        }

        public const string MINOR_PATH = "Issets/MinorAssets";

        private static void PackMinorAssets(DirectoryInfo dir, string name, IEnumerable joArray)
        {
            var list = new List<FileInfo>();
            foreach (var ab in joArray) {
                list.Add(new FileInfo(Path.Combine(dir.FullName, (string)ab)));
            }

            if (list.Count > 0) {
                ZFrame.Bundle.Pack(dir, list, Path.Combine(MINOR_PATH, name));
            }
        }

        public static ProxyObject GetSubAssets(string assetPack)
        {
            var path = string.Format("Assets/Editor/{0}assets_{1}.txt", assetPack, buildTarget);
            if (File.Exists(path)) {
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
                return JSON.Load(asset.text) as ProxyObject;
            }

            return null;
        }

        /// <summary>
        /// 生成次要资源包
        /// </summary>
        public static void GenMinorAssets()
        {
            var mode = assetMode;
            assetMode = AssetMode.Split;
            List<string> inAssets, dlAssets;
            GetPackAssetNames(out inAssets, out dlAssets);
            assetMode = mode;

            if (dlAssets.Count == 0) {
                Log("没有下载资源需要生成");
                return;
            }

            var rootDir = new DirectoryInfo(EditorDownloadAssetsPath);

            var jsonObj = GetSubAssets("minor");
            if (jsonObj != null) {
                foreach (var jo in jsonObj) {
                    int minLevel = (int)jo.Value.ConvTo("minLevel", 0);
                    if (minLevel > 0) {
                        var array = jo.Value["Assets"] as ProxyArray;
                        PackMinorAssets(rootDir, jo.Key, array);
                        foreach (var name in array) dlAssets.Remove(name);
                    } else {
                        var array = jo.Value as ProxyArray;
                        PackMinorAssets(rootDir, jo.Key, array);
                        foreach (var name in array) dlAssets.Remove(name);
                    }
                }
            }

            if (dlAssets.Count > 0) {
                PackMinorAssets(rootDir, OTHERS, dlAssets);
            }
        }

        private static readonly HashSet<string> IgnoreInFilelist = new HashSet<string> {
            "AssetBundles", "md5", "filelist",
        };

        private static IEnumerator<FileInfo> ForEachAssetBundle()
        {
            DirectoryInfo dir = new DirectoryInfo(EditorStreamingAssetsPath);
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; ++i) {
                var file = files[i];
                // 忽略无关文件
                if (file.Name[0] == '.' || file.Extension == ".manifest") continue;
                if (IgnoreInFilelist.Contains(file.Name)) continue;

                yield return file;
            }
        }

        /// <summary>
        /// 生成整包资源列表
        /// </summary>
        /// <param name="resInf"></param>
        private static ResInf GenFullPackageFileList(ResInf resInf)
        {
            ResInf majorInf = new ResInf(resInf);
            List<string> inAssets = null, dlAssets = null;
            GetPackAssetNames(out inAssets, out dlAssets);

            foreach (var kv in resInf.Assets) {
                var assetName = kv.Key;
                var assetInf = (AssetInf)kv.Value;
                majorInf.Assets.Add(assetName, new AssetInf {md5 = assetInf.md5, siz = 0,});
            }

            return majorInf;
        }

        /// <summary>
        /// 生成分包模式的资源列表
        /// </summary>
        /// <param name="resInf"></param>
        /// <returns>主包资源</returns>
        private static ResInf GenSplitPackageFileList(ResInf resInf)
        {
            ResInf majorInf = new ResInf(resInf) {
                Downloads = new Dictionary<string, DownloadInf>()
            };
            List<string> inAssets = null, dlAssets = null;
            GetPackAssetNames(out inAssets, out dlAssets);

            foreach (var kv in resInf.Assets) {
                if (inAssets.Contains(kv.Key)) {
                    majorInf.Assets.Add(kv.Key, kv.Value);
                }
            }

            var jsonObj = GetSubAssets("minor");
            if (jsonObj != null) {
                foreach (var jo in jsonObj) {
                    int minLevel = (int)jo.Value.ConvTo("minLevel", 0);
                    if (minLevel > 0) {
                        string fileName = jo.Key;
                        var fileInfo = new FileInfo(Path.Combine(MINOR_PATH, fileName));
                        var maxLevel = (int)jo.Value.ConvTo("maxLevel", minLevel);
                        majorInf.Downloads.Add(fileName,
                            new DownloadInf() {siz = fileInfo.Length, minLevel = minLevel, maxLevel = maxLevel});
                    }
                }
            }

            var othersPath = Path.Combine(MINOR_PATH, OTHERS);
            if (File.Exists(othersPath)) {
                var fileInfo = new FileInfo(othersPath);
                majorInf.Downloads.Add(OTHERS, new DownloadInf() {siz = fileInfo.Length, minLevel = 0, maxLevel = 0});
            }

            return majorInf;
        }

        /// <summary>
        /// 生成微端模式的资源列表
        /// </summary>
        /// <param name="resInf"></param>
        /// <returns>主包资源</returns>
        private static ResInf GenMicroPackageFileList(ResInf resInf)
        {
            ResInf majorInf = new ResInf(resInf);
            List<string> inAssets = null, dlAssets = null;
            GetPackAssetNames(out inAssets, out dlAssets);

            foreach (var kv in resInf.Assets) {
                var assetName = kv.Key;
                var assetInf = (AssetInf)kv.Value;
                var majorEnt = inAssets.Contains(assetName) ? new AssetInf {md5 = assetInf.md5, siz = 0,} : new EntryInf {md5 = assetInf.md5,};
                majorInf.Assets.Add(assetName, majorEnt);
            }

            return majorInf;
        }

        private static void GenPatchAssets(ResInf resInf)
        {
            var filelist = AssetBundleLoader.FILE_LIST;
            ResInf oldInf = null;
            var savedPath = Path.Combine(AssetBundleLoader.streamingAssetsPath, filelist);
            if (File.Exists(savedPath)) {
                JSON.MakeInto(JSON.Load(File.ReadAllText(savedPath)), out oldInf);
            }

            if (oldInf == null) return;

            ResInf diffInf = new ResInf(resInf) {version = resInf.version, code = assetCode,};
            foreach (var kv in resInf.Assets) {
                EntryInf oldAsset;
                if (!oldInf.Assets.TryGetValue(kv.Key, out oldAsset) || oldAsset.md5 != kv.Value.md5) {
                    var assetInf = (AssetInf)kv.Value;
                    var patchEnt = LaunchBundles.Contains(kv.Key) ? new AssetInf {md5 = assetInf.md5, siz = assetInf.siz,} : new EntryInf {md5 = assetInf.md5,};

                    diffInf.Assets.Add(kv.Key, patchEnt);
                }
            }

            if (diffInf.Assets.Count > 0) {
                var patchPath = string.Format("{0}/Patch{1}({2})/Assets/{3}",
                    AssetBundleLoader.streamingAssetsPath, diffInf.version, assetCode,
#if UNITY_ANDROID
                RuntimePlatform.Android
#elif UNITY_IOS
                RuntimePlatform.IPhonePlayer
#elif UNITY_STANDALONE_OSX
                    RuntimePlatform.OSXPlayer
#elif UNITY_STANDALONE_WIN
                RuntimePlatform.WindowsPlayer
#endif
                );
                SystemTools.NeedDirectory(patchPath);
                File.WriteAllText(string.Format("{0}/{1}{2}", patchPath, filelist, assetCode), JSON.Dump(diffInf, true));
                foreach (var kv in diffInf.Assets) {
                    var assetPath = patchPath + "/" + kv.Key;
                    SystemTools.NeedDirectory(Path.GetDirectoryName(assetPath));
                    File.Copy(EditorStreamingAssetsPath + "/" + kv.Key, assetPath, true);
                }
            }
        }

        public static void GenFileListCSV()
        {
            var strbld = new StringBuilder("PATH,SIZE");
            strbld.AppendLine();
            int startIdx = EditorStreamingAssetsPath.Length;
            using (var itor = ForEachAssetBundle()) {
                while (itor.MoveNext()) {
                    var file = itor.Current;

                    long siz = file.Length;
                    var fullName = file.FullName;
                    if (startIdx < fullName.Length) {
                        string path = fullName.Substring(startIdx).Replace("\\", "/").Substring(1);
                        strbld.AppendFormat("{0},{1}\n", path, siz);
                    } else {
                        LogMgr.W("Invalid path: {0}", fullName);
                    }
                }
            }

            var savePath = AssetBundleLoader.streamingAssetsPath + "/filelist.csv";
            File.WriteAllText(savePath, strbld.ToString());
            EditorUtility.RevealInFinder(savePath);
        }

        public static void GenFileList()
        {
            var filelist = AssetBundleLoader.FILE_LIST;

            var ver = VersionMgr.GetAppVer();

            ResInf resInf = new ResInf {
                version = ver.version,
                code = assetCode,
                timeCreated = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                whoCreated = System.Environment.UserName
            };

            int startIdx = EditorStreamingAssetsPath.Length;
            using (var itor = ForEachAssetBundle()) {
                // 读取版本号
                while (itor.MoveNext()) {
                    var file = itor.Current;
                    var assetName = file.FullName.Substring(startIdx).Replace("\\", "/").Substring(1);
                    resInf.Assets.Add(assetName, new AssetInf {
                        siz = file.Length, md5 = CMD5.MD5File(file.FullName),
                    });
                }
            }

            ResInf majorInf = null;
            switch (assetMode) {
                case AssetMode.Full:
                    majorInf = GenFullPackageFileList(resInf);
                    break;
                case AssetMode.Split:
                    majorInf = GenSplitPackageFileList(resInf);
                    break;
                case AssetMode.Micro:
                    majorInf = GenMicroPackageFileList(resInf);
                    break;
            }

            // 保存lua脚本的md5
            File.WriteAllText(EditorStreamingAssetsPath + "/md5", resInf.Assets["lua/script"].md5);

            GenPatchAssets(resInf);

            string savedPath = EditorStreamingAssetsPath + "/" + filelist;
            var totalContent = JSON.Dump(resInf, true);
            File.WriteAllText(savedPath, totalContent);
            Log("Generate {0} success. => {1}", AssetBundleLoader.FILE_LIST, savedPath);

            var insideContent = majorInf != null ? JSON.Dump(majorInf, true) : totalContent;
            savedPath = StreamingAssetsPath + "/" + filelist;
            File.Copy(EditorStreamingAssetsPath + "/md5", StreamingAssetsPath + "/md5", true);
            File.WriteAllText(savedPath, insideContent);
            Log("Update {0} success. => {1}/{2}", filelist, StreamingAssetsPath, filelist);

            var editorFilelist = Path.Combine(AssetBundleLoader.bundleRootPath, AssetBundleLoader.FILE_LIST);
            if (File.Exists(editorFilelist)) File.Delete(editorFilelist);
        }

        public static void UpdateFileList()
        {
            GenFileList();
            AssetDatabase.Refresh();
        }

        public static void ClearStreamingAssets()
        {
            var path = m_StreamingAssetsPath;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
        }

        public static void ClearEditorStreamingAssets()
        {
            var path = AssetBundleLoader.streamingRootPath;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }

            path = AssetBundleLoader.CombinePath(
                AssetBundleLoader.streamingAssetsPath, AssetBundleLoader.DOWNLOAD_FOLDER);
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
        }

        public static void ClearEditorPersistentAssets()
        {
            var path = AssetBundleLoader.bundleRootPath;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
        }
    }
}
