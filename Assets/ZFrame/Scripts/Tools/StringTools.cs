using System.Collections;
using System.Collections.Generic;

public static class StringTools
{
    public static bool StartsWith(this string self, char c)
    {
        return c == self[0];
    }

    public static bool EndsWith(this string self, char c)
    {
        return c == self[self.Length - 1];
    }

    public static bool OrdinalEndsWith(this string self, string other)
    {
        return self.EndsWith(other, System.StringComparison.Ordinal);
    }

    public static bool OrdinalIgnoreCaseEndsWith(this string self, string other)
    {
        return self.EndsWith(other, System.StringComparison.OrdinalIgnoreCase);
    }

    public static bool OrdinalStartsWith(this string self, string other)
    {
        return self.StartsWith(other, System.StringComparison.Ordinal);
    }

    public static bool OrdinalIgnoreCaseStartsWith(this string self, string other)
    {
        return self.StartsWith(other, System.StringComparison.OrdinalIgnoreCase);
    }

}
