using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ObjectPoolManager
// Author: William Ravaine - spk02@hotmail.com (Spk on Unity forums)
// Date: 15-11-09
//
// <LEGAL BLABLA>
// This code package is provided "as is" with no express or implied warranty of any kind. You may use this code for both
// commercial and non-commercial use without any restrictions. Any modification and/or redistribution of this code should
// include the original author's name, contact information and also this paragraph.
// </LEGAL BLABLA>
//
// The goal of this class is to avoid costly runtime memory allocation for objects that are created and destroyed very often during
// gameplay (e.g. projectile, enemies, etc). It achieves this by recycling "destroyed" objects from an internal cache instead of physically
// removing them from memory via Object.Destroy().
//
// To use the ObjectPoolManager, you simply need to replace your regular object creation & destruction calls by ObjectPoolManager.CreatePooled()
// and ObjectPoolManager.DestroyPooled(). Here's an exemple:
//
// 1) Without using the ObjectPoolManager:
// Projectile bullet = Instanciate( bulletPrefab, position, rotation ) as Projectile;
// Destroy( bullet.gameObject );
// 
// 2) Using the ObjetPoolManager:
// Projectile bullet = ObjectPoolManager.CreatePooled( bulletPrefab.gameObject, position, rotation ).GetComponent<Bullet>();
// ObjectPoolManager.DestroyPooled( bullet.gameObject );
//
// When a recycled object is revived from the cache, the ObjectPoolManager calls its Start() method again, so this object can reset itself as
// if it just got newly created.
//
// When using the ObjectPoolManager with your objects, you need to keep several things in mind:
// 1. You need to be in full control of the creation and destruction of the object (so they go through ObjectPoolManager). This means you shouldn't
//	  use it on objects that use exotic destruction methods (e.g. auto-destroy option on particle effects) because the ObjectPoolManager will
//	  not be able to recycle the object 
// 2. When they get revived from the ObjectPoolManager cache, the pooled objects are responsible for re-initializing themselves as if they had
//	  just been newly created via a regular call Instantiate(). So look out for any dynamic component additions and modifications of the initial
//	  object public fields during gameplay

public class ObjectPoolManager : MonoBehaviour
{
    // Only one ObjectPoolManager can exist. We use a singleton pattern to enforce this.
    #region Singleton Access

    static ObjectPoolManager instance = null;
    public static ObjectPoolManager Instance {
        get {
            if (!instance) {
                GameObject obj = new GameObject("ObjectPoolManager");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<ObjectPoolManager>();
            }
            return instance;
        }
    }

    private static ObjectPoolManager m_ScenePool = null;

    public static void NewScenePool()
    {
        if (m_ScenePool == null) {
            GameObject obj = new GameObject("SceneObjectPoolMgr");
            m_ScenePool = obj.AddComponent<ObjectPoolManager>();
        }
    }
    
    #endregion

    #region Public fields

    // turn this on to activate debugging information
    public bool debug = false;
    public Color debugColor = Color.white;

    // the GUI block where the debugging info will be displayed
    public Rect debugGuiRect = new Rect(60, 90, 160, 400);

    #endregion

    #region Private fields

    // This maps a prefab to its ObjectPool
    Dictionary<GameObject, ObjectPool> prefab2pool;

    // This maps a game object instance to the ObjectPool that created/recycled it
    Dictionary<GameObject, ObjectPool> instance2pool;

    #endregion

    #region Public Interface (static for convenience)

    // Create a pooled object. This will either reuse an object from the cache or allocate a new one
    static GameObject CreateGameObject(ObjectPoolManager mgr, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return mgr.InternalCreate(prefab, position, rotation, null, -1);
    }

    public static GameObject CreatePooled(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return CreateGameObject(Instance, prefab, position, rotation);
    }
    public static GameObject CreatePooledScenely(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return m_ScenePool ? CreateGameObject(m_ScenePool, prefab, position, rotation) : null;
    }

    // Destroy the object now
    static void DestroyGameObject(ObjectPoolManager mgr, GameObject obj)
    {
        if (mgr) {
            mgr.InternalDestroy(obj);
        } else {
            Destroy(obj);
        }
    }

