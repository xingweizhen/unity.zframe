using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace ZFrame.Editors
{
    using Asset;
    using Settings;
    public static class AssetBundleMenu
    {
        // 资源目录结构
        // RefAssets
        // |- BUNDLE/                  # 目录下面内的资源打成一个包
        // |  |- aaa/                  -> aaa
        // |  +- bbb/                  -> bbb
        // |- CATEGORY                 # 目录下的每个目录作为一个组，组内的资源打成一个包
        // |  +- ccc/
        // |     |- cc1/               -> ccc/cc1
        // |     +- cc2/               -> ccc/cc2
        // +- OBO                      # 目录下面的资源每个独立打一个包
        //    |- eee/
        //    |  |- e1                 -> eee/e1
        //    |  +- e2                 -> eee/e2
        //    +- fff/
        //       +- f1                 -> fff/f1

        /// <summary>
        /// 目录下的资源以各自的名称独立命名
        /// </summary>
        private static void markSingleAssetName(
            string rootDir,
            string group,
            string pattern,
            SearchOption searchOption = SearchOption.TopDirectoryOnly,
            System.Func<string, bool> filter = null)
        {
            var files = Directory.GetFiles(rootDir, pattern, searchOption);
            foreach (var assetPath in files) {
                if (filter == null || filter.Invoke(assetPath)) {
                    var ai = AssetImporter.GetAtPath(assetPath);
                    if (ai) {
                        string assetName = Path.GetFileNameWithoutExtension(assetPath).Replace('.', '-');

                        assetName = assetName.Replace('~', '.');
                        string abName = string.Format("{0}/{1}", group, assetName).ToLower();
                        ai.assetBundleName = abName;
                        AssetPacker.Log("设置了资源名称: {0} -> {1}", ai.assetPath, abName);
                    }
                }
            }
        }

        /// <summary>
        /// 将目录下的资源以目录名命名
        /// </summary>
        private static void markPackedAssetName(
            string rootPath,
            string abName,
            string pattern,
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (!Directory.Exists(rootPath)) return;

            var files = Directory.GetFiles(rootPath, pattern, searchOption);
            abName = abName.Replace('~', '.');
            int count = 0;
            foreach (var assetPath in files) {
                var ai = AssetImporter.GetAtPath(assetPath);
                if (ai) {
                    ai.assetBundleName = abName;
                    count += 1;
                }
            }

            AssetPacker.Log("设置了资源名称: {0} -> {1}。共{2}个资源", rootPath, abName, count);
        }

        /// <summary>
        /// 将根目录下子目录的资源以子目录名命名。
        /// </summary>
        /// <param name="rootPath">根目录</param>
        /// <param name="pattern">筛选规则</param>
        /// <param name="group">组名</param>
        private static void markMultipleAssetName(string rootPath, string group, string pattern)
        {
            var dirs = Directory.GetDirectories(rootPath);
            foreach (var d in dirs) {
                var dName = Path.GetFileNameWithoutExtension(d);
                var abName = string.Format("{0}/{1}", group, dName).ToLower();
                markPackedAssetName(d, abName, pattern);
            }
        }

        private static void AutoSetAssetBundleName(string assetPath, string abName)
        {
            var ai = AssetImporter.GetAtPath(assetPath);
            abName = abName.ToLower();
            if (ai && ai.assetBundleName != abName) {
                if (string.IsNullOrEmpty(abName)) {
                    AssetPacker.Log("-移除了资源名称: {0} -> {1}", ai.assetPath, abName);
                } else {
                    AssetPacker.Log("+设置了资源名称: {0} -> {1}", ai.assetPath, abName);
                }
                ai.assetBundleName = abName;
            }
        }

        public static void AutoSetAssetBundleName(string assetPath)
        {
            var ext = Path.GetExtension(assetPath).ToLower();
            if (ext == "cs" || ext == "shader") return;

            var oboDirs = new HashSet<string>();
            var categoryDirs = new HashSet<string>();
            var bundleDirs = new HashSet<string>();
            var sceneDirs = new HashSet<string>();
            var ignoreDirs = new HashSet<string>();

            var guids = AssetDatabase.FindAssets("t:AssetBundleSettings");
            if (guids != null && guids.Length > 0) {
                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var frameSettings = AssetDatabase.LoadAssetAtPath<AssetBundleSettings>(path);
                    if (frameSettings) {
                        frameSettings.CollectPaths(bundleDirs, categoryDirs, oboDirs, sceneDirs, ignoreDirs);
                    }
                }
            }

            var stdAssetPath = assetPath.Replace('\\', '/');

            foreach (var path in ignoreDirs) {
                if (stdAssetPath.Contains(path)) return;
            }

            // BUNDLE/Launch/xxxx.prefab => launch
            foreach (var path in bundleDirs) {
                if (stdAssetPath.Contains(path)) {
                    var assetDir = Path.GetDirectoryName(stdAssetPath);
                    if (Path.GetDirectoryName(assetDir).EndsWith(path)) {
                        AutoSetAssetBundleName(assetPath, Path.GetFileName(assetDir));
                        return;
                    }
                    goto NO_NAME;
                }
            }

            // CAGETORY/FX/Orb/xxxx.prefab => fx/orb
            foreach (var path in categoryDirs) {
                if (stdAssetPath.Contains(path)) {
                    var assetDir = Path.GetDirectoryName(stdAssetPath);
                    var assetCategory = Path.GetDirectoryName(assetDir);
                    if (Path.GetDirectoryName(assetCategory).EndsWith(path)) {
                        AutoSetAssetBundleName(assetPath, Path.GetFileName(assetCategory) + "/" + Path.GetFileName(assetDir));
                        return;
                    }
                    goto NO_NAME;
                }
            }

            // OBO/Atlas/common.spriteatlas => atlas/common
            foreach (var path in oboDirs) {
                if (stdAssetPath.Contains(path)) {
                    var assetDir = Path.GetDirectoryName(stdAssetPath);
                    if (Path.GetDirectoryName(assetDir).EndsWith(path)) {
                        AutoSetAssetBundleName(assetPath, Path.GetFileName(assetDir) + "/" + Path.GetFileNameWithoutExtension(stdAssetPath));
                        return;
                    }
                    goto NO_NAME;
                }
            }

            // Scenes/stage_test/artwork/??/??.?? => scnens/stage_test
            // Scenes/stage_test/stage_test_1 => scnens/stage_test_1
            foreach (var path in sceneDirs) {
                var match = Regex.Match(stdAssetPath, path + @"/(stage_[A-Za-z0-9]+)/(stage_[A-Za-z0-9]+_[A-Za-z0-9]+).unity");
                if (match.Success) {
                    var sceneName = match.Groups[2].Value;
                    var sceneGroup = match.Groups[1].Value;
                    if (sceneName.StartsWith(sceneGroup)) {
                        AutoSetAssetBundleName(assetPath, "scenes/" + sceneName);
                        return;
                    }

                }
            }

            NO_NAME:
            AutoSetAssetBundleName(assetPath, string.Empty);
        }

        [MenuItem("Assets/资源/自动标志资源(AssetBundle Name)")]
        public static void AutoMarkAssetBundle()
        {
            var oboDirs = new HashSet<string>();
            var categoryDirs = new HashSet<string>();
            var bundleDirs = new HashSet<string>();
            var sceneDirs = new HashSet<string>();

            var guids = AssetDatabase.FindAssets("t:AssetBundleSettings");
            if (guids != null && guids.Length > 0) {
                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var frameSettings = AssetDatabase.LoadAssetAtPath<AssetBundleSettings>(path);
                    if (frameSettings) {
                        frameSettings.CollectPaths(bundleDirs, categoryDirs, oboDirs, sceneDirs);
                    }
                }
            }

            try {
                // 独立资源
                foreach (var path in oboDirs) {                    
                    var dirs = Directory.GetDirectories(path);
                    foreach (var d in dirs) {
                        var dName = Path.GetFileNameWithoutExtension(d);
                        markSingleAssetName(d, dName, "*");
                    }
                }
            } catch (System.Exception e) {
                Debug.LogWarning(e.Message);
            }

            try {
                // 分类资源
                foreach (var path in categoryDirs) {
                    var dirs = Directory.GetDirectories(path);
                    foreach (var d in dirs) {
                        var dName = Path.GetFileNameWithoutExtension(d);
                        foreach (var d2 in Directory.GetDirectories(d)) {
                            var d2Name = Path.GetFileNameWithoutExtension(d2);
                            var abName = string.Format("{0}/{1}", dName, d2Name).ToLower();
                            markPackedAssetName(d2, abName, "*");
                        }
                    }
                }
            } catch (System.Exception e) {
                Debug.LogWarning(e.Message);
            }

            try {
                // 多资源包
                foreach (var path in bundleDirs) {
                    var dirs = Directory.GetDirectories(path);
                    foreach (var d in dirs) {
                        var dName = Path.GetFileNameWithoutExtension(d);
                        var abName = string.Format("{0}", dName).ToLower();
                        markPackedAssetName(d, abName, "*", SearchOption.AllDirectories);
                    }
                }
            } catch (System.Exception e) {
                Debug.LogWarning(e.Message);
            }

            try {
                // 场景依赖
                foreach (var path in sceneDirs) {
                    var dirs = Directory.GetDirectories(path);
                    foreach (var d in dirs) {
                        var subdirs = Directory.GetDirectories(d);
                        var dname = Path.GetFileName(d);
                        if (dname.OrdinalIgnoreCaseStartsWith("stage")) {
                            foreach (var sd in subdirs) {
                                var sdname = Path.GetFileName(sd).ToLower();
                                if (sdname == "artwork") {
                                    var ai = AssetImporter.GetAtPath(sd);
                                    if (ai != null) ai.assetBundleName = "scenes/" + dname;
                                }
                            }
                        }
                    }
                }
            } catch (System.Exception e) {
                Debug.LogWarning(e.Message);
            }

            try {
                // 战斗场景
                foreach (var path in sceneDirs) {
                    markSingleAssetName(path, "scenes", "*.unity", SearchOption.AllDirectories, (p) => {
                        var dir = Path.GetFileName(Path.GetDirectoryName(p));
                        return dir.StartsWith("stage_");
                    });
                }
            } catch (System.Exception e) {
                Debug.LogWarning(e.Message);
            }
        }

        [MenuItem("Assets/资源/删除废弃的资源包")]
        public static void RemoveUnusedAssest()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            if (!Directory.Exists(AssetBundleLoader.streamingRootPath)) return;

            var list = new List<string>(AssetDatabase.GetAllAssetBundleNames());
            DirectoryInfo dir = new DirectoryInfo(AssetBundleLoader.streamingRootPath);
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
            var index = AssetBundleLoader.streamingRootPath.Length + 1;
            var listDel = new List<string>();
            foreach (var f in files) {
                if (f.Name[0] == '.' ||
                    f.Name == AssetBundleLoader.FILE_LIST ||
                    f.Name == AssetBundleLoader.ASSETBUNDLE_FOLDER ||
                    f.Extension == ".manifest" ||
                    f.Name == "md5") continue;

                var abName = f.FullName.Substring(index).Replace('\\', '/');
                if (list.Contains(abName)) continue;

                f.Delete();
                listDel.Add(f.FullName);

                LogMgr.D("删除废弃的资源包: {0}", abName);
            }

            foreach (var path in listDel) File.Delete(path + ".manifest");

            // Remove empty directories
            var dirList = new List<string>();
            for (int i = 0; i < list.Count; ++i) {
                string root = Path.GetDirectoryName(list[i]);
                for (; !string.IsNullOrEmpty(root); root = Path.GetDirectoryName(root)) {
                    if (!dirList.Contains(root)) {
                        dirList.Add(root);
                    }
                }
            }

            var subs = dir.GetDirectories("*", SearchOption.AllDirectories);
            foreach (var d in subs) {
                var abRoot = d.FullName.Substring(index).Replace('\\', '/');
                if (dirList.Contains(abRoot)) continue;

                if (d.Exists) d.Delete(true);
                AssetPacker.Log("删除空的资源目录: {0}", abRoot);
            }

            AssetPacker.Log("共删除{0}个废弃的资源包", listDel.Count);
        }

        [MenuItem("Assets/资源/刷新shader集")]
        public static void CollectShaders()
        {
            foreach (var path in AssetDatabase.GetAssetPathsFromAssetBundle("shaders")) {
                AssetImporter.GetAtPath(path).assetBundleName = string.Empty;
            }

            var assetPaths = new List<string>();
            foreach (var abName in AssetDatabase.GetAllAssetBundleNames())
                assetPaths.AddRange(AssetDatabase.GetAssetPathsFromAssetBundle(abName));

            foreach (var path in AssetDatabase.GetDependencies(assetPaths.ToArray())) {
                if (path.ToLower().EndsWith(".shader")) {
                    AssetImporter.GetAtPath(path).assetBundleName = "shaders";
                }
            }
            AssetDatabase.Refresh();
        }
    }
}
