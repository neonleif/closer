using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

class Build
{
	#region Menu items
	[MenuItem ("Build/Build All Targets")]
	static void BuildAll ()
	{
		BuildForWeb ();
		BuildForMac ();
		BuildForWin ();
	}

	[MenuItem ("Build/Build Webplayer")]
	static void BuildForWeb ()
	{
		GenericBuild ("web", "", BuildTarget.WebPlayer);
	}
	
	[MenuItem ("Build/Build Windows Standalone")]
	static void BuildForWin ()
	{
		GenericBuild ("win", ".exe", BuildTarget.StandaloneWindows);
	}
		
	[MenuItem ("Build/Build Mac OS X Standalone")]
	static void BuildForMac ()
	{
		GenericBuild ("mac", ".app", BuildTarget.StandaloneOSXIntel);
	}
	#endregion Menu items	
	
	private static void GenericBuild (string platformFolder, string fileExtension, BuildTarget target)
	{
		string locationPathName = Application.dataPath + "/../Builds/" + platformFolder + "/";
		string fileName = PlayerSettings.productName + fileExtension;
		string locationPathNameAndFileName = locationPathName + fileName;
		
		// make sure build paths exist
		if (!Directory.Exists (locationPathName)) {
			Debug.Log (locationPathName + " does not exist. Creating it now.");
			Directory.CreateDirectory (locationPathName);
		}
		
		// save active build target
		BuildTarget userBuildTarget;
		if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.WebPlayer) {
			userBuildTarget = BuildTarget.WebPlayer;
		} else if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Standalone) {
			userBuildTarget = EditorUserBuildSettings.selectedStandaloneTarget;
		} else {
			// default to webplayer
			userBuildTarget = BuildTarget.WebPlayer;
		}
		
		EditorUserBuildSettings.SwitchActiveBuildTarget (target);
		string res = BuildPipeline.BuildPlayer (FindEnabledEditorScenes (), locationPathNameAndFileName, target, BuildOptions.None);
		
		// restore active build target
		EditorUserBuildSettings.SwitchActiveBuildTarget (userBuildTarget);
		
		// complain on error
		if (res.Length > 0) {
			throw new Exception ("BuildPlayer failure: " + res);
		} else {
			Debug.Log (target + "-build successfull and saved as: " + locationPathNameAndFileName);
		}
	}
	
	// TODO add post build action to zip artifacts

	private static string[] FindEnabledEditorScenes ()
	{
		List<string> EditorScenes = new List<string> ();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
			if (!scene.enabled)
				continue;
			EditorScenes.Add (scene.path);
		}
		return EditorScenes.ToArray ();
	}
}