using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZFrame.UGUI;

namespace ZFrame.Editors
{
    using Lua;
    
    /// <summary>
    /// 自动生成Lua脚本处理UI表现和逻辑
    /// 1. 每次只能选择一个UI的预设
    /// 2. 预设必须挂载一个<LuaComponent>组件
    /// 3. LuaComponent组件上定义好目标lua脚本和所需调用的函数
    /// 4. UI构建规则
    ///     以"_"结尾的对象会被忽略
    ///     前缀      说明
    ///     lb      该对象挂载有UILable组件  
    ///     sp      该对象挂载有UISprite组件
    ///     btn     该对象挂载有UIButton组件
    ///     tgl     该对象挂载有UIToggle组件
    ///     bar     该对象挂载有UISlider或UIProgress组件
    ///     ---     挂载在以上对象下的其他对象会被忽略
    ///     ent     只能挂载在Grp下面, 该对象下只能挂载以上基本对象
    ///     Grp     该对象下会必须挂载一个ent前缀，和以上其他对象
    ///     Sub     该对象下可以挂载所有对象，包括Sub自己
    ///     elm     只能获取到该节点的GameObject
    /// </summary>
    public class LuaUIGenerator : EditorWindow
    {
        private struct EventSet
        {
            public string path;
            public IEventSender sender;

            public EventSet(string path, IEventSender sender)
            {
                this.path = path;
                this.sender = sender;
            }
        }

        [MenuItem("ZFrame/UI控件/全屏窗口 &#w")]
        [MenuItem("GameObject/ZFrame UI/全屏窗口", priority = 0)]
        private static void CreateFullScreenWindow()
        {
            var rect = UGUICreateMenu.CreatUIBase(null);
            rect.name = "WNDFullScreen";
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            rect.gameObject.AddComponent(typeof(LuaComponent));
            Selection.activeTransform = rect;
        }

        [MenuItem("ZFrame/UI控件/弹出窗口 &#e")]
        [MenuItem("GameObject/ZFrame UI/弹出窗口", priority = 0)]
        private static void CreateSubScreenWindow()
        {
            var rect = UGUICreateMenu.CreatUIBase(null);
            var spSub = UGUICreateMenu.CreateUIElm<UISprite>(rect.gameObject);
            spSub.SetSprite(UGUICreateMenu.kStandardSpritePath);
            spSub.name = "SubMain";
            spSub.rectTransform.sizeDelta = new Vector2(
                UGUITools.settings.defRes.x * 2 / 3,
                UGUITools.settings.defRes.y * 2 / 3);

            rect.name = "WNDSubScreen";
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            rect.gameObject.AddComponent(typeof(LuaComponent));
            Selection.activeTransform = rect;
        }

        public static void ShowWindow()
        {
            EditorWindow edtWnd = GetWindow(typeof(LuaUIGenerator));
            edtWnd.minSize = new Vector2(800, 650);
            edtWnd.maxSize = edtWnd.minSize;
        }

        const string AUTO_DEFINE_BEGIN = "--!*[开始]自动生成函数*--";
        const string AUTO_DEFINE_END = "--!*[结束]自动生成函数*--";
        const string AUTO_REGIST = "--!*[结束]自动生成代码*--";

        const string INIT_VIEW = "init_view";
        const string INIT_LOGIC = "init_logic";

        private static bool IsDefine(string line, string define)
        {
            return line.Trim() == define;
        }

        private static Dictionary<int, string> s_ScrollViewArgs = new Dictionary<int, string>() {
            {(int)TriggerType.None, "scr, delta" },
            {(int)TriggerType.BeginDrag, "scr, evtData" },
            {(int)TriggerType.Drag, "scr, evtData" },
            {(int)TriggerType.EndDrag, "scr, evtData" },
            {UIScrollView.TGR_OVERLAP, "trans, overlap" },
        };

