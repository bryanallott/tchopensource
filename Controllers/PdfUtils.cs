using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ICSharpCode.SharpZipLib.Zip;
using iText.IO.Source;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Html2pdf;
using TchOpenSource.Lib;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Geom;
using iText.StyledXmlParser.Css.Media;

namespace TchOpenSource.Controllers
{
    public class PdfUtilsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Make()
        {
            return View();
        }
        public ActionResult Protect()
        {
            return View();
        }
        public ActionResult Extract()
        {
            return View();
        }

        private byte[] GetContents(Stream inputstream)
        {
            StringBuilder sb = new StringBuilder();
            using (PdfReader pdfReader = new PdfReader(inputstream))
            {
                using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
                {
                    LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();
                    PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                    for (int ii = 1; ii <= pdfDoc.GetNumberOfPages(); ii++)
                    {
                        parser.ProcessPageContent(pdfDoc.GetPage(ii));
                        sb.Append(strategy.GetResultantText());
                    }
                    pdfDoc.Close();
                }
            }
            return UTF8Encoding.UTF8.GetBytes(sb.ToString());
        }
        [HttpPost]
        public ActionResult ExtractText(List<HttpPostedFileBase> Files)
        {
            List<Tuple<byte[], string>> files = new List<Tuple<byte[], string>>();
            foreach (var file in Files)
            {
                files.Add(new Tuple<byte[], string>(GetContents(file.InputStream), file.FileName));
            }
            using (MemoryStream result = new MemoryStream())
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(result))
                {
                    zipStream.SetLevel(3);
                    foreach (var entry in files)
                    {
                        ZipEntry zipEntry = new ZipEntry(ZipEntry.CleanName($"{entry.Item2}.txt"));
                        //zipEntry.Size = entry.Item2.Length;
                        zipStream.PutNextEntry(zipEntry);
                        zipStream.Write(entry.Item1, 0, entry.Item1.Length);
                        zipStream.CloseEntry();
                    }
                    zipStream.Finish();
                    zipStream.Close();
                    return File(result.ToArray(), "application/zip", "protectedbundle.zip");
                }
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult MakePdf(string Content, string Password)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                WriterProperties writerProperties = new WriterProperties();
                if (!string.IsNullOrEmpty(Password))
                {
                    writerProperties.SetStandardEncryption(UTF8Encoding.UTF8.GetBytes(Password), null, EncryptionConstants.ALLOW_PRINTING | EncryptionConstants.ALLOW_FILL_IN, EncryptionConstants.ENCRYPTION_AES_128);
                }
                using (var writer = new PdfWriter(ms, writerProperties))
                {
                    PdfDocument pdf = new PdfDocument(writer);
                    pdf.SetTagged();
                    pdf.SetDefaultPageSize(PageSize.A4);

                    ConverterProperties properties = new ConverterProperties();
                    //properties.setBaseUri(baseUri);
                    MediaDeviceDescription mediaDeviceDescription = new MediaDeviceDescription(MediaType.SCREEN);
                    mediaDeviceDescription.SetWidth(PageSize.A4.GetWidth());
                    properties.SetMediaDeviceDescription(mediaDeviceDescription);

                    HtmlConverter.ConvertToPdf(Content, pdf, properties);

                    //HtmlConverter.ConvertToPdf(Content, writer);
                    writer.Close();
                }
                return File(ms.ToArray(), "application/pdf", "mynewpdf.pdf");
            }
        }
        [HttpPost]
        public ActionResult MakeProtectionBundle(List<HttpPostedFileBase> Files, string Password)
        {
            List<Tuple<byte[], string>> files = new List<Tuple<byte[], string>>();
            foreach (var file in Files)
            {
                PdfReader pdfReader = new PdfReader(file.InputStream);
                WriterProperties writerProperties = new WriterProperties();
                writerProperties.SetStandardEncryption(UTF8Encoding.UTF8.GetBytes(Password), null, EncryptionConstants.ALLOW_PRINTING | EncryptionConstants.ALLOW_FILL_IN, EncryptionConstants.ENCRYPTION_AES_128);

                using (MemoryStream ms = new MemoryStream())
                {
                    PdfWriter pdfWriter = new PdfWriter(ms, writerProperties);
                    PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                    pdfDocument.Close();
                    files.Add(new Tuple<byte[], string>(ms.ToArray(), file.FileName));
                }
            }
            using (MemoryStream result = new MemoryStream())
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(result))
                {
                    zipStream.SetLevel(3);
                    foreach (var entry in files)
                    {
                        ZipEntry zipEntry = new ZipEntry(ZipEntry.CleanName(entry.Item2));
                        //zipEntry.Size = entry.Item2.Length;
                        zipStream.PutNextEntry(zipEntry);
                        zipStream.Write(entry.Item1, 0, entry.Item1.Length);
                        zipStream.CloseEntry();
                    }
                    zipStream.Finish();
                    zipStream.Close();
                    return File(result.ToArray(), "application/zip", "protectedbundle.zip");
                }
            }
        }
        [HttpPost]
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