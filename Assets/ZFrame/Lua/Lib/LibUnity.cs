using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif
using ILuaState = System.IntPtr;
using UnityEngine.SceneManagement;

namespace ZFrame.Lua
{
    using Asset;
    public static class LibUnity
    {
        public const string LIB_NAME = "libunity.cs";

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int OpenLib(ILuaState lua)
        {
            lua.NewTable();
            lua.SetDict("AppQuit", AppQuit);

            // Debug
            lua.SetDict("Log", Log);
            lua.SetDict("BeginSample", BeginSample);
            lua.SetDict("EndSample", EndSample);

            // GameObject
            lua.SetDict("IsObject", IsObject);
            lua.SetDict("IsActive", IsActive);
            lua.SetDict("IsEnable", IsEnable);
            lua.SetDict("Recycle", Recycle);
            lua.SetDict("Destroy", Destroy);
            lua.SetDict("Find", Find);
            lua.SetDict("FindByName", FindByName);
            lua.SetDict("AddChild", AddChild);
            lua.SetDict("AddCom", AddCom);
            lua.SetDict("NewChild", NewChild);
            lua.SetDict("SetLayer", SetLayer);
            lua.SetDict("SendMessage", SendMessage);
            lua.SetDict("SetActive", SetActive);
            lua.SetDict("ReActive", ReActive);
            lua.SetDict("SetEnable", SetEnable);
            lua.SetDict("SetParent", SetParent);
            lua.SetDict("SetSibling", SetSibling);
            lua.SetDict("SetPos", SetPos);
            lua.SetDict("GetPos", GetPos);
            lua.SetDict("SetEuler", SetEuler);
            lua.SetDict("GetEuler", GetEuler);
            lua.SetDict("SetScale", SetScale);
            lua.SetDict("GetScale", GetScale);
            lua.SetDict("LookAt", LookAt);
            lua.SetDict("CameraForLayer", CameraForLayer);
            lua.SetDict("FaceCamera", FaceCamera);

            // Rendering
            lua.SetDict("SetMaterials", SetMaterials);
            lua.SetDict("ClsMaterials", ClsMaterials);

            lua.SetDict("AddCullingMask", AddCullingMask);
            lua.SetDict("DelCullingMask", DelCullingMask);
            lua.SetDict("HasCullingMask", HasCullingMask);

            lua.SetDict("InitMaterialProperty", InitMaterialProperty);
            lua.SetDict("KeepMaterialProperty", KeepMaterialProperty);
            lua.SetDict("SetMaterialProperty", SetMaterialProperty);
            //rendertexture
            lua.SetDict("GetRenderTexture", GetRenderTexture);
            lua.SetDict("ReleaseRenderTexture", ReleaseRenderTexture);


            // Audio
            lua.SetDict("PlayAudio", PlayAudio);
            lua.SetDict("StopAudio", StopAudio);
            lua.SetDict("SetAudioParams", SetAudioParams);
            lua.SetDict("GetAudioVolume", GetAudioVolume);
            lua.SetDict("SetAudioVolume", SetAudioVolume);
            lua.SetDict("GetAudioMute", GetAudioMute);
            lua.SetDict("SetAudioMute", SetAudioMute);
            lua.SetDict("GetAudioPause", GetAudioPause);
            lua.SetDict("SetAudioPause", SetAudioPause);

            // Async Methods
            lua.SetDict("Invoke", Invoke);
            lua.SetDict("InvokeRepeating", InvokeRepeating);
            lua.SetDict("CancelInvoke", CancelInvoke);
            lua.SetDict("StartCoroutine", StartCoroutine);
            lua.SetDict("StopAllCoroutine", StopAllCoroutine);

            // Ray
            lua.SetDict("GetScreenCoord", GetScreenCoord);
            lua.SetDict("Raycast", Raycast);

            // Scene
            lua.SetDict("IsLevelLoaded", IsLevelLoaded);
            lua.SetDict("LoadLevel", LoadLevel);
            lua.SetDict("ActiveLevel", ActiveLevel);
            lua.SetDict("UnloadLevel", UnloadLevel);
            lua.SetDict("NewLevel", NewLevel);

            lua.SetDict("GC", GC);
            return 1;
        }

        public static GameObject FindGO(Transform parent, string path)
        {
            if (string.IsNullOrEmpty(path)) {
                return parent ? parent.gameObject : null;
            } else {
                GameObject ret = null;
                if (parent == null) {
                    ret = GoTools.Seek(path);
                } else {
                    var t = parent.Find(path);
                    ret = t ? t.gameObject : null;
                }

                return ret;
            }
        }

        public static GameObject FindGO(Transform parent, int sibling)
        {
            var child = parent && sibling < parent.childCount ? parent.GetChild(sibling) : null;
            return child ? child.gameObject : null;
        }

