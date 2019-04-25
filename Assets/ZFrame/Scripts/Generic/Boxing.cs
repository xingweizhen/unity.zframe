using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boxing<T>
{
    public T Value { get; set; }
    public Boxing(T value) { this.Value = value; }
    public override int GetHashCode() { return Value.GetHashCode(); }
    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Boxing<T>)) {
            return false;
        }
        return Value.Equals(((Boxing<T>)obj).Value);
    }

    public static implicit operator T(Boxing<T> boxing)
    {
        return boxing.Value;
    }
    public static implicit operator Boxing<T>(T value)
    {
        return new Boxing<T>(value);
    }

    public readonly static Boxing<T> Key = new Boxing<T>(default(T));
    public static Boxing<T> Out;
}

