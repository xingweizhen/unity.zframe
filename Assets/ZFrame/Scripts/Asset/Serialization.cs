using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
// List<T>
[System.Serializable]
public class SerializeList<T>
{
	[SerializeField] List<T> target;

	public List<T> ToList()
	{
		return target;
	}

	public SerializeList(List<T> target)
	{
		this.target = target;
	}
}

// Dictionary<TKey, TValue>
[System.Serializable]
public class SerializeDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
	[SerializeField] List<TKey> keys;
	[SerializeField] List<TValue> values;

	Dictionary<TKey, TValue> target;

	public TValue this[TKey key] {
		get { return target[key]; }
		set { target[key] = value; }
	}

	public Dictionary<TKey, TValue> ToDictionary()
	{
		return target;
	}

	public SerializeDictionary(Dictionary<TKey, TValue> target = null)
	{
		this.target = target ?? new Dictionary<TKey, TValue>();
	}

	public void Add(TKey key, TValue value)
	{
		target.Add(key, value);
	}

	public void OnBeforeSerialize()
	{
		keys = new List<TKey>(target.Keys);
		values = new List<TValue>(target.Values);
	}

	public void OnAfterDeserialize()
	{
		var count = System.Math.Min(keys.Count, values.Count);
		target = new Dictionary<TKey, TValue>(count);
		for (var i = 0; i < count; ++i) {
			target.Add(keys[i], values[i]);
		}
	}
}
#endif
