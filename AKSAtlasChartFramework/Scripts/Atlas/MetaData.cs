using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Collections;

namespace AKSaigyouji.AtlasGeneration
{
    [Serializable]
    public sealed class MetaData : UDictionary<string, string>
    {
        public MetaData() : base()
        {

        }

        public MetaData(IEnumerable<KeyValuePair<string, string>> data) : base(data)
        {

        }

        public MetaData(IEnumerable<string> keys) : base(keys.Select(key => new KeyValuePair<string, string>(key, "")))
        {

        }

        public MetaData(string key) : base()
        {
            Add(key, "");
        }

        public override string ToString()
        {
            return "MetaData object: " + string.Join(", ", this.Select(pair => pair.ToString()).ToArray());
        }
    } 
}