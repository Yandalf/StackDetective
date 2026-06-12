using System;
using System.Linq;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpritesheetTools
{
	/// <summary>
	/// Base class for SetSelectors that rely on the Spritesheet's rows and columns.
	/// </summary>
	abstract public class RowColumnSetSelector : SetSelector
	{
		static readonly GUIContent _spriteRowsColumnsLabel = new("Rows & Columns");

		[Tooltip("Rows and columns of sprites in the sheet.")]
		public Vector2Int spriteRowsColumns;


		public int GetSpriteRowIndex(Sprite sprite, int[] skippedRows = null)
		{
			//By default Unity lists sprites from top to bottom, but texture space is bottom to top.
			//We recalculate the Y-Value of the Sprite origin to be top to bottom.
			var origin = -sprite.textureRect.y + sprite.texture.height - sprite.textureRect.height;
			var axisSize = sprite.textureRect.size.y;
			var index = Mathf.FloorToInt(origin/axisSize);
			if (skippedRows != null)
			{
				var lastIndex = Array.IndexOf(skippedRows, skippedRows.LastOrDefault(i => index > i));
				return lastIndex >= 0 ? index - (lastIndex + 1) : index;
			}
			return index;
		}

		public int GetSpriteColumnIndex(Sprite sprite, int[] skippedColumns = null)
		{
			var origin = sprite.textureRect.x;
			var axisSize = sprite.textureRect.size.x;
			var index = Mathf.FloorToInt(origin / axisSize);
			if (skippedColumns != null)
			{
				var lastIndex = Array.IndexOf(skippedColumns, skippedColumns.LastOrDefault(i => index > i));
				return lastIndex >= 0 ? index - (lastIndex + 1) : index;
			}
			return index;
		}
	}
}