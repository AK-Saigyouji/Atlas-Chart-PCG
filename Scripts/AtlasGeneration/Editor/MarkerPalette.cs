/* A marker palette is essentially a soft schema. It defines categories of markers, as well as colours for those markers,
 which will affect their appearance in the chart builder window. Each category will correspond to an item in the context
 menu when creating markers. Presets are similar to categories, but more specialized: each preset belongs to a category,
 but also defines preset values for size and metadata. These presets are intended to alleviate the otherwise repetitive
 workflow of having to configure each marker as it's placed.
 
  e.g. one can have a category Enemy, and three presets "Single, Group, and Boss". The single is configured with
 size 1 by 1, the group with size 3 by 3, and the boss with size 2 by 2. Furthermore, they all have metadata to specify
 modifiers for difficulty and treasure, as well as to represent which monster classes they should be pulled from. */

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji.AtlasGeneration
{
    [CreateAssetMenu(fileName = "Marker Palette", menuName = menuPath + "Default")]
    public sealed class MarkerPalette : MarkerPaletteModule
    {
        [SerializeField] List<MarkerCategory> categories;
        [SerializeField] List<MarkerPreset> presets;
        [SerializeField] Color defaultColor;

        void OnEnable()
        {
            if (categories == null)
            {
                defaultColor = MarkerCategory.DefaultColor;
                categories = new List<MarkerCategory>() { new MarkerCategory() };
                presets = new List<MarkerPreset>(0);
            }
        }

        public override Color DefaultColor { get { return defaultColor; } }

        public static MarkerPalette DefaultPalette
        {
            get
            {
                var palette = CreateInstance<MarkerPalette>();
                palette.name = "Default Palette";
                palette.categories = new List<MarkerCategory>
                {
                    new MarkerCategory(new Color(1f, 0f, 0f, 0.4f), "Enemy"),
                    new MarkerCategory(new Color(0f, 1f, 0f, 0.4f), "Treasure"),
                    new MarkerCategory(new Color(0f, 0f, 1f, 0.4f), "Environment"),
                };
                palette.defaultColor = MarkerCategory.DefaultColor;
                return palette;
            }
        }

        public override IEnumerable<MarkerCategory> GetCategories()
        {
            return categories;
        }

        public override IEnumerable<MarkerPreset> GetPresets()
        {
            return presets;
        }

        public void AddNewPreset()
        {
            presets.Add(new MarkerPreset());
        }

        public void AddNewCategory()
        {
            categories.Add(new MarkerCategory(defaultColor));
        }

        public void DuplicatePresetAtIndex(int index)
        {
            var preset = presets[index];
            var presetCopy = new MarkerPreset(preset);
            presets.Insert(index + 1, presetCopy);
        }

        public void DeletePresetAtIndex(int index)
        {
            if (index >= presets.Count || index < 0)
                throw new ArgumentOutOfRangeException();

            presets.RemoveAt(index);
        }

        public void DeleteCategoryAtIndex(int index)
        {
            if (index >= categories.Count || index < 0)
                throw new ArgumentOutOfRangeException();

            if (categories.Count != 1) // Must be at least one category at all times.
                categories.RemoveAt(index);
        }
    } 
}