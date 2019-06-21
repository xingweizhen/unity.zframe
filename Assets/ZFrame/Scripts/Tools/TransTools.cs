using UnityEngine;
using System.Collections;

public static class TransTools
{

    public static string GetHierarchy(this Component self, Transform root = null)
    {
        var trans = self.transform;

        if (trans == root) return string.Empty;

        var obj = trans;
        string path = obj.name;
        obj = obj.parent;
        while (obj && obj != root) {
            path = obj.name + "/" + path;
            obj = obj.parent;
        }

        return path;
    }

    private static void SetEnable(Component com, bool enabled)
    {
        var bhr = com as Behaviour;
        if (bhr) {
            bhr.enabled = enabled;
            return;
        }

        var cld = com as Collider;
        if (cld) {
            cld.enabled = enabled;
        }
    }

    public static void SetEnable(this Component self, System.Type comType, bool enabled)
    {
        SetEnable(self.GetComponent(comType), enabled);
    }

    public static void SetEnable(this GameObject self, System.Type comType, bool enabled)
    {
        SetEnable(self.GetComponent(comType), enabled);
    }

    //[XLua.LuaCallCSharp]
    public static bool CheckInside(this RectTransform self, RectTransform rect, out Vector2 newPos)
    {
        var scrSiz = rect.rect.size / 2;
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rect, self);
        newPos = self.anchoredPosition;
        var anchoredPos = newPos;

        var v2Max = bounds.max;
        var v2Min = bounds.min;

        if (v2Max.x > scrSiz.x) {
            anchoredPos.x -= (v2Max.x - scrSiz.x);
        }

        if (v2Max.y > scrSiz.y) {
            anchoredPos.y -= (v2Max.y - scrSiz.y);
        }

        if (v2Min.x < -scrSiz.x) {
            anchoredPos.x -= (scrSiz.x + v2Min.x);
        }

        if (v2Min.y < -scrSiz.y) {
            anchoredPos.y -= (scrSiz.y + v2Min.y);
        }

        if (newPos != anchoredPos) {
            newPos = anchoredPos;
            return false;
        }

        return true;
    }

    /// <summary>
    /// 把self的位置对齐到target
    /// </summary>
    public static void Overlay(this Transform self, Transform target)
    {
        if (target) {
            Camera tarCam = target.gameObject.FindCameraForLayer();
            self.Overlay(target.position, tarCam, self.position.z);
        }
    }

    public static void Overlay(this Transform self, Vector3 target, Camera tarCam, float depthOfView)
    {
        var rect = self as RectTransform;
        if (rect) {
            Camera selfCam = self.gameObject.FindCameraForLayer();
            Vector3 pos = tarCam.WorldToScreenPoint(target);

            Vector2 anchoredPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(self.parent as RectTransform, pos, selfCam, out anchoredPos)) {
                rect.anchoredPosition = anchoredPos;
            }
        } else {
            Camera selfCam = self.gameObject.FindCameraForLayer();
            var pos = tarCam.WorldToViewportPoint(target);
            pos.z = depthOfView;

            self.position = selfCam.ViewportToWorldPoint(pos);
        }
    }

    private static Rect ToWorldRect(this RectTransform self, Camera cam)
    {
        var rectW = self.rect.width;
        var rectH = self.rect.height;
        var screenPos = RectTransformUtility.WorldToScreenPoint(cam, self.position);
        float minX = screenPos.x - self.rect.width * self.pivot.x;
        float minY = screenPos.y - self.rect.height * self.pivot.y;

        return new Rect(minX, minY, rectW, rectH);
    }

    public static bool Overlabs(this RectTransform self, RectTransform target, Camera cam)
    {
        var selfRect = self.ToWorldRect(cam);
        var targetRect = target.ToWorldRect(cam);
        return selfRect.Overlaps(targetRect);
    }

    public static Transform FindByName(this Transform self, string name, bool includeInactive = true)
    {
        var ret = self.Find(name);
        if (ret && (includeInactive || ret.gameObject.activeInHierarchy)) return ret;

        ret = null;
        for (int i = 0; i < self.childCount; ++i) {
            var t = self.GetChild(i);
            ret = t.FindByName(name, includeInactive);
            if (ret) break;
        }
        return ret;
    }
}
