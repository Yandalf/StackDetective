using System.Linq;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpritesheetTools
{
	/// <summary>
	/// Base class for SetSelectors that rely on the Spritesheet's rows and columns.
	/// </summary>
	abstract public class RowColumnSetSelector : SetSelector
	{
		[Tooltip("Rows and columns of sprites in the sheet.")]
		public Vector2Int spriteRowsColumns;
		[Tooltip("0-based indices of rows that must be skipped in the spritesheet.")]
		public string skippedRows;
		[Tooltip("0-based indices of columns that must be skipped in the spritesheet.")]
		public string skippedColumns;


		/// <summary>
		/// Get the 0-based index of the sprite's row within its spritesheet.
		/// </summary>
		/// <param name="skipRows">If true the index will be reduced if there are empty rows before the sprite's row.</param>
		public int GetSpriteRowIndex(Sprite sprite, bool skipRows = true)
		{
			//By default Unity lists sprites from top to bottom, but texture space is bottom to top.
			//We recalculate the Y-Value of the Sprite origin to be top to bottom.
			var origin = -sprite.textureRect.y + sprite.texture.height - sprite.textureRect.height;
			var axisSize = sprite.textureRect.size.y;
			var index = Mathf.FloorToInt(origin/axisSize);
			if (skipRows)
			{
				var rowIndices = ParseNumberString(skippedRows);
				var skippedRowCount = rowIndices.Count(i => index > i);
				return index - skippedRowCount;
			}
			return index;
		}

		/// <summary>
		/// Get the 0-based index of the sprite's column within its spritesheet.
		/// </summary>
		/// <param name="skipColumns">If true the index will be reduced if there are empty columns before the sprite's column.</param>
		public int GetSpriteColumnIndex(Sprite sprite, bool skipColumns = true)
		{
			var origin = sprite.textureRect.x;
			var axisSize = sprite.textureRect.size.x;
			var index = Mathf.FloorToInt(origin / axisSize);
			if (skipColumns)
			{
				var columnIndices = ParseNumberString(skippedColumns);
				var skippedColumnCount = columnIndices.Count(i => index > i);
				return index - skippedColumnCount;
			}
			return index;
		}
	}
}