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
			return GetSpriteColumnIndex(sprite);
		}
		public override int GetSpriteSetSubIndex(Sprite sprite)
		{
			return GetSpriteRowIndex(sprite);
		}

		public override int GetSpriteAbsoluteIndex(Sprite sprite)
		{
			return GetSpriteColumnIndex(sprite) * spriteRowsColumns.y + GetSpriteRowIndex(sprite);
		}
	}
}