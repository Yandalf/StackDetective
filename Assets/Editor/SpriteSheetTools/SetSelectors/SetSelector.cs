using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpritesheetTools
{
	/// <summary>
	/// Base class for rules to define a Set of Sprites within a Spritesheet with.
	/// </summary>
	[Serializable]
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
		/// Sorts the Sprite array by Absolute Indices of the Sprites.
		/// </summary>
		public void OrderSpritesByAbsoluteIndex(Sprite[] sprites)
		{
			Array.Sort(sprites, this);
		}

		/// <summary>
		/// Returns the Set Index of the Sprite.
		/// </summary>
		abstract public int GetSpriteSetIndex(Sprite sprite);
		/// <summary>
		/// Returns the Index of the Sprite relative to other Sprites in the same Set.
		/// </summary>
		abstract public int GetSpriteSetSubIndex(Sprite sprite);
		/// <summary>
		/// Returns the Index of the Sprite unique from all Sprites in its source Texture.
		/// </summary>
		abstract public int GetSpriteAbsoluteIndex(Sprite sprite);

		public int Compare(Sprite x, Sprite y)
		{
			return GetSpriteAbsoluteIndex(x).CompareTo(GetSpriteAbsoluteIndex(y));
		}
	}
}