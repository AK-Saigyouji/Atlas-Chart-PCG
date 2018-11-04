using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.AtlasGeneration;

namespace AKSaigyouji.AtlasGeneration
{
    public interface IContentStrategy 
    {
        /// <summary>
        /// Generates content for the given atlas. In general, one content strategy need not handle the entire
        /// atlas. If using multiple strategies, then used markers should be marked as Used.
        /// </summary>
        void GenerateContent(Atlas atlas);
    } 
}