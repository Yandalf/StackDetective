using com.SolePilgrim.Unity.Extensions.Attributes;
using UnityEditor;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.Extensions.Attributes
{
	[CustomPropertyDrawer(typeof(FilePathDialogAttribute))]
	public class FilePathDialogAttributeDrawer : PropertyDrawer
	{
		static private readonly GUIContent _buttonLabel = new("..", "Open project path browser");
		static private readonly Vector2 _pathButtonSize = new(20, EditorGUIUtility.singleLineHeight);


		#region IMGUI
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var shortenedRect = new Rect(position.position, position.size - new Vector2(_pathButtonSize.x, 0));
			var buttonRect = new Rect(position.position + new Vector2(shortenedRect.width, 0), _pathButtonSize);
			EditorGUI.PropertyField(shortenedRect, property, label);
			if (GUI.Button(buttonRect, _buttonLabel))
				OnClickPathSelectionButton(property);
		}
		#endregion IMGUI

		private void OnClickPathSelectionButton(SerializedProperty property)
		{
			var pathDialogAttr = attribute as FilePathDialogAttribute;
			var path = string.IsNullOrEmpty(property.stringValue) ? pathDialogAttr.Folder : property.stringValue;
			property.stringValue = FileUtil.GetProjectRelativePath(EditorUtility.OpenFilePanel(pathDialogAttr.Title, path, pathDialogAttr.Extension));
		}
	}
}