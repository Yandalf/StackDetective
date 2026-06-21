using com.SolePilgrim.Unity.Extensions.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpritesheetTools
{
	/// <summary>
	/// Editor tool to automatically create Animator Override Controllers with the specified Animation Clips.
	/// </summary>
	public sealed class AnimatorOverrideControllerMaker : EditorWindow
	{
		static readonly string _animationCaptureGroupName = "animationName";
		static readonly string _animationSetCaptureGroupName = "animationSetName";
		static readonly GUIContent _animatorControllerLabel = new("Target Animator Controller", "The Animator Controller for which to create overrides.");
		static readonly GUIContent _animationSetsFoldersLabel = new("Animationset Folders", "Paths to the sets of animations for which to make Animator Override Controllers.");
		static readonly GUIContent _sourceAnimationNameFilterLabel = new("Source Animation Name Filter", "Denotes which part of the name of animations in the source clips to compare with. Use * as greedy wildcard characters, and {compare} for parts that must be compared.");
		static readonly GUIContent _replacementAnimationNameFilterLabel = new("Replacement Animation Name Filter", "Denotes which part of the name of animations in the replacement clips to compare with. Use * as greedy wildcard characters, {compare} for parts that must be compared, and {setName} for the part denoting the animation set name.");
		static readonly GUIContent _generateButtonLabel = new("Create Override Controllers");
		public AnimatorController targetAnimatorController;
		[FolderPathDialog("Animation Set")]
		public string[] animationSetsFolders;
		public string sourceAnimationNameFilter;
		public string replacementAnimationNameFilter;


		[MenuItem("Tools/Sprites/Animator Override Controller Maker")]
		private static void OpenWindow()
		{
			const float wndWidth = 400.0f;
			const float wndHeight = 200.0f;
			var pos = new Vector2(0.5f * (Screen.currentResolution.width - wndWidth),
								  0.5f * (Screen.currentResolution.height - wndHeight));
			var window = GetWindow<AnimatorOverrideControllerMaker>();
			window.titleContent = new GUIContent(ObjectNames.NicifyVariableName(nameof(AnimatorOverrideControllerMaker)));
			window.position = new Rect(pos, new Vector2(wndWidth, wndHeight));
			window.Show();
		}

		private void OnGUI()
		{
			var serializedObject = new SerializedObject(this);

			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.labelWidth += 60;
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(targetAnimatorController)), _animatorControllerLabel);
			EditorGUIUtility.labelWidth -= 60;
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(animationSetsFolders)), _animationSetsFoldersLabel);
			EditorGUIUtility.labelWidth += 60;
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(sourceAnimationNameFilter)), _sourceAnimationNameFilterLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(replacementAnimationNameFilter)), _replacementAnimationNameFilterLabel);
			EditorGUIUtility.labelWidth -= 60;
			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			EditorGUI.BeginDisabledGroup(targetAnimatorController == null || 
				animationSetsFolders.Any(p => string.IsNullOrEmpty(p)) || 
				string.IsNullOrEmpty(sourceAnimationNameFilter) ||
				string.IsNullOrEmpty(replacementAnimationNameFilter));
			if (GUILayout.Button(_generateButtonLabel))
			{
				var sourceRegex = CreateFilterRegex(sourceAnimationNameFilter);
				var targetRegex = CreateFilterRegex(replacementAnimationNameFilter);
				var sourceFilePath = AssetDatabase.GetAssetPath(targetAnimatorController);
				var sourceMatchings = targetAnimatorController.animationClips.Select(c => sourceRegex.Match(c.name).Groups[_animationCaptureGroupName].Value).ToArray();
				foreach (var path in animationSetsFolders)
				{
					var replacementFiles = Directory.GetFiles(path, "*.anim").Select(p => p.Replace(@"\", "/")).ToArray(); //TODO find a cleaner way to replace \\ with /. This feels like a Unity vs .net thing.
					var matches = replacementFiles.Select(p => targetRegex.Match(p[(p.LastIndexOf('/') + 1)..]));
					var replacementMatchings = matches.Select(m => m.Groups[_animationCaptureGroupName].Value).ToArray();
					var setName = matches.First().Groups[_animationSetCaptureGroupName].Value;
					//Create a newAnimation Override Controller and match each animation to an override animation by name
					var overrideController = new AnimatorOverrideController(targetAnimatorController)
					{
						name = $"{targetAnimatorController.name}_{setName}"
					};
					var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
					overrideController.GetOverrides(overrides);
					foreach (var matching in sourceMatchings)
					{
						var index = Array.IndexOf(sourceMatchings, matching);
						var replacementFile = replacementFiles[Array.IndexOf(replacementMatchings, matching)];
						var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(replacementFile);
						overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(targetAnimatorController.animationClips[index], clip));
					}
					overrideController.ApplyOverrides(overrides);
					var assetPath = sourceFilePath.Insert(sourceFilePath.LastIndexOf("."), $"_{setName}");
					AssetDatabase.CreateAsset(overrideController, assetPath);
					AssetDatabase.SaveAssetIfDirty(overrideController);
				}
				AssetDatabase.Refresh();
			}
			EditorGUI.EndDisabledGroup();
		}

		private Regex CreateFilterRegex(string filterString)
		{
			var pattern = filterString.Replace(".", "[.]").Replace("*", ".+?").Replace("{compare}", $"(?'{_animationCaptureGroupName}'.+?)").Replace("{setName}", $"(?'{_animationSetCaptureGroupName}'.+)");
			return new Regex(pattern);
		}
	}
}