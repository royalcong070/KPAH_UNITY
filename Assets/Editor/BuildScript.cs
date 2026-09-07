using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build Android APK")]
    public static void BuildAPK()
    {
        string buildPath = "build/KPAH.apk";
        string dir = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string[] scenes = { "Assets/Scenes/Main.unity" };
        
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        PlayerSettings.Android.bundleVersionCode++;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)30;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingBackend.Mono);
        
        var report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {report.summary.outputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"Build failed: {report.summary.result}");
            EditorApplication.Exit(1);
        }
    }
}
