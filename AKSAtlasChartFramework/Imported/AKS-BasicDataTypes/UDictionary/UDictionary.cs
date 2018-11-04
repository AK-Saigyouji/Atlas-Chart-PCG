/* This offers a serializable generic class with dictionary semantics, intended for use with the inspector. Using a real
 * dictionary internally would introduce a large number of problems and edge cases: when the dictionary resizes, the hashes
 * would get recomputed and items would be reordered: this means that when an element is added in the inspector, all existing
 * items might spontaneously reorganize themselves. Instead, we use a simpler approach with two lists. This makes serialization
 * trivial. And for most cases where one wants to work with the inspector, i.e. with a small number of items, the performance
 * of lists is comparable or even superior to a proper dictionary. 
 *
 * In cases where one needs a large number of items and wants to work through the inspector, then a custom solution would be required.
 * This class merely handles the common use case of small mappings in the inspector.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji
{
    // This non-generic base class enables us to get around an issue with trying to write a property drawer for
    // a generic class. By writing a drawer for the non-generic UDictionary, we can push down the drawer to all
    // implementations of UDictionary<TKey, TValue>, even though we can't write a drawer for UDictionary<TKey, TValue>

    /// <summary>
    /// Exists solely for editor scripting purposes. Do not use this class directly.
    /// </summary>
    [Serializable]
    public abstract class UDictionary { }

    /// <summary>
    /// A dictionary that works with Unity's serialization. Note that the types of both the key and value must be
    /// serializable by Unity to work. Also note that it must be subtyped with concrete type values passed in for 
    /// both TKey and TValue, since Unity's serialization does not like generics: this is why it's marked abstract.
    /// Note that the dictionary is backed by a list - it thus has the semantics of a dictionary, but not the performance.
    /// This is fine for smaller use cases (less than 20 items) but it will not scale for large numbers of items.
    /// </summary>
    [Serializable]
    public abstract class UDictionary<TKey, TValue> : UDictionary, IDictionary<TKey, TValue>
    {
        [SerializeField] List<TKey> keys;
        [SerializeField] List<TValue> values;

        public IEnumerable<TKey> Keys { get { return keys; } }
        public IEnumerable<TValue> Values { get { return values; } }

        public int Count { get { return keys.Count; } }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys { get { return keys.AsReadOnly(); } }
        ICollection<TValue> IDictionary<TKey, TValue>.Values { get { return values.AsReadOnly(); } }

        public bool IsReadOnly { get { return false; } }

        public UDictionary()
        {
            keys = new List<TKey>();
            values = new List<TValue>();
        }

        public UDictionary(int capacity)
        {
            keys = new List<TKey>(capacity);
            values = new List<TValue>(capacity);
        }

        public UDictionary(IEnumerable<KeyValuePair<TKey, TValue>> entries)
        {
            keys = entries.Select(pair => pair.Key).ToList();
            values = entries.Select(pair => pair.Value).ToList();
        }

        public TValue this[TKey key]
        {
            get
            {
                int index = keys.IndexOf(key);
                if (index == -1)
                {
                    throw new KeyNotFoundException("Key not found: " + key.ToString());
                }
                return values[index];
            }
            set
            {
                int index = keys.IndexOf(key);
                if (index == -1)
                {
                    keys.Add(key);
                    values.Add(value);
                }
                else
                { 
                    values[index] = value;
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (keys.Contains(key))
            {
                throw new ArgumentException("Key already exists: " + key.ToString());
            }
            keys.Add(key);
            values.Add(value);
        }
        
        public bool Remove(TKey key)
        {
            int index = keys.IndexOf(key);
            if (index == -1)
            {
                return false;
            }
            keys.RemoveAt(index);
            values.RemoveAt(index);
            return true;
        }

        public bool ContainsKey(TKey key)
        {
            return keys.Contains(key);
        }

        public bool ContainsValue(TValue value)
        {
            return values.Contains(value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return Contains(item.Key, item.Value);
        }

        public bool Contains(TKey key, TValue value)
        {
            int index = keys.IndexOf(key);
            return index != -1 && values[index].Equals(value);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < keys.Count; i++)
            {
                yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array.Length < Count + arrayIndex)
            {
                throw new ArgumentException("Cannot fit values into array.");
            }
            for (int i = 0; i < keys.Count; i++)
            {
                TKey key = keys[i];
                TValue value = values[i];
                array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            int index = keys.IndexOf(item.Key);
            if (index != -1 && values[index].Equals(item.Value))
            {
                keys.RemoveAt(index);
                values.RemoveAt(index);
                return true;
            }
            else
            {
                return false;
            }
        }
    } 
}