using SkiaSharp;
using System;
using System.IO;

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
        int pixelCount = Math.Max(1, targetBytes / 3);
        int dimension = Math.Max(1, (int)Math.Sqrt(pixelCount));

        // Use RGB (no alpha) for slightly more predictable size
        var info = new SKImageInfo(dimension, dimension, SKColorType.Rgb888x, SKAlphaType.Opaque);
        byte[] generatedData = null;

        try
        {
            using var surface = SKSurface.Create(info);
            if (surface == null) throw new Exception($"SKSurface.Create returned null for {dimension}x{dimension}");

            using var canvas = surface.Canvas;
            canvas.Clear(SKColors.White); // Consistent white background

            var random = new Random();
            byte[] buffer = new byte[3];

            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    random.NextBytes(buffer);
                    var color = new SKColor(buffer[0], buffer[1], buffer[2]);
                    using var paint = new SKPaint { Color = color };
                    canvas.DrawPoint(x, y, paint);
                }
            }

            using var image = surface.Snapshot();
            if (image == null) throw new Exception("surface.Snapshot returned null");

            // Encode to PNG (Quality setting has minor effect on PNG size but keep 100)
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            if (data == null || data.IsEmpty) throw new Exception("image.Encode returned null or empty data");

            generatedData = data.ToArray();

            // Debug logging
            // Console.WriteLine($" -> Target: {targetBytes} bytes, Actual PNG: {generatedData.Length} bytes (Dim: {dimension}x{dimension})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating image data (Target: {targetBytes} bytes): {ex.Message}. Generating fallback 1x1 black pixel PNG.");
            // Fallback: Generate a minimal, predictable PNG (1x1 black pixel)
            var fallbackInfo = new SKImageInfo(1, 1, SKColorType.Rgb888x, SKAlphaType.Opaque);
            using var fallbackSurface = SKSurface.Create(fallbackInfo);
            using var fallbackCanvas = fallbackSurface.Canvas;
            fallbackCanvas.Clear(SKColors.Black);
            using var fallbackImage = fallbackSurface.Snapshot();
            using var fallbackData = fallbackImage.Encode(SKEncodedImageFormat.Png, 100);
            generatedData = fallbackData.ToArray();
        }
        return generatedData;
    }

    public byte[] GetImageData()
    {
        return _imageData;
    }

    public long ActualImageDataSize => _imageData?.Length ?? 0;
}