using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildMuseumGetawayPlayer
{
    private static readonly string[] Scenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/MuseumGetaway.unity"
    };

    [MenuItem("Tools/Build Museum Getaway Player")]
    public static void BuildPlayer()
    {
        string outputPath = GetOutputPath(EditorUserBuildSettings.activeBuildTarget);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = outputPath,
            target = EditorUserBuildSettings.activeBuildTarget,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Museum Getaway player build succeeded: {outputPath} ({summary.totalSize} bytes)");
            EditorUtility.RevealInFinder(outputPath);
            return;
        }

        Debug.LogError($"Museum Getaway player build failed: {summary.result}, {summary.totalErrors} errors.");
    }

    private static string GetOutputPath(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneOSX:
                return "Builds/macOS/Museum Getaway.app";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Builds/Windows/Museum Getaway.exe";
            case BuildTarget.StandaloneLinux64:
                return "Builds/Linux/Museum Getaway";
            case BuildTarget.WebGL:
                return "Builds/WebGL";
            default:
                return $"Builds/{target}/MuseumGetaway";
        }
    }
}
