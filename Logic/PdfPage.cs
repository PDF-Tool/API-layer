using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Logic
{
    public class PDFPage
    {
        private readonly int _targetSizeBytes;
        private readonly byte[] _bitmapData;

        public PDFPage(int targetSizeBytes)
        {
            _targetSizeBytes = targetSizeBytes;
            int pixels = Math.Max(1, targetSizeBytes / 3);
            int width = Math.Max(1, (int)Math.Sqrt(pixels));
            int height = Math.Max(1, pixels / width);
            _bitmapData = new byte[width * height * 3];
            new Random().NextBytes(_bitmapData);
        }

        public byte[] GetUncompressedPngBytes()
        {
            int pixels = Math.Max(1, _targetSizeBytes / 3);
            int width = Math.Max(1, (int)Math.Sqrt(pixels));
            int height = Math.Max(1, pixels / width);

            using var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                System.Runtime.InteropServices.Marshal.Copy(_bitmapData, 0, bmpData.Scan0, _bitmapData.Length);
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            using var ms = new MemoryStream();
            var encoder = GetPngEncoder();
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, 0L);
            bmp.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }

        private static ImageCodecInfo GetPngEncoder()
        {
            var encoders = ImageCodecInfo.GetImageEncoders();
            foreach (var enc in encoders)
                if (enc.FormatID == ImageFormat.Png.Guid)
                    return enc;
            throw new Exception("PNG encoder not found");
        }
    }
}