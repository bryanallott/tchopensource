using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TchOpenSource.Lib
{
    class ByteArrayPdfSplitter : PdfSplitter
    {

        private MemoryStream currentOutputStream;

        public ByteArrayPdfSplitter(PdfDocument pdfDocument) : base(pdfDocument)
        {
        }

        protected override PdfWriter GetNextPdfWriter(PageRange documentPageRange)
        {
            currentOutputStream = new MemoryStream();
            return new PdfWriter(currentOutputStream);
        }

        public MemoryStream CurrentMemoryStream
        {
            get { return currentOutputStream; }
        }

        public class DocumentReadyListender : IDocumentReadyListener
        {

            private ByteArrayPdfSplitter splitter;

            public DocumentReadyListender(ByteArrayPdfSplitter splitter)
            {
                this.splitter = splitter;
            }

            public void DocumentReady(PdfDocument pdfDocument, PageRange pageRange)
            {
                pdfDocument.Close();
                byte[] contents = splitter.CurrentMemoryStream.ToArray();
                String pageNumber = pageRange.ToString();
            }
        }
    }
}