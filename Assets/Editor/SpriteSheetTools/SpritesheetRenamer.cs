using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.SolePilgrim.Unity.Editor.SpritesheetTools
{
    /// <summary>
    /// Editor tool for quickly renaming all sprites in a spritesheet following a scheme.
    /// </summary>
    public sealed class SpritesheetRenamer : EditorWindow
    {
        static readonly GUIContent _explanationLabel = new("Automatically renames sprites in a spritesheet asset file following a given naming scheme.\nUse this to easily group sets of sprites in large sheets.");
        static readonly GUIContent _spritesheetLabel = new("Spritesheet", "Spritesheet Texture2D asset of which the sub-sprites have to be renamed.");
        static readonly GUIContent _selectorLabel = new("Selector", "Selectors define how Sprites are ordered within a spritesheet.");
        static readonly GUIContent _subSpriteCountLabel = new("Sprite Count", "Sprites in the sheet.");
        static readonly GUIContent _spriteNameLabel = new("Sprites Name", "Name to give each sprite. Insert indices with {setIndex} and {setSubIndex}.");
        static readonly GUIContent _renameButtonLabel = new("Rename", "Rename all the sprites.");

        public Texture2D targetSpritesheet;
        [SerializeReference]
        public SetSelector setSelector;
        public string spriteName;
        private readonly SetSelector[] _selectors;
        private readonly GUIContent[] _selectorsDisplayOptions;
        private Sprite[] _subSprites;


		public SpritesheetRenamer()
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

		[MenuItem("Tools/Sprites/Spritesheet Renamer")]
        private static void OpenWindow()
        {
            const float wndWidth = 400.0f;
            const float wndHeight = 240.0f;
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
                _subSprites = SpritesheetUtilities.GetSpritesFromTexture2D(targetSpritesheet);
            }
            EditorGUI.BeginDisabledGroup(_subSprites == null || _subSprites.Length == 0);
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
            EditorGUILayout.LabelField(_subSpriteCountLabel, new GUIContent(_subSprites?.Length.ToString() ?? "0"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(spriteName)), _spriteNameLabel);
            if (EditorGUI.EndChangeCheck())
            {
				serializedObject.ApplyModifiedProperties();
				setSelector.skippedRowsIndices = setSelector.ParseNumberString(setSelector.skippedRows);
				setSelector.skippedColumnsIndices = setSelector.ParseNumberString(setSelector.skippedColumns);
			}
			GUILayout.Space(EditorGUIUtility.singleLineHeight);
            if (GUILayout.Button(_renameButtonLabel))
            {
                setSelector.skippedColumnsIndices = setSelector.ParseNumberString(setSelector.skippedColumns);
                setSelector.skippedRowsIndices = setSelector.ParseNumberString(setSelector.skippedRows).OrderBy(i => i).ToArray();
				setSelector.OrderSpritesByAbsoluteIndex(_subSprites);
                var formatString = spriteName.Replace("{setIndex}", "{0}").Replace("{setSubIndex}", "{1}");
                var namePairs = GetSpriteNamePairs(_subSprites, setSelector, formatString);
                //See https://gist.github.com/edwardrowe/1b18a5c8fd180733a68f
                AmendMetaFile(targetSpritesheet, namePairs);
            }
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Get pairs of old and new names for each given sprite.
        /// </summary>
        static Dictionary<string, string> GetSpriteNamePairs(Sprite[] sprites, SetSelector selector, string formatString)
        {
            //TODO some indices can skip values, see if there's a way to work around this. Perhaps we can parse scripting instructions from the formatString?
            var result = new Dictionary<string, string>(sprites.Length);
            for (int i = 0; i < sprites.Length; i++)
            {
                var index = selector.GetSpriteSetIndex(sprites[i]);
                var subIndex = selector.GetSpriteSetSubIndex(sprites[i]);
                result.Add(sprites[i].name, string.Format(formatString, index, subIndex));
            }
            return result;
        }

        /// <summary>
        /// Update the meta file with the new names by going line per line.
        /// </summary>
        static void AmendMetaFile(Object asset, Dictionary<string, string> renamePairs)
        {
            var assetPath = AssetDatabase.GetAssetPath(asset);
            var pathToTextureMetaFile = assetPath + ".meta";
            var lines = File.ReadAllLines(pathToTextureMetaFile);
            var spriteDefinitionRegex = new Regex(@"name: (?'spriteName'.+)$");
            var nameFileIdTableRegex = new Regex(@"(?'spriteName'.+): -?\d+$"); //This will basically capture ANY KeyValuePair in the meta file, so we will need to check its capturegroup content first!
            for (int i = 0; i < lines.Length; i++)
            {
                var spriteDefinitionMatch = spriteDefinitionRegex.Match(lines[i]);
                var nameFileIdTableMatch = nameFileIdTableRegex.Match(lines[i]);
                if (spriteDefinitionMatch.Success)
                {
                    var foundName = spriteDefinitionMatch.Groups["spriteName"].Value;
                    lines[i] = spriteDefinitionRegex.Replace(lines[i], m => { return m.Value.Replace(foundName, renamePairs[foundName]); });
                }
                else if (nameFileIdTableMatch.Success && renamePairs.Keys.Contains(nameFileIdTableMatch.Groups["spriteName"].Value))
                {
                    var foundName = nameFileIdTableMatch.Groups["spriteName"].Value;
                    lines[i] = nameFileIdTableRegex.Replace(lines[i], m => { return m.Value.Replace(foundName, renamePairs[foundName]); });
                }
            }
            var originalFileAttributes = File.GetAttributes(pathToTextureMetaFile);
            File.SetAttributes(pathToTextureMetaFile, originalFileAttributes & ~FileAttributes.Hidden);
            File.WriteAllText(pathToTextureMetaFile, string.Join('\n', lines));
            File.SetAttributes(pathToTextureMetaFile, originalFileAttributes);
            AssetDatabase.Refresh();
        }
    }
}