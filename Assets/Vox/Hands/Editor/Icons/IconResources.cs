using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Vox.Hands
{
	public class IconResources : ScriptableObject
	{
		private static string BasePath
		{
			get
			{
				var obj = CreateInstance<IconResources>();
				var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(obj));
				DestroyImmediate(obj);

				var fileInfo = new FileInfo(scriptPath);
				var baseDir = fileInfo.Directory.ToString().Replace('\\', '/');
				var index = baseDir.LastIndexOf("Assets/");
				baseDir = baseDir.Substring(index);

				return baseDir;
			}
		}

		public const string kICON_HAND_BOTH = "hands_both.png";
		public const string kICON_HAND_LEFT = "hands_left.png";
		public const string kICON_HAND_RIGHT = "hands_right.png";
		public const string kICON_FOCUS = "focus.png";
		
		public static Texture2D LoadTextureFromFile(string path) {
			Texture2D texture = new Texture2D(1, 1);
			texture.LoadImage(File.ReadAllBytes(path));
			return texture;
		}
	
		public static Texture2D LoadTextureIconsDir(string path)
		{
			return LoadTextureFromFile(string.Format("{0}/{1}", BasePath, path));
		}	
	}
}