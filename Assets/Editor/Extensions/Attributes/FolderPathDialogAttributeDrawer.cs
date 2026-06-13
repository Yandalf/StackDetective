using com.SolePilgrim.Unity.Extensions.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.SolePilgrim.Unity.Editor.Extensions.Attributes
{
	[CustomPropertyDrawer(typeof(FolderPathDialogAttribute))]
	public class FolderPathDialogAttributeDrawer : PropertyDrawer
	{
		static private readonly GUIContent _buttonLabel = new("..", "Open project path browser");
		static private readonly Vector2 _pathButtonSize = new(20, EditorGUIUtility.singleLineHeight);

		//TODO UIToolkit Implementation
		#region UIToolkit
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{        
			// Create property container element.
			var container = new VisualElement();

			// Create property fields.
			var pathField = new PropertyField(property);
			var selectionButton = new Button(() => OnClickPathSelectionButton(property));
			// Add fields to the container.
			container.Add(pathField);
			container.Add(selectionButton);
			return container;

			return base.CreatePropertyGUI(property);
		}
		#endregion UIToolkit
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
			var pathDialogAttr = attribute as FolderPathDialogAttribute;
			var path = string.IsNullOrEmpty(property.stringValue) ? pathDialogAttr.Folder : property.stringValue[..property.stringValue.LastIndexOf('/')];
			var name = string.IsNullOrEmpty(property.stringValue) ? pathDialogAttr.DefaultName : property.stringValue[(property.stringValue.LastIndexOf('/') + 1)..];
			property.stringValue = FileUtil.GetProjectRelativePath(EditorUtility.OpenFolderPanel(pathDialogAttr.Title, path, name));
		}
	}
}