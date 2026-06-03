using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool for quickly renaming all sprites in a spritesheet following a scheme.
/// </summary>
public class SpritesheetRenamer : EditorWindow
{
    static readonly GUIContent _explanationLabel = new("Automatically renames sprites in a spritesheet asset file following a given naming scheme.\nUse this to easily group sets of sprites in large sheets.");
    static readonly GUIContent _spriteSheetLabel = new("Spritesheet", "Spritesheet Texture2D asset of which the sub-sprites have to be renamed.");
    static readonly GUIContent _subSpriteCountLabel = new("Sprite Count", "Sprites in the sheet.");
    static readonly GUIContent _spriteRowsColumnsLabel = new("Rows & Columns", "Rows and columns of sprites in the sheet.");
    static readonly GUIContent _spriteNameLabel = new("Sprites Name", "Name to give each sprite. Insert row and column numbers with {row} and {column}.");
    static readonly GUIContent _renameButtonLabel = new("Rename", "Rename all the sprites.");
    
    private Texture2D _targetSpriteSheet;
    private Sprite[] _subSprites;
    private Vector2Int _spriteRowsColumns;
    private string _spriteName;


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
        GUILayout.Label(_explanationLabel);
        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
        EditorGUI.BeginChangeCheck();
        _targetSpriteSheet = (Texture2D)EditorGUILayout.ObjectField(_spriteSheetLabel, _targetSpriteSheet, typeof(Texture2D), false);
        if (EditorGUI.EndChangeCheck())
			_subSprites = EditorSpriteUtilities.GetSpritesFromTexture2D(_targetSpriteSheet);
		EditorGUILayout.LabelField(_subSpriteCountLabel, new GUIContent(_subSprites?.Length.ToString() ?? "0"));
        EditorGUI.BeginDisabledGroup(_subSprites == null || _subSprites.Length == 0);
        _spriteRowsColumns = EditorGUILayout.Vector2IntField(_spriteRowsColumnsLabel, _spriteRowsColumns);
        _spriteName = EditorGUILayout.TextField(_spriteNameLabel, _spriteName);
        GUILayout.Space(EditorGUIUtility.singleLineHeight);
        if (GUILayout.Button(_renameButtonLabel))
        {
            var formatString = _spriteName.Replace("{row}", "{0}").Replace("{column}", "{1}");
            var namePairs = GetSpriteNamePairs(_subSprites, _spriteRowsColumns, formatString);
            //See https://gist.github.com/edwardrowe/1b18a5c8fd180733a68f
            AmendMetaFile(_targetSpriteSheet, namePairs);
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// Get pairs of old and new names for each given sprite.
    /// </summary>
    /// <param name="sprites"></param>
    /// <param name="spriteRowsColumns"></param>
    /// <param name="formatString"></param>
    /// <returns></returns>
    static Dictionary<string, string> GetSpriteNamePairs(Sprite[] sprites, Vector2Int spriteRowsColumns, string formatString)
    {
        var result = new Dictionary<string, string>(sprites.Length);
        for (int i = 0; i < sprites.Length; i++)
        {
            var column = i % spriteRowsColumns.x;
            var row = i / spriteRowsColumns.x;
            result.Add(sprites[i].name, string.Format(formatString, row, column));
            Debug.Log($"{sprites[i].name}: {string.Format(formatString, row, column)}");
        }
        return result;
    }

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
