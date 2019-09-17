using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
	using Tween;

    public static class FadeTool
    {
        public static object DOFade(GameObject go, FadeGroup fadeDir, bool reset, bool forward = true)
        {
            object ret = null;
            if (go) {
                var list = ListPool<Component>.Get();
                go.GetComponents(typeof(IFading), list);
                float duration = 0;
                for (int i = 0; i < list.Count; ++i) {
                    var com = list[i];
					var fad = com as IFading;
                    if (fadeDir == fad.group) {
                        if (fad.DOFade(reset, forward)) {
                            var fxDuration = fad.lifetime;
                            if (duration < fxDuration) {
                                duration = fxDuration;
                                ret = fad.tweener;
                            }
                        }
                    }
                }
                ListPool<Component>.Release(list);
            }
            return ret;
        }

        public static object DOFade(GameObject go, bool reset, bool forward = true)
        {
            object ret = null;
            if (go) {
                var list = ListPool<Component>.Get();
                go.GetComponents(typeof(IFading), list);
                float duration = 0;
                for (int i = 0; i < list.Count; ++i) {
                    var com = list[i];
                    var fad = com as IFading;
                    if (fad.DOFade(reset, forward)) {
                        var fxDuration = fad.lifetime;
                        if (duration < fxDuration) {
                            duration = fxDuration;
                            ret = fad.tweener;
                        }
                    }
                }
                ListPool<Component>.Release(list);
            }
            return ret;
        }
    }
}
