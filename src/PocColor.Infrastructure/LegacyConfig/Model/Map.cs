using System.Collections.Concurrent;

namespace PocColor.Infrastructure.LegacyConfig.Model;

public class Map<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    where TValue : class
{
    public new TValue this[TKey key]
    {
        get
        {
            return key != null && TryGetValue(key, out TValue value)
                ? value
                : null;
        }
        set
        {
            if (key != null)
            {
                base[key] = value;
            }
        }
    }
}
