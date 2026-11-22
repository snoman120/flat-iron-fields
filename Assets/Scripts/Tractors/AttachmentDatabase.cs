using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FlatIronFields.Tractors
{
    public static class AttachmentDatabase
    {
        private static readonly Dictionary<string, AttachmentDefinition> Attachments = new Dictionary<string, AttachmentDefinition>();

        public static void Initialize()
        {
            Attachments.Clear();
            AttachmentDefinition[] loaded = Resources.LoadAll<AttachmentDefinition>("Attachments");
            if (loaded == null || loaded.Length == 0)
            {
#if UNITY_EDITOR
                loaded = LoadFromAssetDatabase();
#endif
            }
            if (loaded == null) { return; }
            foreach (AttachmentDefinition attachment in loaded)
            {
                if (attachment != null && !string.IsNullOrEmpty(attachment.Id)) { Attachments[attachment.Id] = attachment; }
            }
        }

        public static void Initialize(IEnumerable<AttachmentDefinition> definitions)
        {
            Attachments.Clear();
            if (definitions == null) { return; }
            foreach (AttachmentDefinition attachment in definitions)
            {
                if (attachment != null && !string.IsNullOrEmpty(attachment.Id)) { Attachments[attachment.Id] = attachment; }
            }
        }

        public static AttachmentDefinition GetAttachment(string id)
        {
            AttachmentDefinition attachment;
            if (string.IsNullOrEmpty(id)) { return null; }
            return Attachments.TryGetValue(id, out attachment) ? attachment : null;
        }

        public static void Clear()
        {
            Attachments.Clear();
        }

        public static IReadOnlyList<AttachmentDefinition> All()
        {
            return Attachments.Values.ToList();
        }

#if UNITY_EDITOR
        private static AttachmentDefinition[] LoadFromAssetDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:AttachmentDefinition", new[] { "Assets/ScriptableObjects/Tractors/Attachments" });
            var result = new List<AttachmentDefinition>();
            foreach (string guid in guids)
            {
                AttachmentDefinition attachment = AssetDatabase.LoadAssetAtPath<AttachmentDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (attachment != null) { result.Add(attachment); }
            }
            return result.ToArray();
        }
#endif
    }
}
