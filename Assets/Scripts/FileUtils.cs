using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Settings
{
    internal class FileUtils
    {
        internal static List<Sprite> GetSpritesFromDirectory(DirectoryInfo OpenM)
        {
            List<Sprite> rtn = new List<Sprite>();
            if (OpenM != null)
                foreach (var file in OpenM.EnumerateFiles().Where(o => o.Name.EndsWith("png")))
                {
                    var tex = LoadPNG(file.FullName);
                    rtn.Add(Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f));
                }
            return rtn;
        }
        static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }
    }
}