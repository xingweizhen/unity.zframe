using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZFrame.Asset;

public static class GoTools
{
    public static void Attach(this Component self, Transform parent, bool worldPositionStays = false)
    {
        var trans = self.transform;
        var rect = trans as RectTransform;
        var pos = rect ? rect.anchoredPosition3D : Vector3.zero;

        if (parent) {
            //self.gameObject.layer = parent.gameObject.layer;
            trans.SetParent(parent, worldPositionStays);
        } else {
            trans.SetParent(null, worldPositionStays);
        }
        if (!worldPositionStays) {
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;            
            if (rect) {
                rect.anchoredPosition3D = pos;
            } else {
                trans.localPosition = pos;
            }
        }
    }

    public static void Attach(this GameObject self, Transform parent, bool worldPositionStays = true)
    {
        self.transform.Attach(parent, worldPositionStays);
    }

    public static GameObject NewChild(GameObject parent, GameObject prefab = null, string name = null)
	{
		GameObject go = prefab ? Object.Instantiate(prefab) : new GameObject();
		if (go) {
            go.SetActive(true);
            go.Attach(parent ? parent.transform : null, false);
            go.name = string.IsNullOrEmpty(name) ? (prefab ? prefab.name : "GameObject") : name;
            AssetLoader.AssignEditorShaders(go);
		}
		return go;
	}

    public static GameObject NewChild(GameObject parent, string path)
    {
        var prefab = AssetLoader.Instance.Load(typeof(GameObject), path) as GameObject;
        return NewChild(parent, prefab);
    }

    public static T CreateChild<T>(GameObject parent, string name = null) where T : Component
    {
        GameObject go = new GameObject();
        if (!string.IsNullOrEmpty(name)) go.name = name;
        go.Attach(parent ? parent.transform : null, false);
        return go.AddComponent<T>();
    }

    public static Component NeedComponent(this GameObject self, System.Type type)
    {
        var c = self.GetComponent(type);
        if (c == null) {
            c = self.AddComponent(type);
        }
        return c;
    }

    public static T NeedComponent<T>(this GameObject self) where T : Component
    {
        T c = self.GetComponent<T>();
        if (c == null) {
            c = self.AddComponent<T>();
        }
        return c;
    }

    public static GameObject NeedChild(this GameObject parent, string path)
    {
        GameObject go = null;
        var trans = parent.transform.Find(path);
        if (trans) {
            go = trans.gameObject;
        } else {
            var slash = path.LastIndexOf('/');
            if (slash < 0) {
                go = new GameObject(path);                
                go.layer = parent.layer;

                var t = go.transform;
                t.SetParent(parent.transform);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            } else {
                go = parent.NeedChild(path.Substring(slash + 1));
            }
        }
        return go;
    }

    public static GameObject AddChild(GameObject parent, GameObject prefab, bool scenely = false)
    {
        if (prefab == null) return null;

        GameObject go = scenely ? ObjectPoolManager.AddChildScenely(parent, prefab)
            : ObjectPoolManager.AddChild(parent, prefab);
        go.SetActive(true);
        go.Attach(parent ? parent.transform : null, false);
        go.name = prefab ? prefab.name : "GameObject";
        
        return go;
    }

    public static GameObject AddChild(GameObject parent, string path, bool scenely = false)
    {
        var prefab = AssetLoader.Instance.Load(typeof(GameObject), path) as GameObject;
        return AddChild(parent, prefab, scenely);
    }

    public static void AutoRecycle(GameObject go, float delay = 0)
    {
        var poolStatus = ObjectPoolManager.GetPoolStatus(go);
        if (poolStatus != PoolStatus.None) {
            if (poolStatus == PoolStatus.Pooled) {
                ObjectPoolManager.DestroyPooled(go, delay);
            }
            return;
        }

        poolStatus = ObjectPoolManager.GetPoolStatusScenely(go);
        if (poolStatus != PoolStatus.None) {
            if (poolStatus == PoolStatus.Pooled) {
                ObjectPoolManager.DestroyPooledScenely(go, delay);
            }
            return;
        }

        Object.Destroy(go, delay);
    }

    public static void Destroy(GameObject go)
    {
        var poolStatus = ObjectPoolManager.GetPoolStatus(go);
        if (poolStatus != PoolStatus.None) {
            if (poolStatus == PoolStatus.Pooled) {
                ObjectPoolManager.DestroyPooled(go);
            }
        } else {
            Object.Destroy(go);
        }
    }

