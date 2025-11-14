using System.Collections.Generic;
using UnityEngine;

namespace Mint.Gdk.Utilities.Runtime
{
    public enum ColorSampleType
    {
        Average,
        Dominant
    }
    public static class Texture2DExtensions
    {
        public static Color SampleColor(this Texture2D source, Vector2Int sampleSize, ColorSampleType sampleType = ColorSampleType.Average)
        {
            int width = sampleSize.x;
            int height = sampleSize.y;

            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0);
            RenderTexture.active = renderTexture;

            Graphics.Blit(source, renderTexture);

            Texture2D temp = new(width, height, TextureFormat.RGB24, false);
            temp.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
            temp.Apply();

            RenderTexture.ReleaseTemporary(renderTexture);
            RenderTexture.active = null;

            Color resultColor;
            switch (sampleType)
            {
                case ColorSampleType.Dominant:
                    resultColor = GetDominantColor(temp);
                    break;
                case ColorSampleType.Average:
                default:
                    resultColor = GetAverageColor(temp);
                    break;
            }

            Object.Destroy(temp);
            return resultColor;
        }
        private static Color GetAverageColor(Texture2D tex)
        {
            Color[] pixels = tex.GetPixels();
            Color averageColor = new(0f, 0f, 0f, 0f);

            if (pixels.Length == 0) return averageColor;

            foreach (Color color in pixels)
            {
                averageColor += color;
            }

            return averageColor / pixels.Length;
        }
        private static Color GetDominantColor(Texture2D tex)
        {
            Color32[] pixels = tex.GetPixels32();

            if (pixels.Length == 0) return Color.black;

            Dictionary<Color32, int> colorCounts = new();

            Color32 dominantColor = pixels[0];
            int maxCount = 0;

            foreach (Color32 color in pixels)
            {
                if (color.a == 0) continue;

                if (colorCounts.ContainsKey(color))
                {
                    colorCounts[color]++;
                }
                else
                {
                    colorCounts[color] = 1;
                }

                if (colorCounts[color] > maxCount)
                {
                    maxCount = colorCounts[color];
                    dominantColor = color;
                }
            }

            return dominantColor;
        }
    }
}
