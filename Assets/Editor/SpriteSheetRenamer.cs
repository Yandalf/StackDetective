using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class SpriteSheetRenamer : EditorWindow
{
    static readonly GUIContent _explanationLabel = new("Automatically renames sprites in a sprite sheet asset file following a given naming scheme. Use this to easily group sets of sprites in large sheets.");
    static readonly GUIContent _spriteSheetLabel = new("Sprite Sheet", "Sprite Sheet asset of which the sub-sprites have to be renamed.");
    static readonly GUIContent _subSpriteCountLabel = new("Sprite Count", "Sprites in the sheet.");
    static readonly GUIContent _spriteRowsColumnsLabel = new("Rows & Columns", "Rows and columns of sprites in the sheet.");
    static readonly GUIContent _spriteNameLabel = new("Sprites Name", "Name to give each sprite. Insert row and column numbers with {row} and {column}.");
    static readonly GUIContent _renameButtonLabel = new("Rename", "Rename all the sprites.");
    private Texture2D _targetSpriteSheet;
    private Sprite[] _subSprites;
    private Vector2Int _spriteRowsColumns;
    private string _spriteName;


    [MenuItem("Tools/Sprites/Sprite Sheet Renamer")]
    private static void OpenWindow()
    {
        const float wndWidth = 400.0f;
        const float wndHeight = 200.0f;
        var pos = new Vector2(0.5f * (Screen.currentResolution.width - wndWidth),
                              0.5f * (Screen.currentResolution.height - wndHeight));
        var window = GetWindow<SpriteSheetRenamer>();
        window.titleContent = new GUIContent(ObjectNames.NicifyVariableName(nameof(SpriteSheetRenamer)));
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
        {
            if (_targetSpriteSheet == null)
                _subSprites = new Sprite[0];
            else
            {
                _subSprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_targetSpriteSheet))
                    .Where(o => o is Sprite).Cast<Sprite>()
                    .OrderByDescending(s => s.rect.position.y).ThenBy(s => s.rect.position.x)
                    .ToArray();
                Debug.Log(string.Join("\n", _subSprites.Select(s => s.name)));
            }
        }
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
