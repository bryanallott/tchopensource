using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using iText.IO.Source;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using TchOpenSource.Lib;

namespace TchOpenSource.Controllers
{
    public class PdfUtilController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Stitch(HttpPostedFileBase firstFile, HttpPostedFileBase secondFile)
        {
            string filename = $"{firstFile.FileName}-merged.pdf";
            using (MemoryStream ms = new MemoryStream())
            {
                using (PdfDocument pdf = new PdfDocument(new PdfWriter(ms)))
                {
                    using (PdfReader reader = new PdfReader(firstFile.InputStream))
                    {
                        PdfDocument srcDoc = new PdfDocument(reader);
                        srcDoc.CopyPagesTo(1, srcDoc.GetNumberOfPages(), pdf);
                    }

                    using (PdfReader reader = new PdfReader(secondFile.InputStream))
                    {
                        PdfDocument srcDoc = new PdfDocument(reader);
                        srcDoc.CopyPagesTo(1, srcDoc.GetNumberOfPages(), pdf);
                    }
                    pdf.Close();
                    return File(ms.ToArray(), "application/pdf", filename);
                }
            }
        }
        [HttpPost]
        public ActionResult Stitch2(HttpPostedFileBase firstFile, HttpPostedFileBase secondFile)
        {
            string filename = $"{firstFile.FileName}-merged.pdf";
            ByteArrayOutputStream baos = new ByteArrayOutputStream();

            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(firstFile.InputStream), new PdfWriter(baos)))
            {
                using (PdfDocument pdfDocument2 = new PdfDocument(new PdfReader(secondFile.InputStream)))
                {
                    PdfMerger merger = new PdfMerger(pdfDocument);
                    merger.Merge(pdfDocument2, 1, pdfDocument2.GetNumberOfPages());
                    pdfDocument2.Close();
                }
                //baos.Close();
                byte[] bytes = baos.ToArray();

                /*
                ByteArrayPdfSplitter splitter = new ByteArrayPdfSplitter(pdfDocument);
                splitter.SplitByPageCount(1, new ByteArrayPdfSplitter.DocumentReadyListender(splitter));

                List<byte> allPagesToBytes = new 
                foreach (PdfDocument splitPage in splitter.SplitByPageCount(1))
                {
                    PdfPage page = pdfDocument.GetPage(i);
                    byte[] bytes = page.GetContentBytes();
                }
                */
                //pdfDocument.Close();
                return File(bytes, "application/pdf", filename);
            }
        }
    }
}