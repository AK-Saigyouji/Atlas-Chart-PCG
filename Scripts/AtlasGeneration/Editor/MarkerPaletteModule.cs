using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Modules;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Base class for editor-only marker palettes to customize the available marker types in the chart builder.
    /// Derive from this to implement your own type of marker palette.
    /// </summary>
    public abstract class MarkerPaletteModule : Module
    {
        protected const string menuPath = MODULE_ASSET_PATH + "Marker Palette/";

        public abstract IEnumerable<MarkerCategory> GetCategories();
        public abstract IEnumerable<MarkerPreset> GetPresets();
        public abstract Color DefaultColor { get; }

        public Color GetColor(string categoryName)
        {
            var category = GetCategories().FirstOrDefault(cat => cat.name == categoryName);
            return category == null ? DefaultColor : category.color;
        }
    } 
}