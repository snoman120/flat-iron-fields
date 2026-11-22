using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FlatIronFields.Game.Editor
{
    public static class PhaseTwoAssetGenerator
    {
        public const string MixerPath = "Assets/Audio/FarmAudio.mixer";
        public const string ProfilePath = "Assets/Audio/SeasonalVolumeProfile.asset";

        [MenuItem("Flat Iron Fields/Generate Audio and Palette Assets")]
        public static void GenerateAll()
        {
            EnsureFolder("Assets/Audio");
            CreateMixer();
            CreateVolumeProfile();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Flat Iron Fields: generated audio mixer and seasonal volume profile.");
        }

        private static void CreateMixer()
        {
            AudioMixer existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (existing != null) { return; }

            System.Type controllerType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
            if (controllerType == null)
            {
                Debug.LogError("Flat Iron Fields: AudioMixerController type not found.");
                return;
            }

            object rawController = Activator.CreateInstance(controllerType, true);
            UnityEngine.Object controller = rawController as UnityEngine.Object;
            if (controller == null)
            {
                Debug.LogError("Flat Iron Fields: failed to instantiate AudioMixerController.");
                return;
            }
            MethodInfo createAsset = controllerType.GetMethod("CreateDefaultAsset", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo createGroup = controllerType.GetMethod("CreateNewGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (createAsset == null || createGroup == null)
            {
                Debug.LogError("Flat Iron Fields: AudioMixerController creation methods not found.");
                UnityEngine.Object.DestroyImmediate(controller);
                return;
            }

            createAsset.Invoke(controller, new object[] { MixerPath });
            createGroup.Invoke(controller, new object[] { "Music", true });
            createGroup.Invoke(controller, new object[] { "Sfx", true });
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null)
            {
                Debug.LogError("Flat Iron Fields: failed to create FarmAudio.mixer.");
            }
        }

        private static void CreateVolumeProfile()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            ColorAdjustments adjustments = null;
            foreach (VolumeComponent component in profile.components)
            {
                adjustments = component as ColorAdjustments;
                if (adjustments != null) { break; }
            }
            if (adjustments == null)
            {
                adjustments = profile.Add<ColorAdjustments>(true);
            }
            if (!AssetDatabase.Contains(adjustments))
            {
                AssetDatabase.AddObjectToAsset(adjustments, profile);
            }

            adjustments.active = true;
            adjustments.postExposure.Override(0f);
            adjustments.contrast.Override(2f);
            adjustments.saturation.Override(5f);
            adjustments.colorFilter.Override(new Color(0.92f, 1f, 0.90f));
            EditorUtility.SetDirty(profile);
            EditorUtility.SetDirty(adjustments);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) { return; }
            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            string name = path.Substring(separator + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