    // Destroy the object after <delay> seconds have elapsed
    static void DestroyGameObject(ObjectPoolManager mgr, GameObject obj, float delay, bool ignoreTimescale)
    {
        if (mgr) {
            if (ignoreTimescale) {
                mgr.StartCoroutine(mgr.InternalDestroyIgnoreTimescale(obj, delay));
            } else {
                mgr.StartCoroutine(mgr.InternalDestroy(obj, delay));
            }
        } else {
            // NOT support <ignoreTimescale>
            Destroy(obj, delay);
        }
    }
    public static void DestroyPooled(GameObject obj, float delay = 0f)
    {
        if (delay > 0) {
            DestroyGameObject(instance, obj, delay, true);
        } else {
            DestroyGameObject(instance, obj);
        }
    }
    public static void DestroyPooledScenely(GameObject obj, float delay = 0f)
    {
        if (delay > 0) {
            DestroyGameObject(m_ScenePool, obj, delay, false);
        } else {
            DestroyGameObject(m_ScenePool, obj);
        }
    }
    private static ObjectPool IsObjectPooled(ObjectPoolManager mgr, GameObject obj)
    {
        ObjectPool pool;
        mgr.instance2pool.TryGetValue(obj, out pool);
        return pool;
    }

    public static ObjectPool IsPooled(GameObject obj)
    {
        return instance ? IsObjectPooled(instance, obj) : null;
    }

    public static ObjectPool IsPooledScenely(GameObject obj)
    {
        return m_ScenePool ? IsObjectPooled(m_ScenePool, obj) : null;
    }

    private static PoolStatus GetObjectPoolStatus(ObjectPoolManager mgr, GameObject obj)
    {
        if (obj) {
            ObjectPool pool;
            if (mgr.instance2pool.TryGetValue(obj, out pool)) {
                return pool.Constains(obj) ? PoolStatus.Recycled : PoolStatus.Pooled;
            };
        }

        return PoolStatus.None;
    }

    public static PoolStatus GetPoolStatus(GameObject obj)
    {
        return instance ? GetObjectPoolStatus(instance, obj) : PoolStatus.None;
    }

    public static PoolStatus GetPoolStatusScenely(GameObject obj)
    {
        return m_ScenePool ? GetObjectPoolStatus(m_ScenePool, obj) : PoolStatus.None;
    }

    private static GameObject AddChild(ObjectPoolManager mgr, GameObject parent, GameObject child, int siblingIndex)
    {
        GameObject go = null;
        if (child != null) {
            if (mgr) {
                go = mgr.InternalCreate(child, Vector3.zero, Quaternion.identity, 
                    parent ? parent.transform : null, siblingIndex);
            } else {
                go = Instantiate(child);
                ObjectPool.InitObjectTransform(go, parent ? parent.transform : null, 
                    Vector3.zero, Quaternion.identity, siblingIndex);
            }

            if (go != null) {
                Transform t = go.transform;
                if (parent != null) {
                    go.layer = parent.layer;
                }
                t.localScale = Vector3.one;
                go.name = child.name;
            }
        }
        return go;
    }
    public static GameObject AddChild(GameObject parent, GameObject child, int siblingIndex = -1)
    {
        return AddChild(Instance, parent, child, siblingIndex);
    }
    public static GameObject AddChildScenely(GameObject parent, GameObject child, int siblingIndex = -1)
    {
        return AddChild(m_ScenePool, parent, child, siblingIndex);
    }

    private static GameObject DupChild(ObjectPoolManager mgr, GameObject parent, GameObject child, int siblingIndex)
    {
        GameObject go = null;
        if (child != null) {
            if (mgr) {
                go = mgr.Duplicate(child, Vector3.zero, Quaternion.identity, 
                    parent ? parent.transform : null, siblingIndex);
            } else {
                go = Instantiate(child);
                ObjectPool.InitObjectTransform(go, parent ? parent.transform : null, 
                    Vector3.zero, Quaternion.identity, siblingIndex);
            }

            if (go != null) {
                Transform t = go.transform;
                if (parent != null) {
                    go.layer = parent.layer;
                }
                t.localScale = Vector3.one;
                go.name = child.name;
            }
        }
        return go;
    }
    public static GameObject DupChild(GameObject parent, GameObject child, int siblingIndex = -1)
    {
        return DupChild(Instance, parent, child, siblingIndex);
    }
    public static GameObject DupChildScenely(GameObject parent, GameObject child, int siblingIndex = -1)
    {
        return DupChild(m_ScenePool, parent, child, siblingIndex);
    }

