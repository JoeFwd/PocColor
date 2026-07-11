using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PocColor.Config;

public class Map<TKey, TValue> : ConcurrentDictionary<TKey, TValue> where TValue : class
{
	public new TValue this[TKey key]
	{
		get
		{
			if (key == null || !ContainsKey(key))
			{
				return null;
			}
			return base[key];
		}
		set
		{
			if (key != null)
			{
				if (!ContainsKey(key))
				{
					TryAdd(key, value);
				}
				else
				{
					base[key] = value;
				}
			}
		}
	}

	public override string ToString()
	{
		string text = "[";
		using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				text += $"Key = {current.Key}, Value = {current.Value}";
				text += ", ";
			}
		}
		return text + "]";
	}
}
