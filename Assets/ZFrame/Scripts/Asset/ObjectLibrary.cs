using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.Asset
{
    public class ObjectLibrary : MonoBehaviour
    {
        [FormerlySerializedAs("Objects"), SerializeField]
        private List<Object> m_Objects = new List<Object>();

        public IEnumerator<T> ObjectsOfType<T>() where T : Object
        {
            for (int i = 0; i < m_Objects.Count; ++i) {
                var obj = m_Objects[i] as T;
                if (obj != null) yield return obj;
            }
        }

        public void Clear() { m_Objects.Clear(); }

        public bool Set(Object addedObj)
        {
            if (!m_Objects.Contains(addedObj)) {
                for (int i = 0; i < m_Objects.Count; ++i) {
                    if (m_Objects[i] == null) {
                        m_Objects[i] = addedObj;
                        addedObj = null;
                        break;
                    }
                }
                if (addedObj) {
                    m_Objects.Add(addedObj);
                }
                return true;
            }

            return false;
        }

        public Object Get(string name, bool warnIfMissing = true)
        {
            for (int i = 0; i < m_Objects.Count; ++i) {
                var o = m_Objects[i];
                if (o && o.name == name) {
                    return o;
                }
            }

            if (warnIfMissing) LogMgr.W("[{0}]Get {1} = NULL", this.name, name);

            return null;
        }

        public Object Get(string name, string type, bool warnIfMissing = true)
        {
            for (int i = 0; i < m_Objects.Count; ++i) {
                var o = m_Objects[i];
                var otype = o.GetType();
                if (o.name == name && (string.IsNullOrEmpty(type) || type == otype.FullName)) {
                    return o;
                }
            }
            if (warnIfMissing) LogMgr.W("[{0}]Get {1}<{2}> = NULL", this.name, name, type);
            return null;
        }

        public Object Get(string name, System.Type type, bool warnIfMissing = true)
        {
            for (int i = 0; i < m_Objects.Count; ++i) {
                var o = m_Objects[i];
                if (o.name == name && (type == null || type.IsInstanceOfType(o))) {
                    return o;
                }
            }
            if (warnIfMissing) LogMgr.W("[{0}]Get {1}<{2}> = NULL", this.name, name, type);
            return null;
        }

        public T Get<T>(string name, bool warnIfMissing = true) where T : Object
        {
            var type = typeof(T);
            return Get(name, type, warnIfMissing) as T;
        }

        public static ObjectLibrary Load(string path)
        {
            var go = AssetLoader.Instance.Load(typeof(GameObject), path) as GameObject;
            if (go == null) return null;

            return go.GetComponent(typeof(ObjectLibrary)) as ObjectLibrary;
        }

        public static Object Load(string path, string name)
        {
            var go = AssetLoader.Instance.Load(typeof(GameObject), path) as GameObject;
            if (go == null) return null;

            var lib = go.GetComponent(typeof(ObjectLibrary)) as ObjectLibrary;
            if (lib == null) return null;

            return lib.Get(name);
        }
    }
}

