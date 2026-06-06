using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

//TODO FIX: renaming once in either direction works, but afterwards renaming an asset with the other direction jumbles all the sprites for some reason.
/// <summary>
/// Editor tool for quickly renaming all sprites in a spritesheet following a scheme.
/// </summary>
public class SpritesheetRenamer : EditorWindow
{
    static readonly GUIContent _explanationLabel = new("Automatically renames sprites in a spritesheet asset file following a given naming scheme.\nUse this to easily group sets of sprites in large sheets.");
    static readonly GUIContent _spritesheetLabel = new("Spritesheet", "Spritesheet Texture2D asset of which the sub-sprites have to be renamed.");
	static readonly GUIContent _setDirectionLabel = new("Set Direction", "Direction of each set of Sprites in the spritesheet.");
	static readonly GUIContent _subSpriteCountLabel = new("Sprite Count", "Sprites in the sheet.");
    static readonly GUIContent _spriteRowsColumnsLabel = new("Rows & Columns", "Rows and columns of sprites in the sheet.");
    static readonly GUIContent _spriteNameLabel = new("Sprites Name", "Name to give each sprite. Insert row and column numbers with {row} and {column}.");
    static readonly GUIContent _renameButtonLabel = new("Rename", "Rename all the sprites.");
    
    public Texture2D targetSpritesheet;
    public SpritesheetSetDirection setDirection = SpritesheetSetDirection.Rows;
	public Vector2Int spriteRowsColumns = Vector2Int.one;
	public string spriteName;
	private Sprite[] _subSprites;


    [MenuItem("Tools/Sprites/Spritesheet Renamer")]
    private static void OpenWindow()
    {
        const float wndWidth = 400.0f;
        const float wndHeight = 200.0f;
        var pos = new Vector2(0.5f * (Screen.currentResolution.width - wndWidth),
                              0.5f * (Screen.currentResolution.height - wndHeight));
        var window = GetWindow<SpritesheetRenamer>();
        window.titleContent = new GUIContent(ObjectNames.NicifyVariableName(nameof(SpritesheetRenamer)));
        window.position = new Rect(pos, new Vector2(wndWidth, wndHeight));
        window.Show();
    }

    private void OnGUI()
    {
		var serializedObject = new SerializedObject(this);
		GUILayout.Label(_explanationLabel);
        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(targetSpritesheet)), _spritesheetLabel);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
			_subSprites = EditorSpriteUtilities.GetSpritesFromTexture2D(targetSpritesheet);
		}
		EditorGUI.BeginDisabledGroup(_subSprites == null || _subSprites.Length == 0);
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(setDirection)), _setDirectionLabel);
		EditorGUILayout.LabelField(_subSpriteCountLabel, new GUIContent(_subSprites?.Length.ToString() ?? "0"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(spriteRowsColumns)), _spriteRowsColumnsLabel);
		EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(spriteName)), _spriteNameLabel);
		if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
        GUILayout.Space(EditorGUIUtility.singleLineHeight);
        if (GUILayout.Button(_renameButtonLabel))
        {
            _subSprites = EditorSpriteUtilities.OrderSpritesByTexturePlacement(_subSprites, setDirection);
            var formatString = spriteName.Replace("{row}", "{0}").Replace("{column}", "{1}");
            var namePairs = GetSpriteNamePairs(_subSprites, setDirection, spriteRowsColumns, formatString);
            //See https://gist.github.com/edwardrowe/1b18a5c8fd180733a68f
            AmendMetaFile(targetSpritesheet, namePairs);
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// Get pairs of old and new names for each given sprite.
    /// </summary>
    static Dictionary<string, string> GetSpriteNamePairs(Sprite[] sprites, SpritesheetSetDirection setDirection, Vector2Int spriteRowsColumns, string formatString)
    {
        var result = new Dictionary<string, string>(sprites.Length);
        for (int i = 0; i < sprites.Length; i++)
        {
            var column = setDirection == SpritesheetSetDirection.Rows ? 
                i % spriteRowsColumns.y :
                i / spriteRowsColumns.x;
            var row = setDirection == SpritesheetSetDirection.Rows ? 
                i / spriteRowsColumns.y :
                i % spriteRowsColumns.x;
            result.Add(sprites[i].name, string.Format(formatString, row, column));
            Debug.Log($"{sprites[i].name}: {string.Format(formatString, row, column)}");
        }
        return result;
    }

    /// <summary>
    /// Update the meta file.
    /// </summary>
    static void AmendMetaFile(Object asset, Dictionary<string, string> renamePairs)
    {
        string path = AssetDatabase.GetAssetPath(asset);
        var pathToTextureMetaFile = path + ".meta";
        string metaFile = System.IO.File.ReadAllText(pathToTextureMetaFile);
        foreach (var pair in renamePairs)
        {
            var regex = new Regex($"({pair.Key})\\b"); //Use a regex to ensure we have an exact match with the key.
            metaFile = regex.Replace(metaFile, pair.Value);
        }
        // If users have hidden meta files they will get an access exception.
        // Need to unhide them briefly.
        var originalFileAttributes = System.IO.File.GetAttributes(pathToTextureMetaFile);
        System.IO.File.SetAttributes(pathToTextureMetaFile, originalFileAttributes & ~System.IO.FileAttributes.Hidden);
        System.IO.File.WriteAllText(pathToTextureMetaFile, metaFile);
        System.IO.File.SetAttributes(pathToTextureMetaFile, originalFileAttributes);

        AssetDatabase.Refresh();
    }
}
