using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
		/// <exception cref="ArgumentException">Throws exception when the sprites don't share the same origin Texture.</exception>
		public Dictionary<int, Sprite[]> SplitSpritesInSets(Sprite[] sprites)
		{
			if (!SpritesheetUtilities.SpritesShareOriginTexture(sprites))
				throw new ArgumentException("Sprites must share the originTexture!", nameof(sprites));

			return sprites.OrderBy(sprite => GetSpriteAbsoluteIndex(sprite))
				.GroupBy(sprite => GetSpriteSetIndex(sprite))
				.ToDictionary(group => group.Key, group => group.ToArray());
		}

		/// <summary>
		/// Returns true if all the Sprites are part of a single set in the same origin Texture.
		/// </summary>
		public bool AreSpritesSingleSet(Sprite[] sprites)
		{
			if (!SpritesheetUtilities.SpritesShareOriginTexture(sprites))
				return false;
			return sprites.All(s => GetSpriteSetIndex(s) == GetSpriteSetIndex(sprites.First()));
		}

		/// <summary>
		/// Sorts the Sprite array by Absolute Indices of the Sprites.
		/// </summary>
		public void OrderSpritesByAbsoluteIndex(Sprite[] sprites)
		{
			Array.Sort(sprites, this);
		}

		//TODO this could probably be moved to a Utility somewhere.
		/// <summary>
		/// Parses a list of numbers to an array.
		/// </summary>
		/// <param name="sort">If true the list will be sorted in ascending order.</param>
		public int[] ParseNumberString(string numberString, bool sort = true)
		{
			var result = Regex.Matches(numberString, @"\d+").
				Cast<Match>().
				Select(m => int.Parse(m.Value)).
				OrderBy(i => i).
				ToArray();
			if (sort)
				Array.Sort(result);
			return result;
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