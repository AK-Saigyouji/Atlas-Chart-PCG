using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;
using AKSaigyouji.Modules;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Base class for chart modules fed into the atlas system. Represents a local block of a greater map, with content 
    /// markers.
    /// </summary>
    public abstract class RawChart : Module
    {
        protected const string FILE_NAME = "Chart";
        protected const string ASSET_PATH = MODULE_ASSET_PATH + "Charts/";

        public abstract Map Map { get; }
        public abstract MetaData MetaData { get; }
        public abstract IEnumerable<RawMarker> Markers { get; }
    } 
}