using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FlatIronFields.Game.Editor
{
    public static class BuildSmoke
    {
        [MenuItem("Flat Iron Fields/Build Windows")]
        public static void BuildWindows()
        {
            string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "Windows"));
            Directory.CreateDirectory(outputDir);
            string executable = Path.Combine(outputDir, "FlatIronFields.exe");

            var options = new BuildPlayerOptions
            {
                scenes = new[]
                {
                    "Assets/Scenes/Boot.unity",
                    "Assets/Scenes/MainMenu.unity",
                    "Assets/Scenes/Farm.unity"
                },
                locationPathName = executable,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log(string.Format(
                "Flat Iron Fields Windows build: {0}, size={1} bytes, time={2}, errors={3}, warnings={4}",
                summary.result, summary.totalSize, summary.totalTime, summary.totalErrors, summary.totalWarnings));

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception("Flat Iron Fields Windows build failed: " + summary.result);
            }
        }
    }
}
