using UnityEngine;
using System.Collections;

public static class VectorTools {

    public static float HorizontalAngle(this Vector3 self)
    {
        return Mathf.Atan2(self.x, self.z) * Mathf.Rad2Deg;
    }

    public static Vector2 Round(this Vector2 self, float precision = 1)
    {
        return new Vector2(
            Mathf.Round(self.x * precision) / precision,
            Mathf.Round(self.y * precision) / precision);
    }

    public static Vector3 Round(this Vector3 self, float precision = 1)
    {
        return new Vector3(
            Mathf.Round(self.x * precision) / precision,
            Mathf.Round(self.y * precision) / precision, 
            Mathf.Round(self.z * precision) / precision);
    }

    public static bool ViewportRaycast(this Camera self, Vector3 point, float maxDistance, int layer, out RaycastHit hit)
    {
        Ray ray = self.ViewportPointToRay(point);
        return Physics.Raycast(ray, out hit, maxDistance, layer);
    }

    public static RaycastHit[] ViewportRaycast(this Camera self, Vector3 point, float maxDistance, int layer)
    {
        Ray ray = self.ViewportPointToRay(point);
        return Physics.RaycastAll(ray, maxDistance, layer);
    }

    public static bool ScreenRaycast(this Camera self, Vector3 point, float maxDistance, int layer, out RaycastHit hit)
    {
        Ray ray = self.ScreenPointToRay(point);
        return Physics.Raycast(ray, out hit, maxDistance, layer);
    }

    public static RaycastHit[] ScreenRaycastAll(this Camera self, Vector3 point, float maxDistance, int layer)
    {
        Ray ray = self.ScreenPointToRay(point);
        return Physics.RaycastAll(ray, maxDistance, layer);
    }

    public static bool IsNaNOrInfinity(this Vector2 self)
    {
        return float.IsNaN(self.x) || float.IsInfinity(self.x)
            || float.IsNaN(self.y) || float.IsInfinity(self.y);
    }

    public static bool IsNaNOrInfinity(this Vector3 self)
    {
        return float.IsNaN(self.x) || float.IsInfinity(self.x)
            || float.IsNaN(self.y) || float.IsInfinity(self.y)
            || float.IsNaN(self.z) || float.IsInfinity(self.z);
    }

    public static bool IsNaNOrInfinity(this Quaternion self)
    {
        return float.IsNaN(self.x) || float.IsInfinity(self.x)
            || float.IsNaN(self.y) || float.IsInfinity(self.y)
            || float.IsNaN(self.z) || float.IsInfinity(self.z)
            || float.IsNaN(self.w) || float.IsInfinity(self.w);
    }

    public static Vector2 SetX(this Vector2 self, float value)
    {
        var ret = self;
        ret.x = value;
        return ret;
    }

    public static Vector2 SetY(this Vector2 self, float value)
    {
        var ret = self;
        ret.y = value;
        return ret;
    }

    public static Vector3 SetX(this Vector3 self, float value)
    {
        var ret = self;
        ret.x = value;
        return ret;
    }

    public static Vector3 SetY(this Vector3 self, float value)
    {
        var ret = self;
        ret.y = value;
        return ret;
    }

    public static Vector3 SetZ(this Vector3 self, float value)
    {
        var ret = self;
        ret.z = value;
        return ret;
    }
}
