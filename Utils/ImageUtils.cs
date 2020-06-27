using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace InstaminiWebService.Utils
{
    /// <summary>
    /// Handle image resize by providing the target width
    /// Running on Linux requires installing libgdiplus
    /// </summary>
    public static class ImageUtils
    {
        private const int DEFAULT_WIDTH = 2560;

        public static Bitmap ResizeImage(Image image, int desiredWidth = DEFAULT_WIDTH)
        {
            var currentWidth = image.Width;
            float scaleRatio = (float)desiredWidth / (float)currentWidth;
            var desiredHeight = (int)(image.Height * scaleRatio);
            Console.WriteLine($"{desiredWidth}x{desiredHeight}");
            var destRect = new Rectangle(0, 0, desiredWidth, desiredHeight);
            var destImage = new Bitmap(desiredWidth, desiredHeight);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                SetDefaultGraphicsParameters(graphics);

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return destImage;
        }

        private static void SetDefaultGraphicsParameters(Graphics graphics)
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        public static FileStream ReadFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public static FileStream CreateOutputStream(string path)
        {
            // Automatically override
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
        }

        public static void ResizeThenSaveImage(string path, string outputPath)
        {
            using var fileStream = ReadFile(path);
            Image originalImage = Image.FromStream(fileStream);
            var transformedBitmap = ResizeImage(originalImage);
            using var outputStream = CreateOutputStream(outputPath);
            transformedBitmap.Save(outputStream, ImageFormat.Jpeg);
        }
    }
}
