using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool to automate creating animation clips for all sprites in a spritesheet following example animation clips.
/// </summary>
public class SpritesheetAnimationClipMaker : EditorWindow
{
	static readonly GUIContent _explanationLabel = new("Automatically create animation clips for all sprites in a spritesheet following a set of example animation clips.\nThe example clips must be made using sprites of the same sheet.");
	static readonly GUIContent _spriteSheetLabel = new("Spritesheet", "Spritesheet Texture2D asset from which to make new Animation Clips.");
	static readonly GUIContent _sampleClipsLabel = new("Sample Animation Clips", "Animation Clips to create variations of for all other sprites in the spritesheet. Clips must be made using sprites from the spritesheet.");
	static readonly GUIContent _generateButtonLabel = new("Generate Animation Clips");

	public Texture2D targetSpriteSheet;
	public AnimationClip[] sampleClips;
	//TODO add user controls that dictate how the tool searches for new sprite sets within the sheet to use for animations. Default to rows now.
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
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(targetSpriteSheet)), _spriteSheetLabel);
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(sampleClips)), _sampleClipsLabel);
		if (EditorGUI.EndChangeCheck())
		{
			serializedObject.ApplyModifiedProperties();
			_subSprites = EditorSpriteUtilities.GetSpritesFromTexture2D(targetSpriteSheet);
			_spritesAndClipsMatch = sampleClips?.Length > 0 && Validation_SheetContainsAnimationSprites(_subSprites, sampleClips);
			//TODO filter out animation clips that don't affect sprites
		}
		EditorGUI.BeginDisabledGroup(!_spritesAndClipsMatch);
		if (GUILayout.Button(_generateButtonLabel))
		{
			var sampleClipSprites = EditorSpriteUtilities.OrderSpritesByTexturePlacement(
				sampleClips.SelectMany(c => EditorSpriteUtilities.GetSpritesFromClip(c)).Distinct().ToArray());
			var spriteSets = GetSpriteSets(_subSprites, sampleClipSprites);
			foreach (var clip in sampleClips)
			{
				var clipPath = AssetDatabase.GetAssetPath(clip);
				foreach (var set in spriteSets)
				{
					//TODO allow for user to provide a naming scheme, or figure something clever out first.
					var newClipPath = clipPath.Insert(clipPath.LastIndexOf('/') + 1, $"COPY_{set.Key}_");
					if (AssetDatabase.CopyAsset(clipPath, newClipPath))
					{
						var spriteMap = sampleClipSprites.Zip(set.Value, (sample, sprite) => new { sample, sprite }).ToDictionary(item => item.sample, item => item.sprite);
						EditorSpriteUtilities.ReplaceSpritesInClip(spriteMap, AssetDatabase.LoadAssetAtPath<AnimationClip>(newClipPath));
						AssetDatabase.SaveAssetIfDirty(new GUID(AssetDatabase.AssetPathToGUID(newClipPath)));
					}
					else
						Debug.LogError($"Failed to create copy asset of {clipPath} at {newClipPath}");
				}
			}
			AssetDatabase.Refresh();
		}
		EditorGUI.EndDisabledGroup();
	}

	/// <summary>
	/// Organize all the sprites into sets (simple rows for now)
	/// </summary>
	/// <param name="sprites"></param>
	private Dictionary<int, Sprite[]> GetSpriteSets(Sprite[] sprites, Sprite[] sampleSprites)
	{
		var result = new Dictionary<int, Sprite[]>();
		foreach(var sprite in sprites)
		{
			if (sampleSprites.Contains(sprite))
				continue;
			if (int.TryParse(Regex.Match(sprite.name, @"\d+").Value, out var index)) //Find the first number, which in our test case will be X in "CharX". TODO let users provide search patterns
			{
				if (result.ContainsKey(index))
					result[index] = EditorSpriteUtilities.OrderSpritesByTexturePlacement(result[index].Append(sprite).ToArray());
				else
					result.Add(index, new Sprite[] { sprite });
			} 
		}
		return result;
	}

	/// <summary>
	/// Checks if every sprite in the animation clips is present in the given sheet sprites.
	/// </summary>
	/// <param name="sheetSprites">Sprites ripped from a texture sheet.</param>
	/// <param name="animationClips">Animation clips to check.</param>
	private bool Validation_SheetContainsAnimationSprites(Sprite[] sheetSprites, AnimationClip[] animationClips)
	{
		return animationClips?.SelectMany(c => EditorSpriteUtilities.GetSpritesFromClip(c)).Distinct().All(s => ArrayUtility.IndexOf(sheetSprites, s) >= 0) ?? false;
	}
}
