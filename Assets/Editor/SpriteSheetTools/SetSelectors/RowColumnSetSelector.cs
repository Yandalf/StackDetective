using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpritesheetTools
{
	/// <summary>
	/// Base class for SetSelectors that rely on the Spritesheet's rows and columns.
	/// </summary>
	abstract public class RowColumnSetSelector : SetSelector
	{
		static readonly GUIContent _spriteRowsColumnsLabel = new("Rows & Columns", "Rows and columns of sprites in the sheet.");

		public Vector2Int spriteRowsColumns;


		//TODO implement skippedRows
		public int GetSpriteRowIndex(Sprite sprite, int[] skippedRows = null)
		{
			//By default Unity lists sprites from top to bottom, but texture space is bottom to top.
			//We recalculate the Y-Value of the Sprite origin to be top to bottom.
			var origin = -sprite.textureRect.y + sprite.texture.height - sprite.textureRect.height;
			var axisSize = sprite.textureRect.size.y;
			return Mathf.FloorToInt(origin / axisSize);
		}

		//TODO implement skippedColumns
		public int GetSpriteColumnIndex(Sprite sprite, int[] skippedColumns = null)
		{
			var origin = sprite.textureRect.x;
			var axisSize = sprite.textureRect.size.x;
			return Mathf.FloorToInt(origin / axisSize);
		}
	}
}