        /// <summary>
        /// ======= 导出的接口 =======
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int AppQuit(ILuaState lua)
        {
            LogMgr.I("游戏退出...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Log(ILuaState lua)
        {
            var traceLevel = lua.OptInteger(1, -1);
            var logLevel = (LogLevel)lua.ToEnumValue(2, LogMgr.logLevel);
            if (LogMgr.logLevel < logLevel) return 0;

            var logStr = lua.ToCSFormatString(3);
            if (traceLevel < 0) {
                LogMgr.Log(logLevel, "{0}", logStr);
            } else {
                var currLine = lua.DebugCurrentLine(2 + traceLevel);
                LogMgr.Log(logLevel, "{0}", currLine + logStr);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int BeginSample(ILuaState lua)
        {
            var sampleName = lua.ToString(1);
            UnityEngine.Profiling.Profiler.BeginSample(sampleName);
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int EndSample(ILuaState lua)
        {
            UnityEngine.Profiling.Profiler.EndSample();
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int IsObject(ILuaState lua)
        {
            Object o = lua.ToUnityObject(1);
            lua.PushBoolean(o != null);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int IsActive(ILuaState lua)
        {
            GameObject go = lua.ToGameObject(1);
            bool activeHierachy = lua.OptBoolean(2, false);
            bool viewHierachy = lua.OptBoolean(3, false);
            var active = go != null && (activeHierachy ? go.activeInHierarchy : go.activeSelf);
            if (active && viewHierachy) {
                if (activeHierachy) {
                    var trans = go.transform;
                    while (trans != null) {
                        if ((trans.gameObject.hideFlags & HideFlags.HideInHierarchy) != 0) {
                            active = false;
                            break;
                        }

                        trans = trans.parent;
                    }
                } else {
                    active = (go.hideFlags & HideFlags.HideInHierarchy) == 0;
                }
            }

            lua.PushBoolean(active);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int IsEnable(ILuaState lua)
        {
            GameObject go = lua.ToGameObject(1);
            if (go != null && go.activeInHierarchy) {
                string comName = lua.ToString(2);
                var com = go.GetComponent(comName);
                var behaviour = com as Behaviour;
                if (behaviour) {
                    lua.PushBoolean(behaviour.enabled);
                } else {
                    var cld = com as Collider;
                    lua.PushBoolean(cld && cld.enabled);
                }
            } else {
                lua.PushBoolean(false);
            }

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Recycle(ILuaState lua)
        {
            GameObject go = lua.ToGameObject(1);
            float delay = (float)lua.OptNumber(2, 0f);
            if (go != null) {
                ObjectPoolManager.DestroyPooled(go, delay);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Destroy(ILuaState lua)
        {
            Object obj = lua.ToUnityObject(1);
            float delay = (float)lua.OptNumber(2, 0f);
            if (obj != null) {
                if (delay > 0) {
                    Object.Destroy(obj, delay);
                } else {
                    Object.Destroy(obj);
                }

                if (lua.Type(1) == LuaTypes.LUA_TUSERDATA) {
                    MetaMethods.GC(lua);
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Find(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            string comName = lua.OptString(3, null);

            if (lua.IsNumber(2)) {
                var sibling = lua.ToInteger(2);
                if (string.IsNullOrEmpty(comName)) {
                    lua.PushX(FindGO(trans, sibling));
                } else {
                    Component com = null;
                    var ret = FindGO(trans, sibling);
                    if (ret) com = ret.GetComponent(comName);
                    lua.PushX(com);
                }
            } else {
                string path = lua.OptString(2, null);

                if (string.IsNullOrEmpty(comName)) {
                    lua.PushX(FindGO(trans, path));
                } else {
                    Component com = null;
                    if (path == "..") {
                        // 往上查找
                        while (trans && !com) {
                            com = trans.GetComponent(comName);
                            trans = trans.parent;
                        }
                    } else {
                        var ret = FindGO(trans, path);
                        if (ret) com = ret.GetComponent(comName);
                    }

                    lua.PushX(com);
                }
            }

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int FindByName(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            if (trans != null) {
                string childName = lua.OptString(2, null);
                string comName = lua.OptString(3, null);
                if (string.IsNullOrEmpty(comName)) {
                    var child = trans.FindByName(childName);
                    lua.PushX(child != null ? child.gameObject : null);
                } else {
                    Component com = null;
                    var ret = trans.FindByName(childName);
                    if (ret) com = ret.GetComponent(comName);
                    lua.PushX(com);
                }

                return 1;
            }

            return 0;
        }

        public static void ApplyParentAndChild(ILuaState lua, out GameObject parent, out GameObject child)
        {
            parent = lua.ToGameObject(1);
            if (lua.IsString(2)) {
                var strChild = lua.ToString(2);
                child = AssetLoader.Instance.Load(typeof(GameObject), strChild) as GameObject;
            } else {
                child = lua.ToGameObject(2);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int AddChild(ILuaState lua)
        {
            GameObject parent, child;
            ApplyParentAndChild(lua, out parent, out child);
            if (child != null) {
                string goName = lua.OptString(3, child.name);
                int sibling = lua.OptInteger(4, -1);
                GameObject go = ObjectPoolManager.AddChild(parent, child, sibling);
                if (go != null) {
                    go.name = goName;
                    lua.PushX(go);
                } else {
                    lua.PushNil();
                }
            } else {
                lua.PushNil();
            }

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int AddCom(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            System.Type type = lua.ToUserData(2) as System.Type;
            if (go != null && type != null) {
                var com = go.GetComponent(type);
                if (com == null) com = go.AddComponent(type);
                lua.PushX(com);
            } else {
                lua.PushNil();
            }

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int NewChild(ILuaState lua)
        {
            GameObject parent, child;
            ApplyParentAndChild(lua, out parent, out child);
            if (child != null) {
                string goName = lua.OptString(3, child.name);
                GameObject go = GoTools.NewChild(parent, child);
                int sibling = lua.OptInteger(4, -1);
                if (parent) {
                    if (sibling < 0) sibling = parent.transform.childCount + sibling;
                    go.transform.SetSiblingIndex(sibling);
                }

                if (go != null) {
                    go.name = goName;
                    lua.PushX(go);
                } else {
                    lua.PushNil();
                }
            } else {
                lua.PushNil();
            }

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetLayer(ILuaState lua)
        {
            GameObject go = lua.ToGameObject(1);
            string layerName = lua.ChkString(2);
            int iLayer = LayerMask.NameToLayer(layerName);
            if (go) {
                bool recursively = lua.OptBoolean(3, true);
                if (recursively) {
                    go.SetLayerRecursively(iLayer);
                } else {
                    go.layer = iLayer;
                }
            }

            lua.PushInteger(iLayer);
            return 1;
        }

        private static IEnumerator SetActiveDelayed(GameObject go, bool active, float delay)
        {
            yield return Yields.Seconds(delay);
            if (go) go.SetActive(active);
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetActive(ILuaState lua)
        {
            GameObject go = lua.ToGameObject(1);
            if (go != null) {
                bool active = lua.ToBoolean(2);
                float delay = lua.OptSingle(3, 0f);
                string path = lua.OptString(4, null);
                if (!string.IsNullOrEmpty(path)) {
                    go = FindGO(go.transform, path);
                }

                if (delay > 0) {
                    UIManager.Instance.StartCoroutine(SetActiveDelayed(go, active, delay));
                } else {
                    go.SetActive(active);
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ReActive(ILuaState lua)
        {
            GameObject go = lua.ToGameObject(1);
            if (go != null) {
                float delay = lua.OptSingle(2, 0f);
                go.SetActive(false);
                if (delay > 0) {
                    UIManager.Instance.StartCoroutine(SetActiveDelayed(go, true, delay));
                } else {
                    go.SetActive(true);
                }
            }

            return 0;
        }

        private static IEnumerator SetEnableDelayed(Behaviour bev, bool enabled, float delay)
        {
            yield return Yields.Seconds(delay);
            bev.enabled = enabled;
        }

        private static IEnumerator SetEnableDelayed(Collider cld, bool enabled, float delay)
        {
            yield return Yields.Seconds(delay);
            cld.enabled = enabled;
        }

        private static IEnumerator SetEnableDelayed(Renderer rdr, bool enabled, float delay)
        {
            yield return Yields.Seconds(delay);
            rdr.enabled = enabled;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetEnable(ILuaState lua)
        {
            var com = lua.ToComponent(1, typeof(Component));
            bool enabled = lua.ToBoolean(2);
            float delay = (float)lua.OptNumber(3, 0f);

            bool dirty = false;

            var bev = com as Behaviour;
            if (bev) {
                dirty = bev.enabled != enabled;
                if (dirty) {
                    if (delay <= 0) {
                        bev.enabled = enabled;
                    } else {
                        UIManager.Instance.StartCoroutine(SetEnableDelayed(bev, enabled, delay));
                    }
                }

                goto RETURN;
            }

            var cld = com as Collider;
            if (cld) {
                dirty = cld.enabled != enabled;
                if (dirty) {
                    if (delay <= 0) {
                        cld.enabled = enabled;
                    } else {
                        UIManager.Instance.StartCoroutine(SetEnableDelayed(cld, enabled, delay));
                    }
                }
            }

            var rdr = com as Renderer;
            if (rdr) {
                dirty = rdr.enabled != enabled;
                if (dirty) {
                    if (delay <= 0) {
                        rdr.enabled = enabled;
                    } else {
                        UIManager.Instance.StartCoroutine(SetEnableDelayed(rdr, enabled, delay));
                    }
                }
            }

            RETURN:
            lua.PushBoolean(dirty);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetParent(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            Transform parent = lua.ToComponent<Transform>(2);
            bool worldPositionStays = lua.OptBoolean(3, false);
            int siblingIndex = lua.OptInteger(4, -1);
            if (trans) {
                trans.Attach(parent, worldPositionStays);
                if (siblingIndex >= 0) {
                    trans.SetSiblingIndex(siblingIndex);
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetSibling(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            int index = lua.ChkInteger(2);
            if (trans) {
                if (index > -1) {
                    trans.SetSiblingIndex(index);
                } else {
                    int n = trans.parent.childCount;
                    trans.SetSiblingIndex(n + index);
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int SetPos(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            if (trans != null) {
                bool world = lua.OptBoolean(5, false);
                var pos = world ? trans.position : trans.localPosition;
                float x = lua.OptSingle(2, pos.x);
                float y = lua.OptSingle(3, pos.y);
                float z = lua.OptSingle(4, pos.z);

                if (world) {
                    trans.position = new Vector3(x, y, z);
                } else {
                    trans.localPosition = new Vector3(x, y, z);
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int GetPos(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            Vector3 pos = Vector3.zero;
            if (trans) {
                bool world = lua.OptBoolean(2, false);
                pos = world ? trans.position : trans.localPosition;
            }

            lua.PushNumber(pos.x);
            lua.PushNumber(pos.y);
            lua.PushNumber(pos.z);
            return 3;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int SetEuler(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            if (trans != null) {
                bool world = lua.OptBoolean(5, false);
                var euler = world ? trans.eulerAngles : trans.localEulerAngles;
                float x = lua.OptSingle(2, euler.x);
                float y = lua.OptSingle(3, euler.y);
                float z = lua.OptSingle(4, euler.z);

                if (world) {
                    trans.eulerAngles = new Vector3(x, y, z);
                } else {
                    trans.localEulerAngles = new Vector3(x, y, z);
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int GetEuler(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            bool world = lua.OptBoolean(2, false);
            var euler = world ? trans.eulerAngles : trans.localEulerAngles;
            lua.PushNumber(euler.x);
            lua.PushNumber(euler.y);
            lua.PushNumber(euler.z);
            return 3;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int SetScale(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            if (trans != null) {
                var scale = trans.localScale;
                float x = lua.OptSingle(2, scale.x);
                float y = lua.OptSingle(3, scale.y);
                float z = lua.OptSingle(4, scale.z);

                trans.localScale = new Vector3(x, y, z);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int GetScale(ILuaState lua)
        {
            Transform trans = lua.ToComponent<Transform>(1);
            bool world = lua.OptBoolean(2, false);
            var scale = world ? trans.lossyScale : trans.localScale;
            lua.PushNumber(scale.x);
            lua.PushNumber(scale.y);
            lua.PushNumber(scale.z);
            return 3;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int LookAt(ILuaState lua)
        {
            Transform transMe = lua.ToComponent<Transform>(1);
            Transform transTar = lua.ToComponent<Transform>(2);
            if (transMe != null && transTar != null) {
                transMe.LookAt(transTar);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int CameraForLayer(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go) {
                lua.PushLightUserData(go.FindCameraForLayer());
                return 1;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int FaceCamera(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            var cam = go.FindCameraForLayer();
            if (cam) {
                var direction = cam.transform.position - go.transform.position;
                go.transform.forward = direction.SetY(0).normalized;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int RendererSetValue(ILuaState lua)
        {
#if false
		Renderer renderer = toComponent<Renderer>((UnityEngine.Object)lua.ToUserData(1));
		if (renderer != null) {
			string propertyName = lua.L_CheckString(2);
			LuaType luaT = lua.Type(3);
			switch (luaT) {
			case LuaType.LUA_TSTRING: {
				string strColor = lua.L_CheckString(3);
				int iColor = System.Convert.ToInt32(strColor, 16);
				renderer.material.SetColor(propertyName, NGUIMath.IntToColor(iColor));
			} break;
			case LuaType.LUA_TUINT64: {
				int val = lua.L_CheckInteger(3);
				renderer.material.SetInt(propertyName, val);
			} break;
			case LuaType.LUA_TNUMBER: {
				float val = (float)lua.L_CheckNumber(3);
				renderer.material.SetFloat(propertyName, val);
			} break;
            case LuaType.LUA_TUSERDATA:
            case LuaType.LUA_TLIGHTUSERDATA: {
                System.Object o = lua.ToUserData(3);
                if (o is Texture) {
                    renderer.material.SetTexture(propertyName, (Texture)o);
                }
                } break;
			default: break;
			}
		}
#endif
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int RendererSetTexture(ILuaState lua)
        {
            Renderer renderer = lua.ToComponent<Renderer>(1);
            string propertyName = lua.ChkString(2);
            float tilingX = (float)lua.ChkNumber(3);
            float tilingY = (float)lua.ChkNumber(4);
            float offsetX = (float)lua.ChkNumber(5);
            float offsetY = (float)lua.ChkNumber(6);
            if (renderer != null) {
                renderer.material.SetTextureScale(propertyName, new Vector2(tilingX, tilingY));
                renderer.material.SetTextureOffset(propertyName, new Vector2(offsetX, offsetY));
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetMaterials(ILuaState lua)
        {
            Renderer renderer = lua.ToComponent<Renderer>(1);
            int nMat = lua.ChkInteger(2);
            Material[] mats = new Material[nMat];
            for (int i = 0; i < nMat; ++i) {
                mats[i] = lua.ToUserData(i + 3) as Material;
            }

            if (renderer) {
                renderer.enabled = true;
                renderer.materials = mats;
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ClsMaterials(ILuaState lua)
        {
            Renderer renderer = lua.ToComponent<Renderer>(1);
            if (renderer) {
                renderer.enabled = false;
                renderer.materials = new Material[1];
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int AddCullingMask(ILuaState lua)
        {
            int cullingMask = lua.ChkInteger(1);
            for (int i = 2; i < lua.GetTop(); ++i) {
                cullingMask = cullingMask.AddCullingMask(lua.ToString(i));
            }

            lua.PushInteger(cullingMask);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int DelCullingMask(ILuaState lua)
        {
            int cullingMask = lua.ChkInteger(1);
            for (int i = 2; i < lua.GetTop(); ++i) {
                cullingMask = cullingMask.DelCullingMask(lua.ToString(i));
            }

            lua.PushInteger(cullingMask);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int HasCullingMask(ILuaState lua)
        {
            int cullingMask = lua.ChkInteger(1);
            string layerName = lua.ChkString(2);
            lua.PushBoolean(cullingMask.HasCullingMask(layerName));
            return 1;
        }

        private static bool CopyPropertiesFromMat(ILuaState lua, int index, Renderer rdr, int matIdx, Material mat)
        {
            using (var scope = new MaterialPropertyScope(rdr, matIdx)) {
                bool hasPropertyBlock = true;
                lua.PushNil();
                while (lua.Next(index)) {
                    // Color = 0, Vector = 1, Float = 2, Range = 3, TexEnv = 4
                    var propId = Shader.PropertyToID(lua.ToString(-2));
                    var propType = lua.ToInteger(-1);
                    if (mat.HasProperty(propId)) {
                        switch (propType) {
                            case 0:
                                scope.block.SetColor(propId, mat.GetColor(propId));
                                break;
                            case 1:
                                scope.block.SetVector(propId, mat.GetVector(propId));
                                break;
                            case 2:
                            case 3:
                                scope.block.SetFloat(propId, mat.GetFloat(propId));
                                break;
                            case 4:
                                var tex = mat.GetTexture(propId);
                                if (tex) scope.block.SetTexture(propId, tex);
                                break;
                        }
                    } else {
                        hasPropertyBlock = false;
                    }
                    lua.Pop(1);
                }
                return hasPropertyBlock;
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int InitMaterialProperty(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go == null) return 0;

            var templateMat = lua.ToUnityObject(2) as Material;
            if (templateMat == null) return 0;

            if (!lua.IsTable(3)) return 0;

            var applyToEach = lua.OptBoolean(4, false);

            bool hasPropertyBlock = false;
            var list = ListPool<Component>.Get();
            go.GetComponentsInChildren(typeof(Renderer), list, true);
            foreach (Renderer rdr in list) {
                if (rdr.HasPropertyBlock()) continue;

                if (applyToEach) {
                    var mats = ListPool<Material>.Get();
                    rdr.GetSharedMaterials(mats);
                    for (var i = 0; i < mats.Count; ++i) {
                        var mat = mats[i];
                        if (mat == null) continue;

                        if (CopyPropertiesFromMat(lua, 3, rdr, i, mat)) {
                            hasPropertyBlock = true;
                            mats[i] = templateMat;
                        }
                    }

                    if (hasPropertyBlock) rdr.sharedMaterials = mats.ToArray();
                    ListPool<Material>.Release(mats);
                } else {
                    if (CopyPropertiesFromMat(lua, 3, rdr, -1, rdr.sharedMaterial)) {
                        hasPropertyBlock = true;
                        rdr.sharedMaterial = templateMat;
                    }
                }
            }
            ListPool<Component>.Release(list);

            lua.PushBoolean(hasPropertyBlock);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int KeepMaterialProperty(ILuaState lua)
        {
            var go = lua.ToGameObject(1);
            if (go == null) return 0;

            if (!lua.IsTable(2)) return 0;

            var index = lua.OptInteger(3, -1);

            var list = ListPool<Component>.Get();
            go.GetComponentsInChildren(typeof(Renderer), list, true);
            foreach (Renderer rdr in list) {
                var oldBlock = MaterialPropertyScope.Get();
#if UNITY_2018_3_OR_NEWER
                if (index < 0) {
                    rdr.GetPropertyBlock(oldBlock);
                } else {
                    rdr.GetPropertyBlock(oldBlock, index);
                }
#else
                rdr.GetPropertyBlock(oldBlock);
#endif
                if (oldBlock.isEmpty) continue;

                using (var scope = new MaterialPropertyScope(rdr, index)) {
                    lua.PushNil();
                    while (lua.Next(2)) {
                        // Color = 0, Vector = 1, Float = 2, Range = 3, TexEnv = 4
                        var propId = Shader.PropertyToID(lua.ToString(-2));
                        var propType = lua.ToInteger(-1);
                        switch (propType) {
                            case 0:
                                scope.block.SetColor(propId, oldBlock.GetColor(propId));
                                break;
                            case 1:
                                scope.block.SetVector(propId, oldBlock.GetVector(propId));
                                break;
                            case 2:
                            case 3:
                                scope.block.SetFloat(propId, oldBlock.GetFloat(propId));
                                break;
                            case 4:
                                var tex = oldBlock.GetTexture(propId);
                                if (tex) scope.block.SetTexture(propId, tex);
                                break;
                        }
                        lua.Pop(1);
                    }
                }
                MaterialPropertyScope.Release(oldBlock);
            }
            ListPool<Component>.Release(list);

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetMaterialProperty(ILuaState lua)
        {
            var rdr = lua.ToComponent(1, typeof(Renderer)) as Renderer;
            if (rdr && lua.IsTable(2)) {
                var index = lua.OptInteger(3, -1);
                using (var scope = new MaterialPropertyScope(rdr, index)) {
                    var prop = scope.block;
#if UNITY_2018_3_OR_NEWER
                    if (index < 0) {
                        rdr.GetPropertyBlock(prop);
                    } else {
                        rdr.GetPropertyBlock(prop, index);
                    }
#else
                    rdr.GetPropertyBlock(prop);
#endif
                    lua.PushNil();
                    while (lua.Next(2)) {
                        var name = lua.ToString(-2);
                        var type = lua.Type(-1);
                        switch (type) {
                            case LuaTypes.LUA_TNUMBER:
                                prop.SetFloat(name, (float)lua.ToNumber(-1));
                                break;
                            case LuaTypes.LUA_TSTRING:
                                Color color;
                                if (ColorUtility.TryParseHtmlString(lua.ToString(-1), out color)) {
                                    prop.SetColor(name, color);
                                } else {

                                }
                                break;
                            case LuaTypes.LUA_TTABLE: {
                                    switch (lua.Class(1)) {
                                        case UnityEngine_Color.CLASS:
                                            prop.SetColor(name, lua.ToColor(1));
                                            break;
                                        case UnityEngine_Vector4.CLASS:
                                            prop.SetVector(name, lua.ToVector4(1));
                                            break;
                                    }
                                }
                                break;
                            case LuaTypes.LUA_TLIGHTUSERDATA:
                            case LuaTypes.LUA_TUSERDATA:
                                var tex = lua.ToUnityObject(-1) as Texture;
                                if (tex) prop.SetTexture(name, tex);
                                break;
                            //default: {
                            //        var translator = lua.ToTranslator();
                            //        var uType = translator.GetTypeOf(lua, -1);
                            //        if (uType == typeof(Color)) {
                            //            prop.SetColor(name, lua.ToColor(1)); ;
                            //        } else if (uType == typeof(Vector4)) {
                            //            prop.SetVector(name, lua.ToVector4(1));
                            //        } else if (typeof(Texture).IsAssignableFrom(uType)) {
                            //            prop.SetTexture(name, lua.ToUserData(-1) as Texture);
                            //        }
                            //    }
                            //    break;
                        }
                        lua.Pop(1);
                    }
                }
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetRenderTexture(ILuaState lua)
        {
            var width = lua.ToInteger(1);
            var height = lua.ToInteger(2);
            var depth = lua.OptInteger(3, 0);

            var rt = RenderTexture.GetTemporary(width, height, depth);
            if (lua.IsTable(4)) {
                rt.format = (RenderTextureFormat)lua.GetEnum(4, "format", RenderTextureFormat.Default);
                rt.antiAliasing = lua.GetInteger(4, "antiAliasing", 1);
                rt.autoGenerateMips = lua.GetBoolean(4, "generateMips", false);
                rt.useMipMap = lua.GetBoolean(4, "useMipMap", false);
            }

            lua.PushX(rt);

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ReleaseRenderTexture(ILuaState lua)
        {
            var rt = lua.ToUnityObject(1) as RenderTexture;
            if (rt) {
                RenderTexture.ReleaseTemporary(rt);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int PlayAudio(ILuaState lua)
        {
#if FMOD
        var eventName = lua.ToString(1);
        if (!string.IsNullOrEmpty(eventName)) {
            var parent = lua.ToComponent(2, typeof(Transform)) as Transform;
            var fadeout = lua.OptBoolean(3, true);
            var emitter = FMODUnity.FMODMgr.Find(eventName, parent);
            if (emitter == null || emitter.autoDespwan >= 0) {
                emitter = FMODUnity.FMODMgr.Play(eventName, parent, fadeout);
            }

            if (lua.IsTable(4)) {
                lua.PushNil();
                while (lua.Next(4)) {
                    var paramName = lua.ToString(-2);
                    var paramValue = lua.ToSingle(-1);
                    emitter.SetParam(paramName, paramValue);
                    lua.Pop(1);
                }
            }
        }
#else
#endif
            return 0;

        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int StopAudio(ILuaState lua)
        {
#if FMOD
        var eventName = lua.ToString(1);
        var parent = lua.ToComponent(2, typeof(Transform)) as Transform;
        FMODUnity.FMODMgr.Stop(eventName, parent);
#else
#endif
            return 0;

        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAudioParams(ILuaState lua)
        {
#if FMOD
        var eventName = lua.ToString(1);
        var parent = lua.ToComponent(2, typeof(Transform)) as Transform;
        var emitter = FMODUnity.FMODMgr.Find(eventName, parent);
        if (emitter) {
            lua.PushNil();
            while (lua.Next(2)) {
                var paramName = lua.ToString(-2);
                var paramValue = lua.ToSingle(-1);
                emitter.SetParam(paramName, paramValue);
                lua.Pop(1);
            }
            lua.PushBoolean(true);
            return 1;
        }
#else
#endif
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetAudioVolume(ILuaState lua)
        {
#if FMOD
        lua.PushNumber(FMODUnity.FMODMgr.GetBusVolume(lua.ToString(1)));
#else
            lua.PushNumber(1f);
#endif
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAudioVolume(ILuaState lua)
        {
#if FMOD
        FMODUnity.FMODMgr.SetBusVolume(lua.ToString(1), lua.ToSingle(2));
#else
#endif
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetAudioMute(ILuaState lua)
        {
#if FMOD
        lua.PushBoolean(FMODUnity.FMODMgr.GetBusMute(lua.ToString(1)));
#else
            lua.PushBoolean(false);
#endif
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAudioMute(ILuaState lua)
        {
#if FMOD
        FMODUnity.FMODMgr.SetBusMute(lua.ToString(1), lua.ToBoolean(2));
#else
#endif
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetAudioPause(ILuaState lua)
        {
#if FMOD
        lua.PushBoolean(FMODUnity.FMODMgr.GetBusPause(lua.ToString(2)));
#else
            lua.PushBoolean(false);
#endif
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetAudioPause(ILuaState lua)
        {
#if FMOD
        FMODUnity.FMODMgr.SetBusPause(lua.ToString(1), lua.ToBoolean(2));
#else
#endif
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SendMessage(ILuaState lua)
        {
            GameObject go = lua.ToGameObject(1);
            if (go != null && go.activeInHierarchy) {
                string method = lua.ChkString(2);
                object obj = lua.ToAnyObject(3);
                if (obj == null) {
                    go.SendMessage(method, SendMessageOptions.RequireReceiver);
                } else {
                    go.SendMessage(method, obj, SendMessageOptions.RequireReceiver);
                }
            }

            return 0;
        }

        public static IEnumerator LuaInvoke(MonoBehaviour mono, LuaFunction func, float delay, object param)
        {
            if (delay > 0) {
                yield return Yields.Seconds(delay);
            } else if (delay == 0) {
                yield return null;
            }

            using (func) {
                if (mono && mono.isActiveAndEnabled) {
                    var lua = func.GetState();
                    var top = func.BeginPCall();
                    lua.PushAnyObject(param);
                    func.PCall(top, 1);
                    func.EndPCall(top);
                }
            }

            var disposable = param as System.IDisposable;
            if (disposable != null) disposable.Dispose();
        }

        private static IEnumerator LuaInvokeRepeating(MonoBehaviour mono, LuaFunction func, float delay, float wait, object param)
        {
            if (delay > 0) {
                yield return Yields.Seconds(delay);
            } else if (delay == 0) {
                yield return null;
            }

            using (func) {
                var lua = func.GetState();
                var waiting = wait > 0 ? Yields.Seconds(wait) : null;
                for (int i = 0; mono && mono.isActiveAndEnabled; ++i) {
                    func.push(lua);
                    var b = lua.BeginPCall();
                    lua.PushInteger(i);
                    lua.PushAnyObject(param);
                    lua.ExecPCall(2, 1, b);
                    var finished = lua.OptBoolean(-1, false);
                    lua.Pop(1);
                    if (finished) break;

                    yield return waiting;
                }
            }

            var disposable = param as System.IDisposable;
            if (disposable != null) disposable.Dispose();
        }

        private static MonoBehaviour ToAsyncMono(this ILuaState self, int index, bool allowNull = true)
        {
            var mono = self.ToComponent(index, typeof(LuaComponent)) as MonoBehaviour;
            if (mono == null) mono = self.ToComponent<MonoBehaviour>(index);
            if (mono == null && allowNull) mono = UIManager.Instance;

            return mono;
        }

        /// <summary>
        /// Invoke一个Lua函数
        /// </summary
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Invoke(ILuaState lua)
        {
            var mono = lua.ToAsyncMono(1);
            if (mono.isActiveAndEnabled) {
                var func = lua.ToLuaFunction(3);
                if (func != null) {
                    var wait = lua.ToSingle(2);
                    var param = lua.ToAnyObject(4);
                    //var tag = lua.OptString(5, null);
                    mono.StartCoroutine(LuaInvoke(mono, func, wait, param));
                } else {
                    LogMgr.W("{0}: function is nil for Invoke", mono);
                }
            } else {
                LogMgr.W("{0}{1} is nor Active or Enable for Invoke", lua.DebugCurrentLine(2), mono);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int InvokeRepeating(ILuaState lua)
        {
            var mono = lua.ToAsyncMono(1);
            if (mono.isActiveAndEnabled) {
                var func = lua.ToLuaFunction(4);
                if (func != null) {
                    var delay = lua.ToSingle(2);
                    var wait = lua.ToSingle(3);
                    var param = lua.ToAnyObject(5);
                    //var tag = lua.OptString(6, null);
                    mono.StartCoroutine(LuaInvokeRepeating(mono, func, delay, wait, param));
                } else {
                    LogMgr.W("{0}: function is nil for InvokeRepeating", mono);
                }
            } else {
                LogMgr.W("{0} is nor Active or Enable for InvokeRepeating", mono);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int CancelInvoke(ILuaState lua)
        {
            if (lua.Type(1) == LuaTypes.LUA_TSTRING) {
                // MEC.Timing.KillCoroutines(lua.ToString(1));
            } else {
                var mono = lua.ToAsyncMono(1, false);
                if (mono && mono.isActiveAndEnabled) {
                    mono.StopAllCoroutines();
                }
            }

            return 0;
        }


        private static IEnumerator LuaCoroutine(ILuaState lua, MonoBehaviour mono, int func, object yieldRet)
        {
            lua.GetGlobal("coroutine", "status");
            var coro_status = lua.L_Ref(LuaIndexes.LUA_REGISTRYINDEX);
            lua.GetGlobal("coroutine", "resume");
            var corou_resume = lua.L_Ref(LuaIndexes.LUA_REGISTRYINDEX);

            for (bool coroRet = true; mono.isActiveAndEnabled;) {
                yield return yieldRet;

                var oldTop = lua.GetTop();

                // 检查协程状态
                lua.GetRef(coro_status);
                lua.GetRef(func);
                lua.PCall(1, 1, 0);
                var coStat = lua.ToString(-1);
                lua.Pop(1);
                if (coStat == "dead") {
                    break;
                }

                // 再启动协程
                lua.GetRef(corou_resume);
                lua.GetRef(func);
                var status = lua.PCall(1, 2, 0);
                if (status != LuaThreadStatus.LUA_OK) {
                    LogMgr.E(lua.ToString(-1));
                    lua.SetTop(oldTop);
                    break;
                }

                coroRet = lua.ToBoolean(-2);
                yieldRet = lua.ToYieldValue(-1);
                // 弹出返回值
                lua.Pop(2);
                if (!coroRet) {
                    LogMgr.E("{0}", yieldRet);
                    break;
                }
            }

            lua.L_Unref(LuaIndexes.LUA_REGISTRYINDEX, coro_status);
            lua.L_Unref(LuaIndexes.LUA_REGISTRYINDEX, corou_resume);
            lua.L_Unref(LuaIndexes.LUA_REGISTRYINDEX, func);
        }

        /// <summary>
        /// void function(@MonoBehaviour, function, [args, ...]) 
        /// ！！！严重注意：
        ///     Lua的协程有独立的栈空间和局部变量，
        ///     如果在这过程中保存变量的引用，
        ///     在退出协程后，其引用会失效。
        /// </summary>
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int StartCoroutine(ILuaState lua)
        {
            var oldTop = lua.GetTop();
            // 参数列表 (mono, func, [args, ...])
            var mono = lua.ToAsyncMono(1);
            if (mono.isActiveAndEnabled) {
                // 创建lua协程
                lua.GetGlobal("coroutine", "create");
                lua.PushValue(2);
                var status = lua.PCall(1, 1, 0);
                if (status == LuaThreadStatus.LUA_OK) {
                    // 保存协程引用(这个其实不是函数，应该是LUA_TTHREAD类型)
                    var func = lua.L_Ref(LuaIndexes.LUA_REGISTRYINDEX);

                    var top = lua.GetTop();
                    // 协程启动方法入栈
                    lua.GetGlobal("coroutine", "resume");
                    // 协程入栈
                    lua.GetRef(func);
                    // 启动参数入栈
                    for (int i = 3; i < top + 1; ++i) {
                        lua.PushValue(i);
                    }

                    // 总参数数量=协程+启动参数数量，即：1 + (top - 2)
                    status = lua.PCall(top - 1, 2, 0);
                    if (status == LuaThreadStatus.LUA_OK) {
                        var coroRet = lua.ToBoolean(-2);
                        var yieldRet = lua.ToYieldValue(-1);
                        // 弹出返回值
                        lua.Pop(2);
                        if (coroRet) {
                            if (mono.isActiveAndEnabled) {
                                var coro = mono.StartCoroutine(LuaCoroutine(lua, mono, func, yieldRet));
                                lua.PushLightUserData(coro);
                                return 1;
                            } else {
                                // 无法启动协程
                                lua.L_Unref(LuaIndexes.LUA_REGISTRYINDEX, func);
                            }
                        } else LogMgr.E("{0}", yieldRet);
                    } else LogMgr.E("coroutine.resume FAIL!");
                } else LogMgr.E("coroutine.create FAIL!");

                lua.SetTop(oldTop);
            } else {
                LogMgr.W("MonoBehaviour<{0}> not exist OR not isActiveAndEnabled", mono);
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int StopAllCoroutine(ILuaState lua)
        {
            var mono = lua.ToAsyncMono(1, false);
            if (mono && mono.isActiveAndEnabled) mono.StopAllCoroutines();
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetScreenCoord(ILuaState lua)
        {
            var v2 = lua.ToVector2(1);
            var coord = new Vector2(Screen.width * v2.x, Screen.height * v2.y);
            lua.PushX(coord);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int Raycast(ILuaState lua)
        {
            var cam = lua.ToComponent<Camera>(1);
            Vector2 pos = Vector2.zero;
            if (lua.Type(2) == LuaTypes.LUA_TUSERDATA || lua.IsClass(2, "UnityObject")) {
                var trans = lua.ToComponent(2, typeof(Transform)) as Transform;
                if (trans) {
                    var objCam = trans.gameObject.FindCameraForLayer();
                    if (objCam) pos = objCam.WorldToScreenPoint(trans.position);
                }
            } else {
                pos = lua.ToVector2(2);
            }

            var far = lua.ToSingle(3);
            var mask = lua.ToInteger(4);
            if (cam == null) cam = Camera.main;

            if (cam) {
                RaycastHit hit;
                if (cam.ScreenRaycast(pos, far, mask, out hit)) {
                    lua.PushX(hit.point);
                    lua.PushX(hit.collider.gameObject);
                    return 2;
                }
            }

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int IsLevelLoaded(ILuaState lua)
        {
            string levelName = lua.ChkString(1);
            var scene = SceneManager.GetSceneByName(levelName);
            lua.PushBoolean(scene.IsValid());
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int LoadLevel(ILuaState lua)
        {
            string levelName = lua.ChkString(1);
            var mode = (LoadSceneMode)lua.OptEnumValue(2, typeof(LoadSceneMode), LoadSceneMode.Single);
            var funcRef = 0;
            if (lua.IsFunction(3)) {
                lua.PushValue(3);
                funcRef = lua.L_Ref(LuaIndexes.LUA_REGISTRYINDEX);
            }

            WNDLoading.LoadLevel(levelName, mode, funcRef);
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ActiveLevel(ILuaState lua)
        {
            string levelName = lua.ChkString(1);
            var scene = SceneManager.GetSceneByName(levelName);
            var valid = scene.IsValid();
            if (valid) SceneManager.SetActiveScene(scene);
            lua.PushBoolean(valid);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int UnloadLevel(ILuaState lua)
        {
            var levelName = lua.ToString(1);
            SceneManager.UnloadSceneAsync(levelName);
            AssetLoader.Instance.UnloadAll();
            AssetLoader.Instance.ClearPreload();
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int NewLevel(ILuaState lua)
        {
            string levelName = lua.ChkString(1);
            AssetLoader.Instance.ClearPreload();
            AssetLoader.Instance.UnloadAll(true);
            SceneManager.LoadSceneAsync(levelName);
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GC(ILuaState lua)
        {
            AssetLoader.Instance.GC();
            return 0;
        }
    }
}