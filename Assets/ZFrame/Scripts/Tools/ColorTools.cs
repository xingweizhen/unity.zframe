using UnityEngine;
using System.Collections;

public static class ColorTools 
{
    public static Color ToColor(this string hexColor)
    {
        var strHex = hexColor.Replace("#", "0x");
        var icolor = (uint)System.Convert.ToInt32(strHex, 16);
        if (icolor <= 0xffffff) {
            icolor <<= 8;
            icolor |= 0xff;
        }
        return icolor.ToColor();
    }

    public static Color ToColor(this uint colorValue)
    {
        var r = (colorValue >> 24);
        var g = (colorValue >> 16) & 255;
        var b = (colorValue >> 8) & 255;
        var a = colorValue & 255;
        return new Color32((byte)r, (byte)g, (byte)b, (byte)a);
    }

	public static string ToHexColor(this Color color)
	{
		var r = Mathf.RoundToInt(color.r * 255);
		var g = Mathf.RoundToInt(color.g * 255);
		var b = Mathf.RoundToInt(color.b * 255);
		var a = Mathf.RoundToInt(color.a * 255);
		return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a);
	}

    public static Color ConvertToColor(this object self)
    {
        if (self is Color) return (Color)self;
        if (self is string) {
            var str = (string)self;
#if UNITY_5
            Color color;
            if (ColorUtility.TryParseHtmlString(str, out color)) {
                return color;
            }
#endif
            LogMgr.W("Can't parse \"{0}\" to a UnityEngine.Color. Fallback to Color.clear.", str);
            return Color.clear;
        }

        throw new System.InvalidCastException(string.Format("Can't cast {0} to UnityEngine.Color", self));
    }
}
