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

		return AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture))
			.Where(o => o is Sprite).Cast<Sprite>()
			.OrderByDescending(s => s.rect.position.y).ThenBy(s => s.rect.position.x)
			.ToArray();
	}

	static public void LogSprites(Sprite[] sprites)
	{
		Debug.Log(string.Join("\n",sprites.Select(s => s.name)));
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

	public static void ReplaceSpritesInClip(Dictionary<Sprite, Sprite> spriteReplacements, AnimationClip clip)
	{
		foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
		{
			if (binding.type != typeof(Sprite))
				continue;
			var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
			for (int i = 0; i < keyframes.Length; i++)
				keyframes[i].value = spriteReplacements[(Sprite)keyframes[i].value];
		}
	}
}
