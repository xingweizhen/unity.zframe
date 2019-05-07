using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace ZFrame.Asset
{
    public static class AssetBundleMenu
    {
        /// <summary>
        /// 定义AssetBundle命名规则
        /// </summary>
        public const string DIR_BUNDLE = "BUNDLE";
        public const string DIR_OBO = "OBO";
        public const string DIR_CATEGORY = "CATEGORY";

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

        private static string GetAssetsRoot(string path)
        {
            return string.Format("Assets/{0}/{1}", AssetPacker.DIR_ASSETS, path);
        }

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

        [MenuItem("Assets/资源/自动标志资源(AssetBundle Name)")]
        public static void AutoMarkAssetBundle()
        {
            // 独立资源
            var dirs = Directory.GetDirectories(GetAssetsRoot(DIR_OBO));
            foreach (var d in dirs) {
                var dName = Path.GetFileNameWithoutExtension(d);
                markSingleAssetName(d, dName, "*");
            }

            // 分类资源
            dirs = Directory.GetDirectories(GetAssetsRoot(DIR_CATEGORY));
            foreach (var d in dirs) {
                var dName = Path.GetFileNameWithoutExtension(d);
                foreach (var d2 in Directory.GetDirectories(d)) {
                    var d2Name = Path.GetFileNameWithoutExtension(d2);
                    var abName = string.Format("{0}/{1}", dName, d2Name).ToLower();
                    markPackedAssetName(d2, abName, "*");
                }
            }

            // 多资源包
            dirs = Directory.GetDirectories(GetAssetsRoot(DIR_BUNDLE));
            foreach (var d in dirs) {
                var dName = Path.GetFileNameWithoutExtension(d);
                var abName = string.Format("{0}", dName).ToLower();
                markPackedAssetName(d, abName, "*", SearchOption.AllDirectories);
            }

            // 场景依赖
            dirs = Directory.GetDirectories("Assets/Scenes");
            foreach (var d in dirs) {
                var subdirs = Directory.GetDirectories(d);
                var dname = Path.GetFileName(d);
                if (dname.OrdinalIgnoreCaseStartsWith("stage")) {
                    foreach (var sd in subdirs) {
                        var sdname = Path.GetFileName(sd).ToLower();
                        if (sdname == "prefabs" || sdname == "terrain_tx" || sdname == "textures") {
                            //markPackedAssetName(sd, "scenes/" + dname, "*");
                            var ai = AssetImporter.GetAtPath(sd);
                            if (ai != null) ai.assetBundleName = "scenes/" + dname;
                        }
                    }
                }
            }

            // 战斗场景
            markSingleAssetName("Assets/Scenes", "scenes", "*.unity", SearchOption.AllDirectories, (path) => {
                var dir = Path.GetFileName(Path.GetDirectoryName(path));
                return dir.StartsWith("stage_");
            });

            // FMOD资源
            markSingleAssetName(GetAssetsRoot("FMOD"), "fmod", "*");
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

                d.Delete(true);
                AssetPacker.Log("删除空的资源目录: {0}", abRoot);
            }

            AssetPacker.Log("共删除{0}个废弃的资源包", listDel.Count);
        }
    }
}
