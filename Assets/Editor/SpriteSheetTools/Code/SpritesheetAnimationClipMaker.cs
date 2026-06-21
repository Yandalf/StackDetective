using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpritesheetTools
{
	/// <summary>
	/// Editor tool to automate creating animation clips for all sprites in a spritesheet following example animation clips.
	/// </summary>
	public sealed class SpritesheetAnimationClipMaker : EditorWindow
	{
		static readonly GUIContent _explanationLabel = new("Automatically create animation clips for all sprites in a spritesheet following a set of example animation clips.\nThe example clips must be made using sprites of the same sheet.");
		static readonly GUIContent _spriteSheetLabel = new("Spritesheet", "Spritesheet Texture2D asset from which to make new Animation Clips.");
		static readonly GUIContent _selectorLabel = new("Selector", "Selectors define how Sprites are ordered within a spritesheet.");
		static readonly GUIContent _sampleClipsLabel = new("Sample Animation Clips", "Animation Clips to create variations of for all other sprites in the spritesheet. Clips must be made using sprites from the spritesheet.");
		static readonly GUIContent _copySuffixLabel = new("Copy Suffix", "Suffix to add to the names of the copies, then adds increment numbers at \"{0}\" (example: \"_Variant{0}\").");
		static readonly GUIContent _subfoldersLabel = new("Put Clips in Subfolders", "The newly made clips will be placed in per-variant subfolders.");
		static readonly GUIContent _generateButtonLabel = new("Make Animation Clips", "No, this does not use AI in any way! ;)");

		public Texture2D targetSpriteSheet;
		[SerializeReference]
		public SetSelector setSelector;
		public AnimationClip[] sampleClips;
		public string copySuffix = "_Variant{0}";
		public bool putClipsInSubfolders;
		private readonly SetSelector[] _selectors;
		private readonly GUIContent[] _selectorsDisplayOptions;
		private Sprite[] _subSprites;
		private bool _spritesAndClipsMatch;


		public SpritesheetAnimationClipMaker()
		{
			//TODO search for all Selectors in the project with reflection. I've written tools for this before in another project.
			_selectors = new SetSelector[] { new RowSetSelector(), new ColumnSetSelector() };
			_selectorsDisplayOptions = new GUIContent[]
			{
				new(nameof(RowSetSelector)),
				new(nameof(ColumnSetSelector))
			};
			setSelector = _selectors[0];
		}

		[MenuItem("Tools/Sprites/Spritesheet Animation Clip Maker")]
		private static void OpenWindow()
		{
			const float wndWidth = 400.0f;
			const float wndHeight = 400.0f;
			var pos = new Vector2(0.5f * (Screen.currentResolution.width - wndWidth),
								  0.5f * (Screen.currentResolution.height - wndHeight));
			var window = GetWindow<SpritesheetAnimationClipMaker>();
			window.titleContent = new GUIContent(ObjectNames.NicifyVariableName(nameof(SpritesheetAnimationClipMaker)));
			window.position = new Rect(pos, new Vector2(wndWidth, wndHeight));
			window.Show();
		}

		private void OnGUI()
		{
			var serializedObject = new SerializedObject(this);
			GUILayout.Label(_explanationLabel);
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(targetSpriteSheet)), _spriteSheetLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(sampleClips)), _sampleClipsLabel);
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties(); //Still need to reserialize here!
				_subSprites = SpritesheetUtilities.GetSpritesFromTexture2D(targetSpriteSheet);
				_spritesAndClipsMatch = sampleClips?.Length > 0 && Validation_SheetContainsAnimationSprites(_subSprites, sampleClips);
				//TODO filter out animation clips that don't affect sprites
			}
			EditorGUI.BeginChangeCheck();
			var currentSelectorIndex = Array.IndexOf(_selectors, setSelector);
			currentSelectorIndex = EditorGUILayout.Popup(_selectorLabel, currentSelectorIndex, _selectorsDisplayOptions);
			if (EditorGUI.EndChangeCheck())
			{
				setSelector = _selectors[currentSelectorIndex];
				serializedObject.ApplyModifiedProperties();
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(setSelector)), true); //Draw the Selector and its children. TODO find a way to prettify this with custom labels for the children and such.
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(copySuffix)), _copySuffixLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(putClipsInSubfolders)), _subfoldersLabel);
			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();

			EditorGUI.BeginDisabledGroup(!_spritesAndClipsMatch);
			if (GUILayout.Button(_generateButtonLabel))
			{
				var spriteSets = setSelector.SplitSpritesInSets(_subSprites);
				var suffixRegex = new Regex(copySuffix.Replace("{0}", "\\d+"));
				Sprite[] sampleClipSprites = null;
				foreach (var clip in sampleClips)
				{
					//Verify that the sample clip's sprites are all part of a single set, skip if not.
					sampleClipSprites = SpritesheetUtilities.GetSpritesFromClip(clip).Distinct().ToArray();
					if (!setSelector.AreSpritesSingleSet(sampleClipSprites))
					{
						Debug.LogWarning($"AnimationClip {clip.name} uses Sprites that belong to multiple sets or source Textures and was skipped.");
						continue;
					}
					setSelector.OrderSpritesByAbsoluteIndex(sampleClipSprites);
					var sampleClipSet = setSelector.GetSpriteSetIndex(sampleClipSprites.First());
					var originalClipPath = AssetDatabase.GetAssetPath(clip);
					var baseFileName = Path.GetFileName(originalClipPath);
					var clipPath = originalClipPath.Replace(baseFileName, string.Empty);
					foreach (var set in spriteSets)
					{
						if (sampleClipSprites.First().texture == targetSpriteSheet && 
							set.Key == sampleClipSet) //Skip making animations using the sample clip sprites.
							continue;
						var newFileName = suffixRegex.IsMatch(baseFileName) ?
							suffixRegex.Replace(baseFileName, string.Format(copySuffix, set.Key)) :
							baseFileName.Insert(baseFileName.LastIndexOf('.'), string.Format(copySuffix, set.Key));
						var newClipPath = putClipsInSubfolders ? Path.Combine(clipPath, string.Format(copySuffix, set.Key), newFileName) : Path.Combine(clipPath, newFileName);
						if (putClipsInSubfolders && !AssetDatabase.IsValidFolder(Path.Combine(clipPath, string.Format(copySuffix, set.Key))))
							AssetDatabase.CreateFolder(clipPath[..^1], string.Format(copySuffix, set.Key));
						if (AssetDatabase.CopyAsset(originalClipPath, newClipPath))
						{
							var spriteMap = new Dictionary<Sprite, Sprite>(
								sampleClipSprites.Select(sprite => new KeyValuePair<int, Sprite>(setSelector.GetSpriteSetSubIndex(sprite), sprite)).
								Select(kvp => new KeyValuePair<Sprite, Sprite>(kvp.Value, set.Value.First(s => setSelector.GetSpriteSetSubIndex(s) == kvp.Key))));
							SpritesheetUtilities.ReplaceSpritesInClip(spriteMap, AssetDatabase.LoadAssetAtPath<AnimationClip>(newClipPath));
							AssetDatabase.SaveAssetIfDirty(new GUID(AssetDatabase.AssetPathToGUID(newClipPath)));
						}
					}
				}
				AssetDatabase.Refresh();
			}
			EditorGUI.EndDisabledGroup();
		}

		/// <summary>
		/// Checks if every sprite in the animation clips is present in the given sheet sprites.
		/// </summary>
		/// <param name="sheetSprites">Sprites ripped from a texture sheet.</param>
		/// <param name="animationClips">Animation clips to check.</param>
		static private bool Validation_SheetContainsAnimationSprites(Sprite[] sheetSprites, AnimationClip[] animationClips)
		{
			if (animationClips?.Where(c => c != null).Count() == 0)
				return false;
			return animationClips.SelectMany(c => SpritesheetUtilities.GetSpritesFromClip(c)).Distinct().All(s => ArrayUtility.IndexOf(sheetSprites, s) >= 0);
		}
	}
}