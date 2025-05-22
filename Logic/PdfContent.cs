using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Logic
{
    public class PDFContent
    {
        // --- Size Estimation Constants ---
        // These values NEED TUNING based on testing!
        // Increase these if your estimates are consistently too low, decrease if too high.

        // Overhead per page (page object, content stream, resources dictionary, maybe font refs)
        public const int ESTIMATED_PDF_PAGE_OVERHEAD_BYTES = 750; // Increased starting estimate
        // Fixed overhead (catalog, info, cross-references, trailer, basic doc structure)
        public const int ESTIMATED_PDF_FIXED_OVERHEAD_BYTES = 3000; // Increased starting estimate

        // Factor for embedding already compressed data (PNG stream object syntax, filters, etc.)
        // Should be slightly > 1.0. Adjust if content size contribution is off.
        public const float CONTENT_EMBEDDING_FACTOR = 1.03f;
        // ---------------------------------------------------------------------------

        private byte[] _imageData;
        private int _targetContentBytes;

        public PDFContent(int targetContentBytes)
        {
            if (targetContentBytes <= 0)
            {
                // Ensure we generate *something* even if the target is invalid,
                // otherwise downstream calculations might fail.
                Console.WriteLine($"Warning: Invalid target content size {targetContentBytes}. Generating minimal 1x1 pixel image.");
                _targetContentBytes = 1; // Set a minimal valid internal target
            }
            else
            {
                _targetContentBytes = targetContentBytes;
            }
            _imageData = GenerateImageDataInternal(_targetContentBytes);
        }


        public static long CalculateExpectedTotalSize(int pages, long totalActualContentDataBytes)
        {
            if (pages <= 0) return 0;

            // Calculate overhead and embedded content size
            long estimatedEmbeddedContentSize = (long)(totalActualContentDataBytes * CONTENT_EMBEDDING_FACTOR);
            long totalOverhead = ESTIMATED_PDF_FIXED_OVERHEAD_BYTES + (long)pages * ESTIMATED_PDF_PAGE_OVERHEAD_BYTES;

            long estimatedTotalSize = totalOverhead + estimatedEmbeddedContentSize;

            return estimatedTotalSize;
        }

        private static byte[] GenerateImageDataInternal(int targetBytes)
        {
            int width = 256;
            int height = 256;

            using (Bitmap bmp = new Bitmap(width, height))
            using (MemoryStream ms = new MemoryStream())
            {
                Random rand = new Random();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color randomColor = Color.FromArgb(
                            rand.Next(256),
                            rand.Next(256),
                            rand.Next(256)
                        );
                        bmp.SetPixel(x, y, randomColor);
                    }
                }

                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray(); // Return image as byte array
            }
        }

        public byte[] GetImageData()
        {
            return _imageData;
        }

        public long ActualImageDataSize => _imageData?.Length ?? 0;
    }
}