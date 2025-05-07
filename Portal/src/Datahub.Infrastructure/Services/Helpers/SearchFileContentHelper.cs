using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf; 

namespace Datahub.Infrastructure.Services.Helpers
{
    public static class SearchFileContentHelper
    {
        public static bool SearchWordDocument(Stream stream, string searchTerm)
        {
            using var wordDoc = WordprocessingDocument.Open(stream, false);
            var bodyText = wordDoc.MainDocumentPart?.Document.Body?.InnerText;
            return bodyText != null && bodyText.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
        }

        public static  bool SearchPdfDocument(Stream stream, string searchTerm)
        {
            using var pdfReader = new PdfReader(stream);
            using var pdfDoc = new PdfDocument(pdfReader);
            var strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy();

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);
                if (pageText.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