        private static Dictionary<System.Type, object> s_DArgs = new Dictionary<System.Type, object>() {
            {typeof(UIButton), "btn"},
            {typeof(UIToggle), "tgl, value"},
            {typeof(UIDropdown), "drp, index"},
            {typeof(UIEventTrigger), "evt, data"},
#if USING_TMP
        { typeof(UIInputField), "inp, text" },
#endif
            {typeof(UIInput), "inp, text"},
            {typeof(ILoopLayout), "go, i"},
            {typeof(UISlider), "bar, value"},
            {typeof(UIProgress), "bar, value"},
            {typeof(UIScrollView), s_ScrollViewArgs},
        };

        private static Dictionary<TriggerType, string> s_DActions = new Dictionary<TriggerType, string>() {
            {TriggerType.PointerEnter, "ptrin"},
            {TriggerType.PointerExit, "ptrout"},
            {TriggerType.PointerDown, "ptrdown"},
            {TriggerType.PointerUp, "ptrup"},
            {TriggerType.PointerClick, "click"},
            {TriggerType.BeginDrag, "begindrag"},
            {TriggerType.Drag, "drag"},
            {TriggerType.EndDrag, "enddrag"},
            {TriggerType.Drop, "drop"},
            {TriggerType.Select, "select"},
            {TriggerType.Deselect, "deselect"},
            {TriggerType.Submit, "submit"},
            {TriggerType.Longpress, "pressed"},
            {TriggerType.Unselect, "unselect"},
        };

        private static string GetArgs(IEventSender sender, TriggerType type)
        {
            foreach (var kv in s_DArgs) {
                if (kv.Key.IsInstanceOfType(sender)) {
                    if (kv.Value is string)
                        return (string)kv.Value;

                    var iType = (int)type;
                    foreach (var _kv in (Dictionary<int, string>)kv.Value) {
                        if (iType == _kv.Key) return _kv.Value;
                    }
                    break;
                }
            }

            return "sender, param";
        }

        // 根据控件类型得到回调函数名
        string genFuncName(EventData evt, string path)
        {
            path = path.Replace('/', '_').ToLower();

            string action = null;
            if (s_DActions.TryGetValue(evt.type, out action)) {
                if (evt.name == UIEvent.Auto) {
                    if (string.IsNullOrEmpty(path)) {
                        return string.Format("on_{0}", action);
                    } else {
                        return string.Format("on_{0}_{1}", path, action);
                    }
                }
            }

            return evt.name == UIEvent.Auto ? null : evt.param;
        }

        string opTips = "[---]";
        Vector2 scroll = Vector2.zero;
        string logicFile = "";
        string scriptLogic = "";
        private List<string> scriptLabels = new List<string>();
        LuaComponent selected;

        bool flagGenCode;

        //
        Dictionary<string, Component> dictRef = new Dictionary<string, Component>();
        private List<EventSet> m_EventSets = new List<EventSet>();