    #endregion

    #region Private implementation

    // Constructor
    void Awake()
    {
        prefab2pool = new Dictionary<GameObject, ObjectPool>();
        instance2pool = new Dictionary<GameObject, ObjectPool>();
    }

    private ObjectPool CreatePool(GameObject prefab)
    {
        GameObject obj = new GameObject(prefab.name + " Pool");
        ObjectPool pool = obj.AddComponent<ObjectPool>();
        pool.Prefab = prefab;
        return pool;
    }

    private GameObject InternalCreate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, int siblingIndex)
    {
        ObjectPool pool;

        if (!prefab2pool.TryGetValue(prefab, out pool)) {
            pool = CreatePool(prefab);
            pool.gameObject.transform.parent = this.gameObject.transform;
            prefab2pool[prefab] = pool;
        }

        // create a new object or reuse an existing one from the pool
        GameObject obj = pool.Instanciate(position, rotation, parent, siblingIndex);

        // remember which pool this object was created from
        instance2pool[obj] = pool;

        return obj;
    }

    private GameObject Duplicate(GameObject go, Vector3 position, Quaternion rotation, Transform parent, int siblingIndex)
    {
        ObjectPool pool;
        if (!instance2pool.TryGetValue(go, out pool)) {
            Debug.LogWarning("Duplicate non-pooled object " + go.name);
            pool = CreatePool(go);
            pool.gameObject.transform.parent = this.gameObject.transform;
            prefab2pool[go] = pool;
        }

        // create a new object or reuse an existing one from the pool
        GameObject obj = pool.Instanciate(position, rotation, parent, siblingIndex);

        // remember which pool this object was created from
        instance2pool[obj] = pool;

        return obj;
    }

    private void InternalDestroy(GameObject obj)
    {
        if (obj != null) {
            ObjectPool pool;
            instance2pool.TryGetValue(obj, out pool);
            if (pool) {
                pool.Recycle(obj);
            } else {
                // This object was not created through the ObjectPoolManager, give a warning and destroy it the "old way"
                LogMgr.W("{0}#Destroying non-pooled object \"{1}\"", name, obj.name);
                Object.Destroy(obj);
            }
        }
    }

    // must be run as coroutine
    private IEnumerator InternalDestroyIgnoreTimescale(GameObject obj, float delay)
    {
        if (obj != null) {
            var t = Time.unscaledTime + delay;
            for (; ; ) {
                yield return null;
                if (Time.unscaledTime >= t) {
                    InternalDestroy(obj);
                    break;
                }
            }
        }
    }

    private IEnumerator InternalDestroy(GameObject obj, float delay)
    {
        if (obj != null) {
            var t = Time.time + delay;
            for (; ; ) {
                yield return null;
                if (Time.time >= t) {
                    InternalDestroy(obj);
                    break;
                }
            }
        }
    }

    #endregion

    private void OnDisable()
    {
        if (instance2pool != null) {
            foreach (var inst in instance2pool.Keys) {
                Destroy(inst);
            }
        }
    }

    void OnDestroy()
    {

        if (instance == this) {
            instance = null;
        }
        if (m_ScenePool == this) {
            m_ScenePool = null;
        }
    }

    void OnGUI()
    {
#if UNITY_EDITOR
        if (debug) {
            GUILayout.BeginArea(debugGuiRect);
            GUILayout.BeginVertical();

            GUI.color = debugColor;
            GUILayout.Label("Pools: " + prefab2pool.Count);

            foreach (ObjectPool pool in prefab2pool.Values)
                GUILayout.Label(pool.Prefab.name + ": " + pool.Count);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
#endif
    }
}
