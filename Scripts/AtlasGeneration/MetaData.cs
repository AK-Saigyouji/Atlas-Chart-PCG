/* The motivation behind this class was to provide a highly flexible way to attach arbitrary extra information about 
 * markers and charts in the chart editor. The intention is that it should be operate like a dictionary, but must be 
 * serializable (Unity's serialization system does not work on dictionaries). Some effort has also been made to 
 * optimize for serialization speed and memory use, which is why we pack all the strings together into a single 
 * string before and serialize that, instead of serializing (e.g.) two string arrays. A project making extensive use 
 * of the chart system could end up storing thousands of markers (and thus instances of metadata).
 *
 * A downside of the chosen approach is that using strings for everything is relatively error prone in a statically
 * typed language like C#. An alternative that was considered was to use late binding to allow users to define their 
 * own chart and marker types with exactly the extra fields they needed. But what this approach gains in type safety
 * and intellisense, it loses in adaptability. One is extremely likely to add and remove types of metadata over the 
 * course of a project, and it's much easier to add/remove string/string key-value pairs than to restructure class
 * hierarchies.*/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace AKSaigyouji.AtlasGeneration
{
    [Serializable]
    public sealed class MetaData : ISerializationCallbackReceiver
    {
        List<KeyValuePair<string, string>> metaData;

        [SerializeField] string _serializedData; // used solely for serialization

        public int Count { get { return metaData.Count; } }

        public MetaData()
        {
            metaData = new List<KeyValuePair<string, string>>(0);
        }

        public MetaData(MetaData toCopy)
        {
            metaData = toCopy.Unpack().ToList();
        }

        public IEnumerable<KeyValuePair<string, string>> Unpack()
        {
            // Empty meta data lists are likely to be common. By having all empty metadata return the same empty
            // enumerable, we cut down on the creation of a potentially huge number of empty lists. 
            return Count == 0 ? Enumerable.Empty<KeyValuePair<string, string>>() : metaData.AsReadOnly();
        }

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                    ThrowKeyException();

                var keyValuePair = metaData.FirstOrDefault(pair => pair.Key == key);
                if (keyValuePair.Key != null)
                {
                    return keyValuePair.Value;
                }
                else
                {
                    throw new ArgumentException("Key not found");
                }
            }
            set
            {
                if (string.IsNullOrEmpty(key))
                    ThrowKeyException();

                var newPair = new KeyValuePair<string, string>(key, value);
                for (int i = 0; i < metaData.Count; i++)
                {
                    if (metaData[i].Key == key)
                    {
                        metaData[i] = newPair;  
                        return;
                    }
                }
                metaData.Add(newPair);
            }
        }
       
        /// <summary>
        /// Does this chart have metadata corresponding to this key?
        /// </summary>
        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                ThrowKeyException();

            return metaData.Any(pair => pair.Key == key);
        }

        public void RemoveMetaData(string key)
        {
            if (string.IsNullOrEmpty(key))
                ThrowKeyException();

            for (int i = 0; i < metaData.Count; i++)
            {
                if (metaData[i].Key == key)
                {
                    metaData.RemoveAt(i);
                    return;
                }
            }
        }

        // deserialization: takes a string the form "a1:b1,a2:b2,...,an:bn" and returns a list of key value pairs
        // where the key is ai, and the value is bi. 
        static List<KeyValuePair<string, string>> UnpackRawMetaData(string rawMetaData)
        {
            if (string.IsNullOrEmpty(rawMetaData))
                return new List<KeyValuePair<string, string>>();

            string[] rawKeyValuePairs = rawMetaData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var table = new List<KeyValuePair<string, string>>(rawKeyValuePairs.Length);
            for (int i = 0; i < rawKeyValuePairs.Length; i++)
            {
                var rawPair = rawKeyValuePairs[i];
                int indexOfSeparator = rawPair.IndexOf(':');
                if (indexOfSeparator == -1)
                {
                    throw new InvalidOperationException
                        (string.Format("Invalid chart metadata. Must be of the form a:b,c:d,e:f etc. Actual: {0}", rawMetaData));
                }
                string key = rawPair.Substring(0, indexOfSeparator);
                string value = rawPair.Substring(indexOfSeparator + 1, rawPair.Length - indexOfSeparator - 1);
                table.Add(new KeyValuePair<string, string>(key.Trim(), value.Trim()));
            }
            return table;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            var sb = new StringBuilder();
            foreach (var pair in metaData)
            {
                if (sb.Length > 0)
                {
                    sb.Append(',');
                }
                sb.AppendFormat("{0}:{1}", pair.Key, pair.Value);
            }
            _serializedData = sb.ToString();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            metaData = UnpackRawMetaData(_serializedData);
        }

        void ThrowKeyException()
        {
            throw new ArgumentException("Key is invalid. Must be a valid, non-empty string");
        }

        void ThrowValueException()
        {
            throw new ArgumentException("Value is invalid. Must be a valid, non-empty string.");
        }
    } 
}