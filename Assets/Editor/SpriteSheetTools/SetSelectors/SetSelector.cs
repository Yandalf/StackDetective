using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpritesheetTools
{
	/// <summary>
	/// Base class for rules to define a Set of Sprites within a Spritesheet with.
	/// </summary>
	abstract public class SetSelector : IComparer<Sprite>
	{
		/// <summary>
		/// Divides the given Sprites into Sets by SpriteSetIndex.
		/// </summary>
		/// <returns>Dictionary of Sprite Sets, each key is the Set's index, values are the Sprites in each set sorted by SetSubIndex.</returns>
		public Dictionary<int, Sprite[]> SplitSpritesInSets(Sprite[] sprites)
		{
			if (!SpritesheetUtilities.SpritesShareOriginTexture(sprites))
				throw new ArgumentException("Sprites must share the originTexture!", nameof(sprites));

			return sprites.OrderBy(sprite => GetSpriteAbsoluteIndex(sprite))
				.GroupBy(sprite => GetSpriteSetIndex(sprite))
				.ToDictionary(group => group.Key, group => group.ToArray());
		}

		/// <summary>
		/// Returns the Set Index of the Sprite from all Sprites in its source Texture based on the Selector.
		/// </summary>
		abstract public int GetSpriteSetIndex(Sprite sprite);
		/// <summary>
		/// Returns the Index of te Sprite from all Sprites in its source Texture based on the Selector.
		/// </summary>
		abstract public int GetSpriteAbsoluteIndex(Sprite sprite);

		public int Compare(Sprite x, Sprite y)
		{
			return GetSpriteAbsoluteIndex(x).CompareTo(GetSpriteAbsoluteIndex(y));
		}
	}

	/// <summary>
	/// Selects Sprite Sets in a Spritesheet by counting each row as a set.
	/// </summary>
	public sealed class RowSetSelector : SetSelector
	{
		public override int GetSpriteSetIndex(Sprite sprite)
		{
			//By default Unity lists sprites from top to bottom, but texture space is bottom to top.
			//We recalculate the Y-Value of the Sprite origin to be top to bottom.
			var origin = -sprite.textureRect.y + sprite.texture.height - sprite.textureRect.height; 
			var axisSize = sprite.textureRect.size.y;
			return Mathf.FloorToInt(origin / axisSize);
		}

		public override int GetSpriteAbsoluteIndex(Sprite sprite)
		{
			throw new System.NotImplementedException();
		}
	}

	/// <summary>
	/// Selects Sprite Sets in a Spritesheet by counting each column as a set.
	/// </summary>
	public sealed class ColumnSetSelector : SetSelector
	{
		public override int GetSpriteSetIndex(Sprite sprite)
		{
			var origin = sprite.textureRect.x;
			var axisSize = sprite.textureRect.size.x;
			return Mathf.FloorToInt(origin / axisSize);
		}

		public override int GetSpriteAbsoluteIndex(Sprite sprite)
		{
			throw new System.NotImplementedException();
		}
	}
}