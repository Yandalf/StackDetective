using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

static public class EditorSpriteUtilities
{
	static public Sprite[] GetSpritesFromTexture2D(Texture2D texture)
	{
		if (texture == null)
			return new Sprite[0];

		return OrderSpritesByTexturePlacement(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture))
			.Where(o => o is Sprite).Cast<Sprite>().ToArray());
	}

	static public Sprite[] OrderSpritesByTexturePlacement(Sprite[] sprites)
	{
		return sprites.OrderByDescending(s => s.rect.position.y).ThenBy(s => s.rect.position.x).ToArray();
	}

	static public void LogSprites(Sprite[] sprites, string leadingMessage = "")
	{
		var names = string.Join("\n", sprites.Select(s => s.name));
		Debug.Log(string.IsNullOrEmpty(leadingMessage) ? names : $"{leadingMessage}\n{names}");
	}

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

	//TODO this is now limited to Animation Clips modifying SpriteRenderer components. Does it make sense to allow component type configuration?
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
}
