using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vox.Hands
{
    public static class TextureUtility
    {
        private static Texture2D CreateDstTexture(Texture2D src, float scale)
        {
            var width = (int) (src.width * scale);
            var height = (int) (src.height * scale);
            return new Texture2D(width, height);
        }

        public static Texture2D CreateScaledTexture(Texture2D src, float scale)
        {
            var dst = CreateDstTexture(src, scale);
            var dstPix = new Color[dst.width * dst.height];
            var y = 0;
            while (y < dst.height)
            {
                var x = 0;
                while (x < dst.width)
                {
                    var xFrac = x * 1.0f / (dst.width - 1);
                    var yFrac = y * 1.0f / (dst.height - 1);
                    dstPix[y * dst.width + x] = src.GetPixelBilinear(xFrac, yFrac);
                    ++x;
                }

                ++y;
            }

            dst.SetPixels(dstPix);
            dst.Apply();

            return dst;
        }
    }
}
