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
			return GetSpriteRowIndex(sprite);
		}

		public override int GetSpriteSetSubIndex(Sprite sprite)
		{
			return GetSpriteColumnIndex(sprite);
		}

		public override int GetSpriteAbsoluteIndex(Sprite sprite)
		{
			return GetSpriteRowIndex(sprite) * spriteRowsColumns.x + GetSpriteColumnIndex(sprite);
		}
	}
}