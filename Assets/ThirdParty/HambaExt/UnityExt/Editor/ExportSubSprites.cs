using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityExt
{
    public class ExportSubSprites : Editor
    {
        [MenuItem("Assets/Export Sub-Sprites")]
        public static void DoExportSubSprites()
        {
            var folder = EditorUtility.OpenFolderPanel("Export subsprites into what folder?", "", "");
            foreach (var obj in Selection.objects)
            {
                var sprite = obj as Sprite;
                if (sprite == null) continue;
                var extracted = ExtractAndName(sprite);
                SaveSubSprite(extracted, folder);
            }
        }

        [MenuItem("Assets/Export Sub-Sprites", true)]
        static bool CanExportSubSprites()
        {
            return Selection.activeObject is Sprite;
        }

        // Since a sprite may exist anywhere on a tex2d, this will crop out the sprite's claimed region and return a new, cropped, tex2d.
        static Texture2D ExtractAndName(Sprite sprite)
        {
            //var output = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var r = sprite.textureRect;
            int width = (int)r.width;
            int height = (int)r.height;
            int x = (int)r.x;
            int y = (int)r.y;
            var output = new Texture2D(width, height);
            Debug.Log("x: " + x + " y: " + y + " width: " + width + " height: " + height +
                " output.width: " + output.width + " and output.height: " + output.height);
            var pixels = sprite.texture.GetPixels(x, y, width, height);
            output.SetPixels(pixels);
            output.Apply();
            output.name = sprite.name;
            AssetDatabase.Refresh();
            return output;
        }
        static void SaveSubSprite(Texture2D tex, string saveToDirectory)
        {
            if (!System.IO.Directory.Exists(saveToDirectory)) System.IO.Directory.CreateDirectory(saveToDirectory);
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(saveToDirectory, tex.name + ".png"), tex.EncodeToPNG());
        }
    }
}