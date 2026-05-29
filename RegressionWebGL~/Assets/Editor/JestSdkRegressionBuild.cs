using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace com.jest.sdk.regression.Editor
{
    public static class JestSdkRegressionBuild
    {
        private const string DefaultScenePath = "Assets/Scenes/SdkRegressionScene.unity";
        private const string BootstrapScriptPath = "Assets/Scripts/Regression/SdkRegressionSceneBootstrap.cs";

        public static void BuildWebGL()
        {
            var outputPath = GetCommandLineValue("-outputPath") ?? "Build/WebGL";
            var scenePath = NormalizeAssetPath(GetCommandLineValue("-scenePath") ?? DefaultScenePath);
            EnsureRegressionScene(scenePath);

            // The PR smoke job serves the output with a plain static file server, which
            // cannot satisfy the Content-Encoding headers a compressed build expects.
            // `-disableWebGLCompression` produces uncompressed files for that job only;
            // the released fixture keeps the project's default compression.
            var disableCompression = HasCommandLineFlag("-disableWebGLCompression");
            var previousCompression = PlayerSettings.WebGL.compressionFormat;
            if (disableCompression)
            {
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            }

            try
            {
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { scenePath },
                    locationPathName = outputPath,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                });

                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException("WebGL build failed: " + report.summary.result);
                }
            }
            finally
            {
                if (disableCompression)
                {
                    PlayerSettings.WebGL.compressionFormat = previousCompression;
                }
            }
        }

        private static void EnsureRegressionScene(string scenePath)
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null
                ? EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single)
                : EditorSceneManager.OpenScene(scenePath);

            var bootstrapType = LoadBootstrapType();
            if (UnityEngine.Object.FindObjectOfType(bootstrapType) == null)
            {
                var bootstrapObject = new GameObject("JestSdkRegressionSceneBootstrap");
                bootstrapObject.AddComponent(bootstrapType);
                EditorSceneManager.MarkSceneDirty(scene);
            }

            if (scene.path != scenePath || scene.isDirty)
            {
                EditorSceneManager.SaveScene(scene, scenePath);
                AssetDatabase.ImportAsset(scenePath);
            }
        }

        private static Type LoadBootstrapType()
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(BootstrapScriptPath);
            if (script == null)
            {
                throw new InvalidOperationException(
                    "No SDK regression scene bootstrap script found at " + BootstrapScriptPath + ".");
            }

            var bootstrapType = script.GetClass();
            if (bootstrapType == null)
            {
                throw new InvalidOperationException(
                    "SDK regression scene bootstrap script has not compiled: " + BootstrapScriptPath + ".");
            }

            return bootstrapType;
        }

        private static bool HasCommandLineFlag(string key)
        {
            return Array.IndexOf(Environment.GetCommandLineArgs(), key) >= 0;
        }

        private static string GetCommandLineValue(string key)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == key)
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        private static string NormalizeAssetPath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
