using System;

namespace Logic
{
    public class PDFPage
    {
        private readonly PDFContent _content;
        private readonly int _targetContentSize;

        public PDFPage(int targetContentSizePerPage)
        {
            _targetContentSize = Math.Max(1, targetContentSizePerPage);
            _content = new PDFContent(_targetContentSize);
        }

        public byte[] GetBitmapData() => _content.GetBitmapData();
        public int Width => _content.Width;
        public int Height => _content.Height;
        public long GetActualContentSize() => _content.ActualBitmapDataSize;
        public int TargetContentSize => _targetContentSize;
    }
}