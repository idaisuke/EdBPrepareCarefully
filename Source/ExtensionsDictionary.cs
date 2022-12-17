using System.Collections.Generic;

namespace EdB.PrepareCarefully;

public static class ExtensionsDictionary {
    public static void AddToListOfValues<K, V>(this Dictionary<K, List<V>> dictionary, K key, V value) {
        List<V> list;
        if (!dictionary.TryGetValue(key, out list)) {
            list = new List<V>();
            dictionary.Add(key, list);
        }

        list.Add(value);
    }

    public static V GetOrDefault<K, V>(this Dictionary<K, V> dictionary, K key) {
        if (dictionary.TryGetValue(key, out var value)) {
            return value;
        }

        return default;
    }
}
