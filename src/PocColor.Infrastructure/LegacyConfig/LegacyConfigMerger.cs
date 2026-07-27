using System;
using System.Collections;
using System.Reflection;
using PocColor.Infrastructure.LegacyConfig.Model;

namespace PocColor.Infrastructure.LegacyConfig;

public static class LegacyConfigMerger
{
    public static T Merge<T>(T target, T source)
        where T : class
    {
        if (source == null)
        {
            return target;
        }

        if (target == null)
        {
            return source;
        }

        MergeObject(target, source);
        return target;
    }

    private static void MergeObject(object target, object source)
    {
        if (target == null || source == null)
        {
            return;
        }

        foreach (PropertyInfo property in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || !property.CanWrite)
            {
                continue;
            }

            object sourceValue = property.GetValue(source, null);
            if (sourceValue == null)
            {
                continue;
            }

            object targetValue = property.GetValue(target, null);
            Type propertyType = property.PropertyType;
            if (propertyType == typeof(bool))
            {
                property.SetValue(target, (bool)targetValue || (bool)sourceValue, null);
            }
            else if (IsScalar(propertyType))
            {
                property.SetValue(target, sourceValue, null);
            }
            else if (IsMap(propertyType))
            {
                MergeMap(property, target, targetValue, sourceValue);
            }
            else if (targetValue == null)
            {
                property.SetValue(target, sourceValue, null);
            }
            else
            {
                MergeObject(targetValue, sourceValue);
            }
        }
    }

    private static bool IsScalar(Type type)
    {
        return type.IsValueType || type == typeof(string) || type.IsArray;
    }

    private static bool IsMap(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Map<,>);
    }

    private static void MergeMap(PropertyInfo property, object owner, object targetMap, object sourceMap)
    {
        if (targetMap == null)
        {
            property.SetValue(owner, sourceMap, null);
            return;
        }

        MethodInfo containsKey = targetMap.GetType().GetMethod("ContainsKey");
        MethodInfo tryAdd = targetMap.GetType().GetMethod("TryAdd");
        PropertyInfo indexer = targetMap.GetType().GetProperty("Item");

        foreach (object entry in (IEnumerable)sourceMap)
        {
            Type entryType = entry.GetType();
            object key = entryType.GetProperty("Key").GetValue(entry, null);
            object sourceValue = entryType.GetProperty("Value").GetValue(entry, null);
            bool hasKey = (bool)containsKey.Invoke(targetMap, new[] { key });
            if (!hasKey)
            {
                tryAdd.Invoke(targetMap, new[] { key, sourceValue });
                continue;
            }

            object targetValue = indexer.GetValue(targetMap, new[] { key });
            if (targetValue == null || sourceValue == null || IsScalar(sourceValue.GetType()))
            {
                indexer.SetValue(targetMap, sourceValue, new[] { key });
            }
            else
            {
                MergeObject(targetValue, sourceValue);
            }
        }
    }
}
