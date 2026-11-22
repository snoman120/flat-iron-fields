using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FlatIronFields.Progression
{
    public static class SkillDatabase
    {
        private static readonly Dictionary<string, SkillDefinition> Skills = new Dictionary<string, SkillDefinition>();

        public static void Initialize()
        {
            Skills.Clear();
            SkillDefinition[] loaded = Resources.LoadAll<SkillDefinition>("Skills");
            if (loaded == null || loaded.Length == 0)
            {
#if UNITY_EDITOR
                loaded = LoadFromAssetDatabase();
#endif
            }
            if (loaded == null) { return; }
            foreach (SkillDefinition skill in loaded)
            {
                if (skill != null && !string.IsNullOrEmpty(skill.Id)) { Skills[skill.Id] = skill; }
            }
        }

        public static void Initialize(IEnumerable<SkillDefinition> definitions)
        {
            Skills.Clear();
            if (definitions == null) { return; }
            foreach (SkillDefinition skill in definitions)
            {
                if (skill != null && !string.IsNullOrEmpty(skill.Id)) { Skills[skill.Id] = skill; }
            }
        }

        public static SkillDefinition GetSkill(string id)
        {
            SkillDefinition skill;
            if (string.IsNullOrEmpty(id)) { return null; }
            return Skills.TryGetValue(id, out skill) ? skill : null;
        }

        public static void Clear()
        {
            Skills.Clear();
        }

        public static IReadOnlyList<SkillDefinition> All()
        {
            return Skills.Values.ToList();
        }

#if UNITY_EDITOR
        private static SkillDefinition[] LoadFromAssetDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:SkillDefinition", new[] { "Assets/ScriptableObjects/Skills" });
            var result = new List<SkillDefinition>();
            foreach (string guid in guids)
            {
                SkillDefinition skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (skill != null) { result.Add(skill); }
            }
            return result.ToArray();
        }
#endif
    }
}
