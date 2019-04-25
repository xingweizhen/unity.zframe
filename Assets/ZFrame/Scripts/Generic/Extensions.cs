using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenericExtensions
{
    public static TValue GetValue<TKey, TValue>(this IDictionary<Boxing<TKey>, TValue> self, TKey key, TValue def = default(TValue)) where TKey : struct where TValue : class
    {
        var boxingKey = Boxing<TKey>.Key;
        boxingKey.Value = key;

        TValue value;
        return self.TryGetValue(boxingKey, out value) ? value : def;
    }

    public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, Boxing<TValue>> self, TKey key, TValue def = default(TValue)) where TKey : class where TValue : struct
    {
        Boxing<TValue> value;
        if (self.TryGetValue(key, out value)) {
            return value.Value;
        }
        return def;
        
    }

    public static TValue GetValue<TKey, TValue>(this IDictionary<Boxing<TKey>, Boxing<TValue>> self, TKey key, TValue def = default(TValue)) where TKey : struct where TValue : struct
    {
        var boxingKey = Boxing<TKey>.Key;
        boxingKey.Value = key;
        
        Boxing<TValue> value;
        if (self.TryGetValue(boxingKey, out value)) {
            return value.Value;
        }
        return def;
    }

    public static bool Remove<TKey, TValue>(this IDictionary<Boxing<TKey>, TValue> self, TKey key) where TKey : struct
    {
        var boxingKey = Boxing<TKey>.Key;
        boxingKey.Value = key;
        return self.Remove(boxingKey);
    }
}
