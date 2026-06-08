using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.SolePilgrim.Unity.Editor.SpriteSheetTools
{
	static public class SpritesheetUtilities
	{
		static public Sprite[] GetSpritesFromTexture2D(Texture2D texture)
		{
			if (texture == null)
				return new Sprite[0];

			return OrderSpritesByTexturePlacement(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture))
				.Where(o => o is Sprite).Cast<Sprite>().ToArray(), SpritesheetSetDirection.Rows);
		}

		/// <summary>
		/// Orders a set of Sprites based on their rectangle position within their original Texture.
		/// </summary>
		/// <param name="sortDirection">Primary direction of the sprites within the original Texture.</param>
		static public Sprite[] OrderSpritesByTexturePlacement(Sprite[] sprites, SpritesheetSetDirection sortDirection)
		{
			if (sortDirection == SpritesheetSetDirection.Rows)
				return sprites.OrderByDescending(s => s.rect.position.y).ThenBy(s => s.rect.position.x).ToArray();
			else
				return sprites.OrderBy(s => s.rect.position.x).ThenByDescending(s => s.rect.position.y).ToArray();
		}

		/// <summary>
		/// Returns true if all Sprites come from the same Texture.
		/// </summary>
		static public bool SpritesShareOriginTexture(Sprite[] sprites)
		{
			return sprites.All(s => s.texture == sprites[0].texture);
		}

		/// <summary>
		/// Log all the Sprite names in a single formatted debug message.
		/// </summary>
		static public void LogSprites(Sprite[] sprites, string leadingMessage = "")
		{
			var names = string.Join("\n", sprites.Select(s => s.name));
			Debug.Log(string.IsNullOrEmpty(leadingMessage) ? names : $"{leadingMessage}\n{names}");
		}

		/// <summary>
		/// Retrieve all Sprites used in the given AnimationClip.
		/// </summary>
		public static List<Sprite> GetSpritesFromClip(AnimationClip clip)
		{
			var sprites = new List<Sprite>();
			foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
			{
				var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
				foreach (var frame in keyframes)
					sprites.Add((Sprite)frame.value);
			}
			return sprites;
		}

		/// <summary>
		/// Sorts the given sprites into indexed sets.
		/// </summary>
		public static Dictionary<int, Sprite[]> OrderSpritesInSets(Sprite[] sprites, SpritesheetSetDirection setDirection)
		{
			var result = new Dictionary<int, Sprite[]>();
			foreach (var sprite in sprites)
			{
				var index = GetSpriteSetIndex(sprite, setDirection);
				if (result.ContainsKey(index))
					result[index] = OrderSpritesByTexturePlacement(result[index].Append(sprite).ToArray(), setDirection);
				else
					result.Add(index, new Sprite[] { sprite });
			}
			return result;
		}

		//TODO this is now limited to Animation Clips modifying SpriteRenderer components. Does it make sense to allow component type configuration?
		/// <summary>
		/// Replaces all the Sprites in the AnimationClip with provided Sprites.
		/// </summary>
		/// <param name="spriteReplacements">Map of old Sprites and their replacements. Key is old Sprite, value is replacement Sprite.</param>
		public static void ReplaceSpritesInClip(Dictionary<Sprite, Sprite> spriteReplacements, AnimationClip clip)
		{
			foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
			{
				if (binding.type != typeof(SpriteRenderer))
					continue;
				var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
				for (int i = 0; i < keyframes.Length; i++)
					keyframes[i].value = spriteReplacements[(Sprite)keyframes[i].value];
				AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
			}
		}

		/// <summary>
		/// Gets the set index of the given sprite based on its position in the spritesheet.
		/// </summary>
		private static int GetSpriteSetIndex(Sprite sprite, SpritesheetSetDirection setDirection)
		{
			var origin = setDirection == SpritesheetSetDirection.Rows ?
				sprite.textureRect.y * -1 + sprite.texture.height - sprite.textureRect.height : //Recalculate because by default Unity lists sprites from top to bottom, but texture space is bottom to top
				sprite.textureRect.x;
			var axisSize = setDirection == SpritesheetSetDirection.Rows ?
				sprite.textureRect.size.y :
				sprite.textureRect.size.x;
			return Mathf.FloorToInt(origin / axisSize);
		}
	}
}
