using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Base class for an object that represents a package of serializable editor state.
    /// </summary>
    public abstract class EditorState : ScriptableObject
    {
        const string DEFAULT_PROPERTIES_PATH_PREFIX = "Assets/AKSAtlasChartFramework/Data/";

        protected static T Load<T>(string assetName) where T: EditorState
        {
            string path = DEFAULT_PROPERTIES_PATH_PREFIX + assetName;
            T save = AssetDatabase.LoadAssetAtPath<T>(path);
            if (save == null)
            {
                string[] guids = AssetDatabase.FindAssets(assetName + " t:EditorState");
                if (guids.Length == 0)
                {
                    Debug.LogErrorFormat("Failed to find {0} anywhere in project. Default path: {1}.", assetName, path);
                    return null;
                }
                else if (guids.Length > 1)
                {
                    Debug.LogWarningFormat("Found multiple {0} assets in project - possible duplicates?", assetName);
                }
                save = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            return save;
        }
    }
}