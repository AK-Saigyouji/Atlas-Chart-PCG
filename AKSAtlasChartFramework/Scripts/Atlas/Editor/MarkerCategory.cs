using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji.AtlasGeneration
{
    [Serializable]
    public sealed class MarkerCategory
    {
        public string name;
        public Color color;

        const string DEFAULT_CATEGORY_NAME = "Category Name";
        const float R_DEFAULT = 0.7f;
        const float G_DEFAULT = 0.7f;
        const float B_DEFAULT = 0.7f;
        const float A_DEFAULT = 0.6f;

        public static Color DefaultColor { get { return new Color(R_DEFAULT, G_DEFAULT, B_DEFAULT, A_DEFAULT); } }

        public MarkerCategory()
        {
            name = DEFAULT_CATEGORY_NAME;
            color = DefaultColor;
        }

        public MarkerCategory(Color color, string name = DEFAULT_CATEGORY_NAME)
        {
            this.name = name;
            this.color = color;
        }
    } 
}