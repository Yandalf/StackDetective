using System;
using UnityEngine;

namespace com.SolePilgrim.Unity.Extensions.Attributes
{
	/// <summary>
	/// Attribute to mark a string as holding a Path to a folder.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class FolderPathDialogAttribute : PropertyAttribute
	{
		public string Title { get; private set; }
		public string Folder { get; private set; }
		public string DefaultName { get; private set; }

		/// <summary>
		/// Marks a string property as holding a path to a folder.
		/// </summary>
		/// <param name="title">Title for Editor Path Dialogs.</param>
		/// <param name="folder">Starting Folder, relative to project folder.</param>
		/// <param name="defaultName">Default name for folders, can be empty.</param>
		public FolderPathDialogAttribute(string title, string folder = "Assets", string defaultName = "")
		{
			Title = title;
			Folder = folder;
			DefaultName = defaultName;
		}
	}
}