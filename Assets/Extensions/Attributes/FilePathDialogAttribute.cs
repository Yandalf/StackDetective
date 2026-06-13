using System;
using UnityEngine;

namespace com.SolePilgrim.Unity.Extensions.Attributes
{
	/// <summary>
	/// Attribute to mark a string as holding a Path to a file.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class FilePathDialogAttribute : PropertyAttribute 
	{
		public string Title { get; private set; }
		public string Extension { get; private set; }
		public string Folder { get; private set; }


		/// <summary>
		/// Marks a string as holding a path to a file.
		/// </summary>
		/// <param name="title">Title for Editor Path Dialogs.</param>
		/// <param name="extension">Extension of the file to use.</param>
		/// <param name="folder">Default folder for the dialogue to open with.</param>
		public FilePathDialogAttribute(string title, string extension, string folder = "Assets")
		{
			Title = title;
			Extension = extension;
			Folder = folder;
		}
	}
}