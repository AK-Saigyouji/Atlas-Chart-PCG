using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Object responsible for mapping tags to tag drawers.
    /// </summary>
    [CreateAssetMenu(fileName = "Tag Drawer Table", menuName = "AKSaigyouji/Maze Generators/Tag Drawer Table")]
    public sealed class TagDrawTable : ScriptableObject
    {
        [Tooltip("Any tag not matching any drawer will match this one.")]
        [SerializeField] TagDrawer defaultDrawer;

        [SerializeField] DrawTable drawers;

        public const string DEFAULT_NAME = "__DEFAULT_TAG_TABLE__";

        /// <summary>
        /// Get the tag drawer for this tag. Will return a drawer for any tag.
        /// </summary>
        public TagDrawer this[string key]
        {
            get
            {
                Assert.IsNotNull(key);
                return drawers.ContainsKey(key) ? drawers[key] : defaultDrawer;
            }
        }

        /// <summary>
        /// Does this table have a specific drawer for this tag? If not, will
        /// return the default drawer for this key.
        /// </summary>
        public bool HasDrawerForKey(string key)
        {
            return drawers.ContainsKey(key);
        }

        // Subclass to permit appearance in inspector.
        [Serializable]
        sealed class DrawTable : UDictionary<string, TagDrawer>
        {
            public DrawTable(params KeyValuePair<string, TagDrawer>[] drawers) : base(drawers)
            {

            }
        }
    } 
}