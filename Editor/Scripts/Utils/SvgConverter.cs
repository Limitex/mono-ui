#if UNITY_EDITOR

using System;
using System.IO;
using UnityEngine;
using SkiaSharp;
using Svg.Skia;

namespace Limitex.MonoUI.Editor.Utils
{
    public class SvgConverter : IDisposable
    {
        private SKBitmap _bitmap;
        private SKCanvas _canvas;
        private SKPaint _paint;

        public SvgConverter(int width, int height)
        {
            _bitmap = new SKBitmap(width, height);
            _canvas = new SKCanvas(_bitmap);
            _paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateBlendMode(SKColors.White, SKBlendMode.SrcIn)
            };
        }

        public void Dispose()
        {
            _bitmap?.Dispose();
            _canvas?.Dispose();
            _paint?.Dispose();
        }

        public void SetPaintColor(string hexString)
        {
            _paint.ColorFilter = SKColorFilter.CreateBlendMode(SKColor.Parse(hexString), SKBlendMode.SrcIn);
        }

        public void LoadSvg(string svgPath, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            if (!File.Exists(svgPath))
                throw new FileNotFoundException($"SVG file not found: {svgPath}");

            var loader = new SKSvg();
            loader.Load(svgPath);

            if (loader.Picture == null)
                throw new Exception("Failed to load SVG picture.");

            scaleX *= _bitmap.Width / loader.Picture.CullRect.Width;
            scaleY *= _bitmap.Height / loader.Picture.CullRect.Height;
            var matrix = SKMatrix.CreateScale(scaleX, scaleY);

            _canvas.Clear(SKColors.Transparent);
            _canvas.DrawPicture(loader.Picture, matrix, _paint);
            _canvas.Flush();
        }

        public byte[] GetBytes(int quality = 100)
        {
            using var image = SKImage.FromBitmap(_bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, quality);
            return data.ToArray();
        }

        public void SaveToFile(string filePath, int quality = 100)
        {
            var bytes = GetBytes(quality);
            File.WriteAllBytes(filePath, bytes);
        }

        public static string ColorToHexString(Color color)
        {
            return $"#{Mathf.RoundToInt(color.r * 255):X2}{Mathf.RoundToInt(color.g * 255):X2}{Mathf.RoundToInt(color.b * 255):X2}{Mathf.RoundToInt(color.a * 255):X2}";
        }
    }
}

#endif
