using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    using Asset;
    public class SpriteLoader : MonoBehaviour
    {
        public bool nativeSizeOnLoaded;
        public string assetPath;

        private void OnEnable()
        {
            var loader = AssetLoader.Instance;
            if (loader) {
                var img = GetComponent<Image>();
                if (img) {
                    loader.LoadAsync(typeof(Sprite), assetPath, LoadMethod.Cache, OnSpriteLoaded, null);
                    return;
                }

                var raw = GetComponent<RawImage>();
                if (raw) {
                    loader.LoadAsync(typeof(Texture), assetPath, LoadMethod.Cache, OnTextureLoaded, null);
                    return;
                }
            } else {
#if UNITY_EDITOR
                string assetbundleName, assetName;
                AssetLoader.GetAssetpath(assetPath, out assetbundleName, out assetName);
                var paths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetbundleName, assetName);
                if (paths == null || paths.Length == 0) return;

                var img = GetComponent<Image>();
                if (img) {
                    var obj = UnityEditor.AssetDatabase.LoadAssetAtPath(paths[0], typeof(Sprite));
                    OnSpriteLoaded(assetPath, obj, null);
                    return;
                }

                var raw = GetComponent<RawImage>();
                if (raw) {
                    var obj = UnityEditor.AssetDatabase.LoadAssetAtPath(paths[0], typeof(Texture));
                    OnTextureLoaded(assetPath, obj, null);
                    return;
                }
#endif
            }
        }

        private void OnSpriteLoaded(string a, object o, object p)
        {
            var img = GetComponent<Image>();
            var sp = o as Sprite;
            if (img && sp) {
                img.overrideSprite = sp;
                if (nativeSizeOnLoaded) img.SetNativeSize();
            }
        }

        private void OnTextureLoaded(string a, object o, object p)
        {
            var raw = GetComponent<RawImage>();
            var tex = o as Texture;
            if (raw && tex) {
                raw.texture = tex;
                if (nativeSizeOnLoaded) raw.SetNativeSize();
            }
        }

        private void OnDestroy()
        {
            if (AssetLoader.Instance) {
                AssetLoader.Instance.Unload(assetPath, false);
            }
        }
    }
}
