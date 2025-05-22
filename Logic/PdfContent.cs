using System;
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

        private readonly byte[] _bitmapData;
        private readonly int _width;
        private readonly int _height;
        private readonly int _targetContentBytes;

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
            
            // Calculate dimensions for uncompressed bitmap
            // Each pixel is 3 bytes (RGB)
            int totalPixels = Math.Max(1, _targetContentBytes / 3);
            _width = Math.Max(1, (int)Math.Sqrt(totalPixels));
            _height = Math.Max(1, totalPixels / _width);
            
            _bitmapData = GenerateBitmapData();
        }

        private byte[] GenerateBitmapData()
        {
            try
            {
                // Create uncompressed bitmap data (RGB format)
                byte[] bitmapData = new byte[_width * _height * 3];
                var random = new Random();

                // Fill with random RGB data
                for (int i = 0; i < bitmapData.Length; i += 3)
                {
                    random.NextBytes(bitmapData.AsSpan(i, 3));
                }

                return bitmapData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating bitmap data: {ex.Message}. Generating fallback 1x1 black pixel.");
                return new byte[] { 0, 0, 0 }; // Single black pixel
            }
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

        public byte[] GetBitmapData() => _bitmapData;
        public int Width => _width;
        public int Height => _height;
        public long ActualBitmapDataSize => _bitmapData?.Length ?? 0;
    }
}