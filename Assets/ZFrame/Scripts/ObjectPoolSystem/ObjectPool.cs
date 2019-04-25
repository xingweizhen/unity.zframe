using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZFrame.Asset;

public enum PoolStatus
{
    None, Pooled, Recycled,
}

// The ObjectPool is the storage class for pooled objects of the same kind (e.g. "Pistol Bullet", or "Enemy A")
// This is used by the ObjectPoolManager and is not meant to be used separately
public class ObjectPool : MonoBehavior
{
    // The type of object this pool is handling	
    public GameObject Prefab { get; set; }

    // This stores the cached objects waiting to be reactivated
    [Description("回收池")]
    Queue<GameObject> pool;

    // How many objects are currently sitting in the cache
    public int Count {
        get { return pool.Count; }
    }

    public void Awake()
    {
        pool = new Queue<GameObject>();
    }

    private void OnDisable()
    {
        if (pool != null) {
            while (pool.Count > 0) {
                var inst = pool.Dequeue();
                if (inst != null) Destroy(inst);
            }
        }
    }

    public bool Constains(GameObject obj)
    {
        return pool.Contains(obj);
    }

    public GameObject Instanciate(Vector3 position, Quaternion rotation, Transform parent, int siblingIndex)
    {
        // Try to pull one from the cache
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : null;

        // if we don't have any object already in the cache, create a new one
        if (!obj) {
            obj = Object.Instantiate(Prefab) as GameObject;
            AssetLoader.AssignEditorShaders(obj);
            InitObjectTransform(obj, parent, position, rotation, siblingIndex);
        } else {
        // else use the pulled one
            // reactivate the object
            InitObjectTransform(obj, parent, position, rotation, siblingIndex);
            obj.SetActive(true);

            var list = ZFrame.ListPool<Component>.Get();
            obj.GetComponents(typeof(IPoolable), list);
            if (list.Count > 0) {
                obj.hideFlags &= ~HideFlags.HideInHierarchy;
                for (int i = 0; i < list.Count; ++i) ((IPoolable)list[i]).OnRestart();                
            } else {
                //StartCoroutine(Restart(obj));
                
                // 如果该对象创建后没有调用过Start，此处会导致调用两次Start
                obj.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
            }
            ZFrame.ListPool<Component>.Release(list);
        }

        return obj;
    }

    // put the object in the cache and deactivate it
    public void Recycle(GameObject obj)
    {
        if (!pool.Contains(obj)) {
            // put object back in cache for reuse later (Avoid duplication enqueue)
            pool.Enqueue(obj);

            if (!obj.activeInHierarchy) {
                LogMgr.D("被回收对象\"{0}\"不是Active状态，无法接收到OnRecycle消息", obj);
            }

            var list = ZFrame.ListPool<Component>.Get();
            obj.GetComponents(typeof(IPoolable), list);
            if (list.Count > 0) {
                for (int i = 0; i < list.Count; ++i) ((IPoolable)list[i]).OnRecycle();
                obj.hideFlags |= HideFlags.HideInHierarchy;
            } else {
                obj.SendMessage("OnRecycle", SendMessageOptions.DontRequireReceiver);
                // deactivate the object
                obj.SetActive(false);
                // put the recycled object in this ObjectPool's bucket
                obj.transform.SetParent(transform, false);
            }
            ZFrame.ListPool<Component>.Release(list);
        } else {
            LogMgr.I("尝试回收一个已经被回收的对象: \"{0}\"", obj.name);
        }
    }

    public static void InitObjectTransform(GameObject obj, Transform parent, Vector3 position, Quaternion rotation, int siblingIndex)
    {
        var t = obj.transform;
        t.SetParent(parent, false);
        t.localRotation = rotation;
        var rect = t as RectTransform;
        if (rect) {
            rect.anchoredPosition3D = position;
        } else {
            t.localPosition = position;
        }

        if (parent && siblingIndex != -1) {
            var childCount = parent.childCount;
            if (siblingIndex < 0) {
                t.SetSiblingIndex(childCount + siblingIndex);
            } else {
                t.SetSiblingIndex(siblingIndex);
            }
        }
    }
}
