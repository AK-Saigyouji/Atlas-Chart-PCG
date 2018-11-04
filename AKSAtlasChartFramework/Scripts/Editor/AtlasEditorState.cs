using AKSaigyouji.EditorScripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    public sealed class AtlasEditorState : EditorState
    {
        public TagDrawTable TagTable;

        const string ASSET_NAME = "AKSAtlasEditorState";

        public static AtlasEditorState Load()
        {
            return Load<AtlasEditorState>(ASSET_NAME);
        }
    } 
}