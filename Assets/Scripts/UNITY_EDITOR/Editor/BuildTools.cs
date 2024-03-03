using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Unity.EditorCoroutines.Editor;

public class BuildTools : EditorWindow
{
    private static bool _buildWindowsServer;
    private static bool _buildLinuxServer;
    
    [MenuItem("Tools/Build Tools")]
    public static void OnShowTools()
    {
        GetWindow<BuildTools>();
    }

    private BuildTargetGroup GetTargetGroupForTarget(BuildTarget target) => target switch
    {
        BuildTarget.StandaloneOSX => BuildTargetGroup.Standalone,
        BuildTarget.StandaloneWindows => BuildTargetGroup.Standalone,
        BuildTarget.iOS => BuildTargetGroup.iOS,
        BuildTarget.Android => BuildTargetGroup.Android,
        BuildTarget.StandaloneWindows64 => BuildTargetGroup.Standalone,
        BuildTarget.WebGL => BuildTargetGroup.WebGL,
        BuildTarget.StandaloneLinux64 => BuildTargetGroup.Standalone,
        _ => BuildTargetGroup.Unknown
    };

    Dictionary<BuildTarget, bool> TargetsToBuild = new();
    List<BuildTarget> AvailableTargets = new();

    private void OnEnable()
    {
        AvailableTargets.Clear();
        List<BuildTarget> buildTargets = Enum.GetValues(typeof(BuildTarget)).Cast<BuildTarget>().ToList();
        buildTargets = buildTargets.OrderBy(x => x.ToString()).ToList();
        
        foreach(var buildTargetValue in buildTargets)
        {
            BuildTarget target = buildTargetValue;

            if (target == BuildTarget.StandaloneWindows)
            {
                continue;
            }
            
            // skip if unsupported
            if (!BuildPipeline.IsBuildTargetSupported(GetTargetGroupForTarget(target), target))
                continue;

            AvailableTargets.Add(target);

            // add the target if not in the build list
            if (!TargetsToBuild.ContainsKey(target))
                TargetsToBuild[target] = false;
        }

        // check if any targets have gone away
        if (TargetsToBuild.Count > AvailableTargets.Count)
        {
            // build the list of removed targets
            List<BuildTarget> targetsToRemove = new();
            foreach(var target in TargetsToBuild.Keys)
            {
                if (!AvailableTargets.Contains(target))
                    targetsToRemove.Add(target);
            }

            // cleanup the removed targets
            foreach(var target in targetsToRemove)
                TargetsToBuild.Remove(target);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Platforms to Build", EditorStyles.boldLabel);

        // display the build targets
        int numEnabled = 0;
        foreach(var target in AvailableTargets)
        {
            TargetsToBuild[target] = EditorGUILayout.Toggle(target.ToString(), TargetsToBuild[target]);
        
            if (TargetsToBuild[target])
                numEnabled++;
        }
        
        _buildWindowsServer = EditorGUILayout.Toggle("Windows Server", _buildWindowsServer);
        if (_buildWindowsServer)
        {
            numEnabled++;
        }
        
        _buildLinuxServer = EditorGUILayout.Toggle("Linux Server", _buildLinuxServer);
        if (_buildLinuxServer)
        {
            numEnabled++;
        }

        if (numEnabled > 0)
        {
            // attempt to build?
            string prompt = numEnabled == 1 ? "Build 1 Platform" : $"Build {numEnabled} Platforms";
            if (GUILayout.Button(prompt))
            {
                List<BuildTarget> selectedTargets = new();
                foreach (var target in AvailableTargets)
                {
                    if (TargetsToBuild[target])
                        selectedTargets.Add(target);
                }
                
                if (_buildWindowsServer == true)
                {
                    selectedTargets.Add(BuildTarget.StandaloneWindows64);
                }
                
                if (_buildLinuxServer == true)
                {
                    selectedTargets.Add(BuildTarget.StandaloneLinux64);
                }

                EditorCoroutineUtility.StartCoroutine(PerformBuild(selectedTargets), this);
            }
        }
    }

    IEnumerator PerformBuild(List<BuildTarget> targetsToBuild)
    {
        // show the progress display
        int buildAllProgressID = Progress.Start("Build All", "Building all selected platforms", Progress.Options.Sticky);
        Progress.ShowDetails();
        yield return new EditorWaitForSeconds(1f);

        BuildTarget originalTarget = EditorUserBuildSettings.activeBuildTarget;

        // build each target
        for (int targetIndex = 0; targetIndex < targetsToBuild.Count; ++targetIndex)
        {
            var buildTarget = targetsToBuild[targetIndex];

            Progress.Report(buildAllProgressID, targetIndex + 1, targetsToBuild.Count);
            int buildTaskProgressID;
            yield return new EditorWaitForSeconds(1f);

            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;

            if (_buildWindowsServer == true)
            {
                if (buildTarget is BuildTarget.StandaloneWindows64 or BuildTarget.StandaloneWindows)
                {
                    buildTaskProgressID = Progress.Start($"Build Windows Server", null, Progress.Options.Sticky, buildAllProgressID);
                    yield return new EditorWaitForSeconds(1f);

                    _buildWindowsServer = false;
                    EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
                    // perform the server build
                    if (BuildIndividualTarget(buildTarget))
                    {
                        Progress.Finish(buildTaskProgressID, Progress.Status.Succeeded);
                        yield return new EditorWaitForSeconds(1f);
                    }
                    else
                    {
                        Failed();
                    }
                    
                    continue;
                }
            }

            if (_buildLinuxServer == true)
            {
                if (buildTarget is BuildTarget.StandaloneLinux64)
                {
                    buildTaskProgressID = Progress.Start($"Build Linux Server", null, Progress.Options.Sticky, buildAllProgressID);
                    yield return new EditorWaitForSeconds(1f);

                    _buildLinuxServer = false;
                    EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
                    // perform the server build
                    if (BuildIndividualTarget(buildTarget))
                    {
                        Progress.Finish(buildTaskProgressID, Progress.Status.Succeeded);
                        yield return new EditorWaitForSeconds(1f);
                    }
                    else
                    {
                        Failed();
                    }
                    
                    continue;
                }
            }
            
            yield return new EditorWaitForSeconds(1f);
            buildTaskProgressID = Progress.Start($"Build {buildTarget.ToString()}", null, Progress.Options.Sticky, buildAllProgressID);
            if (BuildIndividualTarget(buildTarget))
            {                
                Progress.Finish(buildTaskProgressID, Progress.Status.Succeeded);
                yield return new EditorWaitForSeconds(1f);
            }
            else
            {
                Failed();
            }

            void Failed()
            {
                Progress.Finish(buildTaskProgressID, Progress.Status.Failed);
                Progress.Finish(buildAllProgressID, Progress.Status.Failed);

                if (EditorUserBuildSettings.activeBuildTarget != originalTarget)
                    EditorUserBuildSettings.SwitchActiveBuildTargetAsync(GetTargetGroupForTarget(originalTarget), originalTarget);
            }
        }

        Progress.Finish(buildAllProgressID, Progress.Status.Succeeded);

        if (EditorUserBuildSettings.activeBuildTarget != originalTarget)
            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(GetTargetGroupForTarget(originalTarget), originalTarget);

        yield return null;
    }

    bool BuildIndividualTarget(BuildTarget target)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(GetTargetGroupForTarget(target), target);
        
        try
        {
            BuildPlayerOptions options = new();

            // get the list of scenes
            List<string> scenes = new();
            foreach (var scene in EditorBuildSettings.scenes)
                scenes.Add(scene.path);

            // configure the build
            options.scenes = scenes.ToArray();
            options.target = target;
            options.targetGroup = GetTargetGroupForTarget(target);

            var suffix = string.Empty;
            if (EditorUserBuildSettings.standaloneBuildSubtarget == StandaloneBuildSubtarget.Server)
            {
                options.subtarget = (int)StandaloneBuildSubtarget.Server;
                suffix = "_Server";

                List<string> serverScenes = new() { options.scenes.First(x => x.Contains(Constants.SceneNames.Kickstart)) };
                foreach (string scene in options.scenes)
                {
                    if (scene.Contains(Constants.SceneNames.Kickstart))
                    {
                        continue;
                    }
                    
                    serverScenes.Add(scene);
                }

                options.scenes = serverScenes.ToArray();
            }
            
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                    options.locationPathName = System.IO.Path.Combine("..\\SnaPBuild\\Windows" + suffix, PlayerSettings.productName + suffix, PlayerSettings.productName + suffix + ".exe");
                    break;
                case BuildTarget.StandaloneLinux64:
                    options.locationPathName = System.IO.Path.Combine("..\\SnaPBuild\\Linux" + suffix, PlayerSettings.productName + suffix, PlayerSettings.productName + suffix + ".x86_64");
                    break;
                case BuildTarget.StandaloneOSX:
                    options.locationPathName = System.IO.Path.Combine("..\\SnaPBuild\\Mac" + suffix, PlayerSettings.productName + suffix, PlayerSettings.productName + suffix + ".app");
                    break;
                case BuildTarget.Android:
                    options.locationPathName = System.IO.Path.Combine("..\\SnaPBuild\\Android" + suffix, PlayerSettings.productName + suffix, PlayerSettings.productName + suffix + ".apk");
                    break;
                default:
                    options.locationPathName = System.IO.Path.Combine("..\\SnaPBuild\\Unknown" + suffix, PlayerSettings.productName, PlayerSettings.productName);
                    break;
            }
            
            if (BuildPipeline.BuildCanBeAppended(target, options.locationPathName) == CanAppendBuild.Yes)
                options.options = BuildOptions.AcceptExternalModificationsToPlayer;
            else
                options.options = BuildOptions.None;
            
            // start the build
            BuildReport report = BuildPipeline.BuildPlayer(options);

            // was the build successful?
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build for {target.ToString()} completed in {report.summary.totalTime.Seconds} seconds");
                return true;
            }

            Debug.LogError($"Build for {target.ToString()} failed");
            
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
