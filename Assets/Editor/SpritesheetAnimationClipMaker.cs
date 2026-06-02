using System.Linq;
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
			//TODO check if the animation clips are valid
		}
		EditorGUI.BeginDisabledGroup(false); //TODO use above validation here
		if (GUILayout.Button(_generateButtonLabel))
		{
			foreach(var clip in sampleClips)
			{
				var clipPath = AssetDatabase.GetAssetPath(clip);
				//TODO allow for user to provide a naming scheme, or figure something clever out first.
				var newClipPath = clipPath.Insert(clipPath.LastIndexOf('/') + 1, "COPY_");
				var sprites = EditorSpriteUtilities.GetSpritesFromClip(clip);
				EditorSpriteUtilities.LogSprites(sprites.ToArray());
				if (AssetDatabase.CopyAsset(clipPath, newClipPath))
				{
					//TODO replace the sprites in the copied animations
					AssetDatabase.SaveAssetIfDirty(new GUID(AssetDatabase.AssetPathToGUID(newClipPath)));
					AssetDatabase.Refresh();
				}
				else
				{
					Debug.LogError($"Failed to create copy asset of {clipPath} at {newClipPath}");
				}
			}
		}
		EditorGUI.EndDisabledGroup();
	}
}