        // *_logic.lua的结构
        string codeDefined = null;
        string codeInited = null;
        Dictionary<string, string> dictFunc = new Dictionary<string, string>();

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(2, 2, position.width - 4, position.height - 4));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生成脚本")) {
                generateWithSelected();
            }

            if (GUILayout.Button("写入脚本")) {
                saveLogic();
            }

            if (GUILayout.Button("生成并写入脚本")) {
                generateWithSelected();
                saveLogic();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.color = Color.red;
            GUILayout.Label(opTips);
            GUI.color = Color.white;

            GUILayout.Label(logicFile, EditorStyles.boldLabel);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.black);

            scroll = GUILayout.BeginScrollView(scroll);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            for (var i = 0; i < scriptLabels.Count; ++i) {
                GUILayout.Label(scriptLabels[i]);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        void ShowMessage(string str)
        {
            opTips = str;
            //EditorUtility.DisplayDialog("提示", str, "确定");
        }

        // 为选定的预设生成Lua脚本
        private void generateWithSelected()
        {
            GameObject[] goes = Selection.gameObjects;
            if (goes != null && goes.Length == 1) {
                selected = goes[0].GetComponent<LuaComponent>();
                flagGenCode = true;
                if (selected != null) {
                    // 清空结构
                    codeDefined = null;
                    codeInited = null;
                    dictRef.Clear();
                    m_EventSets.Clear();
                    dictFunc.Clear();
                    // 生成文件名
                    if (string.IsNullOrEmpty(selected.luaScript)) {
                        ShowMessage("脚本名称为空！");
                        return;
                    }

                    string fileName = selected.luaScript;
                    if (!fileName.Contains("/")) {
                        if (Directory.Exists(ChunkAPI.GetFilePath("ui/" + fileName))) {
                            // 扩展名为空，名称是模块名
                            selected.luaScript = string.Format("ui/{0}/lc_{1}", fileName, selected.name.ToLower());
                        } else {
                            ShowMessage("不存在的UI模块: " + fileName);
                            return;
                        }
                    }

                    logicFile = selected.luaScript;
                    if (!logicFile.EndsWith(".lua")) logicFile += ".lua";

                    parseLogic(logicFile);
                    GenerateLogic();
                } else {
                    ShowMessage("预设体上需要挂载<LuaComponent>组件");
                }
            } else {
                ShowMessage("只能选择一个预设体来生成脚本");
            }
        }

        // 解析已有的UI Logic脚本
        void parseLogic(string path)
        {
            var filePath = ChunkAPI.GetFilePath(path);
            if (!File.Exists(filePath)) {
                codeDefined = null;
                codeInited = null;
                dictFunc.Clear();
                return;
            }

            // 解析
            string text = File.ReadAllText(filePath);
            // 注释：文件名
            text = text.Substring(text.IndexOf(".lua") + 5);

            var beginIdx = text.IndexOf(AUTO_DEFINE_BEGIN);
            int codeStart = beginIdx - 1;
            if (codeStart > 0) {
                codeDefined = text.Substring(0, codeStart);
            } else {
                codeDefined = null;
            }

            codeStart = beginIdx;
            if (codeStart >= 0) {
                text = text.Substring(codeStart);
            } else {
                return;
            }

            string funcName = null;
            string funcDefine = null;
            using (var reader = new StringReader(text)) {
                for (;;) {
                    var line = reader.ReadLine();
                    if (line == null) break;

                    if (IsDefine(line, AUTO_DEFINE_END)) continue;

                    if (IsDefine(line, AUTO_REGIST)) {
                        if (funcName == INIT_VIEW) {
                            // 开始记录codeInited
                            codeInited = "";
                        }

                        continue;
                    } else if (line.Contains("return self")) {
                        if (funcName != null) {
                            dictFunc.Add(funcName, funcDefine.Substring(0, funcDefine.Length - 1));
                        }

                        break;
                    }

                    // function inside function will be ignore...
                    string[] segs = line.Split(new char[] {' ', '\t', '\n'}, System.StringSplitOptions.None);
                    if (segs != null && segs.Length > 0 && segs[0] == "function") {
                        if (funcName != null) {
                            if (funcName != INIT_VIEW) {
                                try {
                                    dictFunc.Add(funcName, funcDefine.Substring(0, funcDefine.Length - 1));
                                } catch (System.Exception e) {
                                    Debug.LogError(e.Message + ":" + funcName);
                                }
                            } else {
                                int endIndex = codeInited.LastIndexOf("end");
                                codeInited = codeInited.Substring(0, endIndex);
                            }
                        }

                        // 取函数名, 记录函数
                        funcName = segs[1].Substring(0, segs[1].IndexOf('(')).Trim();
                        funcDefine = '\n' + line + '\n';
                    } else {
                        if (funcName != null) {
                            if (funcName == INIT_VIEW && codeInited != null) {
                                codeInited += line + '\n';
                            }

                            funcDefine += line + '\n';
                        }
                    }
                }
            }
        }

        // 生成UI Logic脚本 
        private void GenerateLogic()
        {
            StoreGrpEnts(selected.transform, selected.transform);

            StoreSelectable();

            // 文件头
            var strbld = new StringBuilder();
            genFileHeader(strbld);

            // 独立的Triggler
            GenSingleTrigger(strbld);

            // 自定义的事件处理函数
            List<string> listOnMethods = new List<string>();
            foreach (KeyValuePair<string, string> kv in dictFunc) {
                if (kv.Key == INIT_VIEW || kv.Key == INIT_LOGIC) continue;
                if (selected.LocalMethods.Contains(kv.Key)) continue;

                strbld.Append(kv.Value);
                listOnMethods.Add(kv.Key);
            }

            foreach (string method in listOnMethods) {
                dictFunc.Remove(method);
            }

            listOnMethods.Clear();

            // 界面显示初始化
            functionBegin(strbld, false, INIT_VIEW);

            // Grp生成方法
            MakeGroupFunc(strbld);

            normal(strbld, AUTO_REGIST);
            strbld.Append(codeInited);
            functionEnd(strbld);

            // 界面逻辑初始化
            generateFunc(strbld, INIT_LOGIC);

            // 其他函数
            foreach (KeyValuePair<string, string> kv in dictFunc) {
                strbld.Append(kv.Value);
            }

            // 预设函数
            foreach (var method in selected.LocalMethods) {
                if (!dictFunc.ContainsKey(method)) {
                    generateFunc(strbld, method);
                }
            }

            // return表
            normal(strbld, "\nreturn self\n");

            scriptLogic = strbld.ToString();

            scriptLabels.Clear();
            StringBuilder builder = new StringBuilder();
            const int maxLine = 100;
            int nLine = 0;
            using (var reader = new StringReader(scriptLogic)) {
                for (; ; ) {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    builder.AppendFormat("{0:D4}| {1}", nLine, line);
                    nLine++;

                    if (nLine % maxLine == 0) {
                        scriptLabels.Add(builder.ToString());
                        builder.Remove(0, builder.Length);
                    } else {
                        builder.AppendLine();
                    }
                }
            }
            if (builder.Length > 0) scriptLabels.Add(builder.ToString());
        }

        /// <summary>
        /// 记录所有Selectable
        /// </summary>
        private void StoreSelectable()
        {
            var coms = selected.GetComponentsInChildren(typeof(IEventSender), true);
            foreach (var com in coms) {
                var sender = (IEventSender)com;
                bool hasSendEvent = false;
                foreach (var evt in sender) {
                    if (evt.name == UIEvent.Auto || evt.name == UIEvent.Send) {
                        hasSendEvent = true;
                        break;
                    }
                }

                if (!hasSendEvent) continue;

                var path = com.GetHierarchy(selected.transform);
                if (!string.IsNullOrEmpty(path)) {
                    var c = path[path.Length - 1];
                    if (c == '_' || c == '=') continue;
                    if (path.Contains("Elm")) continue;
                }

                //dictRef.AddOrReplace(path, com);
                m_EventSets.Add(new EventSet(path, sender));
            }
        }

        void genFileTmpl(StringBuilder strbld)
        {
            var data = System.DateTime.Now;
            strbld.AppendFormat("-- @author  {0}\n", System.Environment.UserName);
            strbld.AppendFormat("-- @date    {0}\n", data.ToString("yyyy-MM-dd HH:mm:ss"));
            strbld.AppendFormat("-- @desc    {0}\n", selected.name);
            strbld.AppendLine("--\n");
        }

        // 生成文件头
        void genFileHeader(StringBuilder strbld)
        {
            // 注释：文件名
            strbld.AppendFormat("--\n-- @file    {0}\n", logicFile);

            // 已有的本地变量定
            if (codeDefined != null) {
                codeDefined = codeDefined.TrimStart();
                if (!codeDefined.StartsWith("-- @author")) {
                    genFileTmpl(strbld);
                }

                normal(strbld, codeDefined);
            } else {
                genFileTmpl(strbld);

                normal(strbld, "local self = ui.new()");
                //normal(strbld, "setfenv(1, self)");
                // Lua5.2以后
                normal(strbld, "local _ENV = self");
            }
        }

        /// <summary>
        ///  独立的触发器，对于ent中的触发器不生成，仅记录下来
        /// </summary>
        void GenSingleTrigger(StringBuilder strbld)
        {
            normal(strbld, AUTO_DEFINE_BEGIN);

            var listFuncs = new List<string>();
            foreach (var set in m_EventSets) {
                var path = set.path;
                var Events = set.sender;
                if (Events != null) {
                    foreach (var Event in Events) {
                        var funcName = genFuncName(Event, path);
                        if (!string.IsNullOrEmpty(funcName)) {
                            if (listFuncs.Contains(funcName)) continue;
                            listFuncs.Add(funcName);
                            Event.param = funcName;
                            var args = GetArgs(Events, Event.type);
                            if (string.IsNullOrEmpty(args)) {
                                generateFunc(strbld, funcName);
                            } else {
                                generateFunc(strbld, funcName, args);
                            }
                        }
                    }
                }
            }

            normal(strbld, AUTO_DEFINE_END);
        }

        /// <summary>
        /// 产生Group生产的代码
        /// </summary>
        private void MakeGroupFunc(StringBuilder strbld)
        {
            foreach (KeyValuePair<string, Component> kv in dictRef) {
                string path = kv.Key;
                string entName = Path.GetFileNameWithoutExtension(path);
                if (!entName.StartsWith("ent")) continue;

                string grpName = SystemTools.GetDirPath(path);
                string pointName = grpName.Replace('/', '.');
                normal(strbld, string.Format("ui.group(Ref.{0})", pointName));
            }
        }

        // 生成回调函数定义
        void generateFunc(StringBuilder strbld, string funcName, params string[] args)
        {
            string content;
            if (dictFunc.TryGetValue(funcName, out content)) {
                strbld.Append(content);
                dictFunc.Remove(funcName);
            } else {
                functionBegin(strbld, false, funcName, args);
                normal(strbld, "");
                functionEnd(strbld);
            }
        }

        private bool IsNewCreateAsset(GameObject go)
        {
#if UNITY_2018_3_OR_NEWER
            var assetType = PrefabUtility.GetPrefabAssetType(go);
            return assetType == PrefabAssetType.NotAPrefab;
#else
            return PrefabUtility.GetPrefabType(go) == PrefabType.None;
#endif
        }

        private void CreatePrefabAsset(GameObject go, string path)
        {
#if UNITY_2018_3_OR_NEWER
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
#else
            PrefabUtility.CreatePrefab(path, go, ReplacePrefabOptions.ConnectToPrefab);
#endif
        }

        private void UpdatePrefabAsset(GameObject go)
        {
#if UNITY_2018_3_OR_NEWER
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
            var path = AssetDatabase.GetAssetPath(prefab);
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
#else
#if UNITY_2018_2_OR_NEWER
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
#else
            var prefab = PrefabUtility.GetPrefabParent(go);
#endif
            PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
#endif
        }

        void saveLogic()
        {
            if (!string.IsNullOrEmpty(scriptLogic)) {
                string path = ChunkAPI.GetFilePath(logicFile);
                File.WriteAllText(path, scriptLogic);
                ShowMessage(string.Format("写入{0}成功！", path));

                var selectedObj = selected.gameObject;
               
                if (IsNewCreateAsset(selectedObj)) {
                    var ues = Settings.UGUIEditorSettings.Get();
                    if (ues == null) {
                        Debug.LogError("未找到UGUI编辑设置文件：ZFrame->设置选项...->UGUI编辑设置");
                        return;
                    }
                    if (!Directory.Exists(ues.uiFolder)) {
                        Debug.LogErrorFormat("UI预设根目录{0}不存在：ZFrame->设置选项...->UGUI编辑设置", ues.uiFolder);
                        return;
                    }
                    var prefabPath = string.Format("{0}/{1}.prefab", ues.uiFolder, selectedObj.name);
                    CreatePrefabAsset(selectedObj, prefabPath);
                } else {
                    UpdatePrefabAsset(selectedObj);
                }
            } else {
                ShowMessage(logicFile + "脚本为空!");
            }
        }

        /// <summary>
        /// 解析窗口结构，记录所有组
        /// </summary>
        private void StoreGrpEnts(Transform root, Transform curr)
        {
            string findPreffix = curr.GetHierarchy(root);

            for (int i = 0; i < curr.childCount; ++i) {
                Transform trans = curr.GetChild(i);
                if (!trans.gameObject.activeSelf) continue;

                string sName = trans.name;
                if (sName.EndsWith('_')) continue;

                if (sName.StartsWith("Sub")) {
                    StoreGrpEnts(root, trans);
                } else if (sName.StartsWith("Grp")) {
                    StoreGrpEnts(root, trans);
                } else if (sName.StartsWith("ent")) {
                    dictRef.Add(findPreffix + "/" + sName, trans);
                }
            }
        }

        #region 以下为Lua代码组装

        int step = 0;
        int nt = 0;

        StringBuilder appendTabs(StringBuilder strbld)
        {
            if (!flagGenCode) return strbld;

            for (int i = 0; i < step; ++i) {
                strbld.Append("\t");
            }

            return strbld;
        }

        void functionBegin(StringBuilder strbld, bool blocal, string funcName, params string[] Params)
        {
            if (!flagGenCode) return;

            strbld.Append('\n');
            appendTabs(strbld);
            strbld.AppendFormat("{0}function {1}", blocal ? "local " : "", funcName);
            if (Params != null && Params.Length > 0) {
                strbld.Append('(');
                for (int i = 0; i < Params.Length; ++i) {
                    strbld.Append(Params[i]);
                    if (i < Params.Length - 1) {
                        strbld.Append(", ");
                    } else if (i == Params.Length - 1) {
                        strbld.Append(")\n");
                    }
                }
            } else {
                strbld.Append("()\n");
            }

            step += 1;
        }

        void functionEnd(StringBuilder strbld)
        {
            if (!flagGenCode) return;

            step -= 1;
            appendTabs(strbld);
            strbld.Append("end\n");
        }

        void ifBegin(StringBuilder strbld, string logic)
        {
            if (!flagGenCode) return;

            appendTabs(strbld);
            strbld.AppendFormat("if {0} then\n", logic);
            step += 1;
        }

        void ifEnd(StringBuilder strbld)
        {
            if (!flagGenCode) return;

            step -= 1;
            appendTabs(strbld);
            strbld.Append("end\n");
        }

        void forBegin(StringBuilder strbld, string var, string from, string to)
        {
            if (!flagGenCode) return;

            appendTabs(strbld);
            strbld.AppendFormat("for {0} = {1}, {2} do\n", var, from, to);
            step += 1;
        }

        void forEnd(StringBuilder strbld)
        {
            if (!flagGenCode) return;

            step -= 1;
            appendTabs(strbld);
            strbld.Append("end\n");
        }

        void tableBegin(StringBuilder strbld, string tableName)
        {
            if (!flagGenCode) return;

            appendTabs(strbld);
            if (string.IsNullOrEmpty(tableName)) {
                strbld.Append("{\n");
            } else {
                strbld.AppendFormat("{0} = {1}\n", tableName, '{');
            }

            step += 1;
            nt += 1;
        }

        void tableEnd(StringBuilder strbld)
        {
            if (!flagGenCode) return;

            step -= 1;
            nt -= 1;
            appendTabs(strbld).Append('}');
            if (nt > 0) {
                strbld.Append(',');
            }

            strbld.Append("\n");
        }

        void normal(StringBuilder strbld, string logic)
        {
            if (!flagGenCode) return;

            if (logic != null) {
                appendTabs(strbld).Append(logic);
                if (nt > 0) {
                    strbld.Append(',');
                }

                strbld.Append("\n");
            }
        }

        #endregion
    }
}