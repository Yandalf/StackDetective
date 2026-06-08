using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpriteSheetTools
{
	/// <summary>
	/// Editor tool to automate creating animation clips for all sprites in a spritesheet following example animation clips.
	/// </summary>
	public sealed class SpritesheetAnimationClipMaker : EditorWindow
	{
		static readonly GUIContent _explanationLabel = new("Automatically create animation clips for all sprites in a spritesheet following a set of example animation clips.\nThe example clips must be made using sprites of the same sheet.");
		static readonly GUIContent _spriteSheetLabel = new("Spritesheet", "Spritesheet Texture2D asset from which to make new Animation Clips.");
		static readonly GUIContent _sampleClipsLabel = new("Sample Animation Clips", "Animation Clips to create variations of for all other sprites in the spritesheet. Clips must be made using sprites from the spritesheet.");
		static readonly GUIContent _setDirectionLabel = new("Set Direction", "Direction of each set of Sprites in the spritesheet.");
		static readonly GUIContent _copySuffixLabel = new("Copy Suffix", "Suffix to add to the names of the copies, then adds increment numbers at \"{0}\" (example: \"_Variant{0}\").");
		static readonly GUIContent _subfoldersLabel = new("Put Clips in Subfolders", "The newly made clips will be placed in per-variant subfolders.");
		static readonly GUIContent _generateButtonLabel = new("Make Animation Clips", "No, this does not use AI in any way! ;)");

		public Texture2D targetSpriteSheet;
		public AnimationClip[] sampleClips;
		public SpritesheetSetDirection setDirection;
		public string copySuffix = "_Variant{0}";
		public bool putClipsInSubfolders;
		private Sprite[] _subSprites;
		private bool _spritesAndClipsMatch;


		[MenuItem("Tools/Sprites/Spritesheet Animation Clip Maker")]
		private static void OpenWindow()
		{
			const float wndWidth = 400.0f;
			const float wndHeight = 200.0f;
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
			EditorGUI.BeginChangeCheck(); //This is a bit ugly, but we only want to update subSprites and validate when either the sprite sheet or the animation clips change.
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(targetSpriteSheet)), _spriteSheetLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(sampleClips)), _sampleClipsLabel);
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties(); //Still need to reserialize here!
				_subSprites = SpritesheetUtilities.GetSpritesFromTexture2D(targetSpriteSheet);
				_spritesAndClipsMatch = sampleClips?.Length > 0 && Validation_SheetContainsAnimationSprites(_subSprites, sampleClips);
				//TODO filter out animation clips that don't affect sprites
			}
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(setDirection)), _setDirectionLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(copySuffix)), _copySuffixLabel);
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(putClipsInSubfolders)), _subfoldersLabel);
			if (EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();

			EditorGUI.BeginDisabledGroup(!_spritesAndClipsMatch);
			if (GUILayout.Button(_generateButtonLabel))
			{
				var sampleClipSprites = SpritesheetUtilities.OrderSpritesByTexturePlacement(
					sampleClips.SelectMany(c => SpritesheetUtilities.GetSpritesFromClip(c)).Distinct().ToArray(), setDirection);
				var spriteSets = SpritesheetUtilities.OrderSpritesInSets(_subSprites, setDirection);
				var suffixRegex = new Regex(copySuffix.Replace("{0}", "\\d+"));
				foreach (var clip in sampleClips)
				{
					var originalClipPath = AssetDatabase.GetAssetPath(clip);
					var baseFileName = Path.GetFileName(originalClipPath);
					var clipPath = originalClipPath.Replace(baseFileName, string.Empty);
					foreach (var set in spriteSets) //TODO this is a mess and needs a rewrite. Also sometimes throws an error when trying to create the same copy twice???
					{
						var newFileName = suffixRegex.IsMatch(baseFileName) ?
							suffixRegex.Replace(baseFileName, string.Format(copySuffix, set.Key)) :
							baseFileName.Insert(baseFileName.LastIndexOf('.'), string.Format(copySuffix, set.Key));
						var newClipPath = putClipsInSubfolders ? Path.Combine(clipPath, string.Format(copySuffix, set.Key), newFileName) : Path.Combine(clipPath, newFileName);
						if (putClipsInSubfolders && !AssetDatabase.IsValidFolder(Path.Combine(clipPath, string.Format(copySuffix, set.Key))))
							AssetDatabase.CreateFolder(clipPath[..^1], string.Format(copySuffix, set.Key));
						if (originalClipPath != newClipPath && AssetDatabase.CopyAsset(originalClipPath, newClipPath))
						{
							var spriteMap = sampleClipSprites.Zip(set.Value, (sample, sprite) => new { sample, sprite }).ToDictionary(item => item.sample, item => item.sprite);
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