    public static void DestroyScenely(GameObject go, float delay = 0f)
    {
        var poolStatus = ObjectPoolManager.GetPoolStatusScenely(go);
        if (poolStatus != PoolStatus.None) {
            if (poolStatus == PoolStatus.Pooled) {
                ObjectPoolManager.DestroyPooledScenely(go, delay);
            }
        } else {
            Object.Destroy(go, delay);
        }
    }

    /// <summary>
    /// 只会回收被池子管理的对象
    /// </summary>
    public static void DestroyPooled(GameObject go, float delay = 0f)
    {
        var poolStatus = ObjectPoolManager.GetPoolStatus(go);
        if (poolStatus == PoolStatus.Pooled) {
            ObjectPoolManager.DestroyPooled(go, delay);
        }
    }
    /// <summary>
    /// 只会回收被池子管理的对象
    /// </summary>
    public static void DestroyPooledScenely(GameObject go, float delay = 0f)
    {
        var poolStatus = ObjectPoolManager.GetPoolStatusScenely(go);
        if (poolStatus == PoolStatus.Pooled) {
            ObjectPoolManager.DestroyPooledScenely(go, delay);
        }
    }

    private static void _GetComponentsInChildren(Transform trans, System.Type type, List<Component> results, bool includeInactive)
    {
        var list = ZFrame.ListPool<Component>.Get();
        trans.GetComponents(type, list);
        results.AddRange(list);
        ZFrame.ListPool<Component>.Release(list);
        for (var i = 0; i < trans.childCount; ++i) {
            var child = trans.GetChild(i);
            if (includeInactive || child.gameObject.activeInHierarchy) {
                _GetComponentsInChildren(child, type, results, includeInactive);
            }
        }
    }

    public static void GetComponentsInChildren(this GameObject go, System.Type type, List<Component> results, bool includeInactive = false)
    {
        _GetComponentsInChildren(go.transform, type, results, includeInactive);
    }

    public static void GetComponentsInChildren(this Component com, System.Type type, List<Component> results, bool includeInactive = false)
    {
        _GetComponentsInChildren(com.transform, type, results, includeInactive);
    }

    public static void SetLayerRecursively(this GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
            t.gameObject.SetLayerRecursively(layer);
    }
    
    public static void SetHideFlagsRecursively(this GameObject go, HideFlags hideFlags)
    {
        go.hideFlags = hideFlags;
        foreach (Transform t in go.transform)
            t.gameObject.SetHideFlagsRecursively(hideFlags);
    }

    public static Camera FindCameraForLayer(this GameObject self)
    {
        int layer = self.layer;
        int layerMask = 1 << layer;

        if (self.transform is RectTransform) {
            for (var t = self.transform; t != null; ) {
                var canvas = t.GetComponentInParent(typeof(Canvas)) as Canvas;
                if (canvas == null) break;
                if (canvas.worldCamera) return canvas.worldCamera;
                t = canvas.transform.parent;
            }
        }

        Camera cam = Camera.main;
        if (cam == null) {
            var goCam = GameObject.FindWithTag("MainCamera");
            if (goCam) cam = goCam.GetComponent<Camera>();
        }

        if (cam && cam.isActiveAndEnabled && (cam.cullingMask & layerMask) != 0) return cam;

        Camera[] cameras = new Camera[Camera.allCamerasCount];
        int camerasFound = Camera.GetAllCameras(cameras);
        for (int i = 0; i < camerasFound; ++i) {
            cam = cameras[i];
            if ((cam.cullingMask & layerMask) != 0) return cam;
        }

        return null;
    }

    public static GameObject AddForever(GameObject prefab, string name = null)
    {
        GameObject go = NewChild(null, prefab, name);
        Object.DontDestroyOnLoad(go);
        return go;
    }

    public static GameObject Seek(string path)
    {
        GameObject go = GameObject.Find(path);
        if (go == null) {
            var parent = string.Empty;
            var slashIdx = path.IndexOf('/');
            if (slashIdx == 0) {
                parent = "/";
                path = path.Substring(1);
                slashIdx = path.IndexOf('/');
            }
            if (slashIdx > 0) {
                parent += path.Substring(0, slashIdx);
                var root = GameObject.Find(parent);
                if (root) {
                    var trans = root.transform.Find(path.Substring(slashIdx + 1));
                    go = trans ? trans.gameObject : null;
                }
            }
        }
        return go;
    }

    
    
}
