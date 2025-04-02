using System;

public class PdfPage
{
    private PDFContent _content;
    private int _targetContentSize; // Target size contribution for generating this page's content

    public PdfPage(int targetContentSizePerPage)
    {
        // Store the target size used for generation, although estimation uses actual size later
        _targetContentSize = Math.Max(1, targetContentSizePerPage); // Ensure at least 1
        _content = new PDFContent(_targetContentSize);
    }

    public byte[] GetContentImageData()
    {
        return _content.GetImageData();
    }

    public long GetActualContentImageDataSize()
    {
        return _content.ActualImageDataSize;
    }

    public int TargetContentSize => _targetContentSize; // Expose if needed
}