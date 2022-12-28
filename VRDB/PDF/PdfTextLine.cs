namespace VRDB.PDF
{
    public class PdfTextLine
    {
        public int PageNumber { get; set; }
        public int LineNumber { get; set; }
        public string Content { get; set; }

        public PdfTextLine(int page, int line, string content)
        {
            PageNumber = page;
            LineNumber = line;
            Content = content;
        }
    }
}
