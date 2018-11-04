using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.AtlasGeneration
{
    // this exists solely to apply the can edit multiple objects attribute
    [CustomEditor(typeof(ChartStatic))]
    [CanEditMultipleObjects]
    public sealed class ChartStaticEditor : Editor
    {
        
    }
}