using FlatIronFields.Crops;
using FlatIronFields.Tractors;
using UnityEditor;
using UnityEngine;

namespace FlatIronFields.Game.Editor
{
    public static class MilestoneOneSetup
    {
        [MenuItem("Flat Iron Fields/Generate All Milestone 1 Assets")]
        public static void RunAll()
        {
            CropAssetGenerator.GenerateAll();
            SystemAssetGenerator.GenerateAll();
            PhaseTwoAssetGenerator.GenerateAll();
            SceneGenerator.GenerateAll();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Flat Iron Fields: Milestone 1 asset and scene generation complete.");
        }
    }
}
