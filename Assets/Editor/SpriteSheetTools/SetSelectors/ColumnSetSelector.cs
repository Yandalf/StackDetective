using System;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpritesheetTools
{
	/// <summary>
	/// Selects Sprite Sets in a Spritesheet by counting each column as a set.
	/// </summary>
	[Serializable]
	public sealed class ColumnSetSelector : RowColumnSetSelector
	{
		public override int GetSpriteSetIndex(Sprite sprite)
		{
			return GetSpriteColumnIndex(sprite, skippedColumnsIndices);
		}
		public override int GetSpriteSetSubIndex(Sprite sprite)
		{
			return GetSpriteRowIndex(sprite, skippedRowsIndices);
		}

		public override int GetSpriteAbsoluteIndex(Sprite sprite)
		{
			return GetSpriteColumnIndex(sprite, skippedColumnsIndices) * spriteRowsColumns.y + GetSpriteRowIndex(sprite, skippedRowsIndices);
		}
	}
}