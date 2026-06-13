using com.SolePilgrim.Unity.Extensions.Attributes;
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
		static readonly GUIContent _animatorControllerLabel = new("Target Animator Controller", "The Animator Controller for which to create overrides.");
		static readonly GUIContent _animationSetsFoldersLabel = new("Animationset Folders", "Paths to the sets of animations for which to make Animator Override Controllers.");
		static readonly GUIContent _generateButtonLabel = new("Create Override Controllers");
		public AnimatorController targetAnimatorController;
		[FolderPathDialog("Animation Set")]
		public string[] animationSetsFolders;


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
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(targetAnimatorController)), _animatorControllerLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(animationSetsFolders)), _animationSetsFoldersLabel);
			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			if (GUILayout.Button(_generateButtonLabel))
			{

			}
		}
	}
}