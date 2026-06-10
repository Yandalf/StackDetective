using System;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpritesheetTools
{
	/// <summary>
	/// Selects Sprite Sets in a Spritesheet by counting each row as a set.
	/// </summary>
	[Serializable]
	public sealed class RowSetSelector : RowColumnSetSelector
	{
		public override int GetSpriteSetIndex(Sprite sprite)
		{
			return GetSpriteRowIndex(sprite, skippedRowsIndices);
		}

		public override int GetSpriteSetSubIndex(Sprite sprite)
		{
			return GetSpriteColumnIndex(sprite, skippedColumnsIndices);
		}

		public override int GetSpriteAbsoluteIndex(Sprite sprite)
		{
			return GetSpriteRowIndex(sprite, skippedRowsIndices) * spriteRowsColumns.x + GetSpriteColumnIndex(sprite, skippedColumnsIndices);
		}
	}
}