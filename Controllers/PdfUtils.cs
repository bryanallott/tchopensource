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
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.IO.Image;
using iText.Layout.Element;
using iText.Pdfa;
using iText.StyledXmlParser.Jsoup.Nodes;
using System.Drawing.Imaging;
using QRCoder;
using ConvergencyBloc.AzureBlob;
using TchOpenSource.Models;

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
        public ActionResult Compress()
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
        public ActionResult HandleCompress(List<HttpPostedFileBase> Files)
        {
            List<Tuple<byte[], string>> files = new List<Tuple<byte[], string>>();
            foreach (var file in Files)
            {
                using (PdfReader pdfReader = new PdfReader(file.InputStream))
                {
                    WriterProperties writerProperties = new WriterProperties();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (PdfWriter pdfWriter = new PdfWriter(ms, writerProperties))
                        {
                            pdfWriter.SetCompressionLevel(9);
                            PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                            pdfDocument.Close();
                            files.Add(new Tuple<byte[], string>(ms.ToArray(), file.FileName));
                        }
                    }
                }
            }
            if (1 < files.Count)
            {
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
                        return File(result.ToArray(), "application/zip", "compressedbundle.zip");
                    }
                }
            }
            else
            {
                return File(files[0].Item1, "application/pdf", files[0].Item2);
            }
        }
        [HttpPost]
        public ActionResult ExtractText(List<HttpPostedFileBase> Files)
        {
            List<Tuple<byte[], string>> files = new List<Tuple<byte[], string>>();
            foreach (var file in Files)
            {
                files.Add(new Tuple<byte[], string>(GetContents(file.InputStream), file.FileName));
            }
            if (1 < files.Count)
            {
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
            else
            {
                return File(files[0].Item1, "application/pdf", files[0].Item2);
            }
        }

        public ActionResult Initial()
        {
            return View();
        }
        [HttpPost]
        public ActionResult InitialPages(List<HttpPostedFileBase> Files, bool Higher, bool Left)
        {
            List<Tuple<byte[], string>> files = new List<Tuple<byte[], string>>();
            var pdfFiles = Files.Where(x => x.FileName.EndsWith("pdf"));
            var signatureImage = Files.FirstOrDefault(x => !x.FileName.EndsWith("pdf"));

            ImageData imageData = null;
            using (MemoryStream msimg = new MemoryStream())
            {
                signatureImage.InputStream.CopyTo(msimg);
                msimg.Position = 0;
                imageData = ImageDataFactory.CreatePng(msimg.ToArray());
            }
            foreach (var pdfFile in pdfFiles)
            {
                using (PdfReader pdfReader = new PdfReader(pdfFile.InputStream))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (PdfWriter pdfWriter = new PdfWriter(ms))
                        {
                            PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                            iText.Layout.Document document = new iText.Layout.Document(pdfDocument);

                            for (int PageNr = 1; PageNr <= pdfDocument.GetNumberOfPages(); PageNr++)
                            {
                                float right = pdfDocument.GetDefaultPageSize().GetRight();
                                float bottom = pdfDocument.GetDefaultPageSize().GetBottom();
                                if(Higher)
                                {
                                    bottom += 32;
                                }
                                if (Left)
                                {
                                    right -= 32;
                                }
                                Image image = new Image(imageData)
                                        .ScaleToFit(32, 32);
                                image.SetFixedPosition(PageNr, right - 48, bottom + 32);
                                document.Add(image);
                            }
                            document.Close();
                            pdfWriter.Close();

                            files.Add(new Tuple<byte[], string>(ms.ToArray(), pdfFile.FileName));
                        }
                    }
                }
            }
            if (1 < files.Count)
            {
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
            else
            {
                return File(files[0].Item1, "application/pdf", files[0].Item2);
            }
        }

        [HttpPost]
        public ActionResult AddSignature(List<HttpPostedFileBase> Files, int PageNr)
        {
            var pdfFile = Files.FirstOrDefault(x => x.FileName.EndsWith("pdf"));
            var signatureImage = Files.FirstOrDefault(x => !x.FileName.EndsWith("pdf"));

            using (PdfReader pdfReader = new PdfReader(pdfFile.InputStream))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (PdfWriter pdfWriter = new PdfWriter(ms))
                    {
                        PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                        iText.Layout.Document document = new iText.Layout.Document(pdfDocument);

                        using(MemoryStream msimg = new MemoryStream())
                        {
                            signatureImage.InputStream.CopyTo(msimg);
                            ImageData imageData = ImageDataFactory.Create(msimg.ToArray());

                            Image image = new Image(imageData)
                                    .ScaleToFit(100, 200)
                                    .SetFixedPosition(PageNr, 25, 25);

                            document.Add(image);
                            document.Close();
                        }
                        return File(ms.ToArray(), "application/pdf", pdfFile.FileName);
                    }
                }
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult MakePdf(string Content, string Password, string PasswordEdit)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                WriterProperties writerProperties = new WriterProperties();
                if (!string.IsNullOrEmpty(Password) ||
                    !string.IsNullOrEmpty(PasswordEdit))
                {
                    writerProperties.SetStandardEncryption(
                        string.IsNullOrEmpty(Password) ? null : UTF8Encoding.UTF8.GetBytes(Password),
                        string.IsNullOrEmpty(PasswordEdit) ? null : UTF8Encoding.UTF8.GetBytes(PasswordEdit), 
                        EncryptionConstants.ALLOW_PRINTING | EncryptionConstants.ALLOW_FILL_IN, 
                        EncryptionConstants.ENCRYPTION_AES_128);
                }
                using (var writer = new PdfWriter(ms, writerProperties))
                {
                    writer.SetCompressionLevel(9);
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
        public ActionResult MakeProtectionBundle(List<HttpPostedFileBase> Files, string Password, string PasswordEdit)
        {
            List<Tuple<byte[], string>> files = new List<Tuple<byte[], string>>();
            foreach (var file in Files)
            {
                using (PdfReader pdfReader = new PdfReader(file.InputStream))
                {
                    WriterProperties writerProperties = new WriterProperties();
                    writerProperties.SetStandardEncryption(
                        string.IsNullOrEmpty(Password) ? null : UTF8Encoding.UTF8.GetBytes(Password),
                        string.IsNullOrEmpty(PasswordEdit) ? null : UTF8Encoding.UTF8.GetBytes(PasswordEdit), 
                        EncryptionConstants.ALLOW_PRINTING 
                        | EncryptionConstants.ALLOW_FILL_IN
                        | EncryptionConstants.ALLOW_COPY                       
                        , EncryptionConstants.ENCRYPTION_AES_128);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (PdfWriter pdfWriter = new PdfWriter(ms, writerProperties))
                        {
                            pdfWriter.SetCompressionLevel(9);
                            PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                            pdfDocument.Close();
                            files.Add(new Tuple<byte[], string>(ms.ToArray(), file.FileName));
                        }
                    }
                }
            }
            if (1 < files.Count)
            {
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
            else
            {
                return File(files[0].Item1, "application/pdf", files[0].Item2);
            }
        }
		[HttpPost]
        public ActionResult Readonly(HttpPostedFileBase firstFile)
        {
			string DefaultPassword = "OpenS0urcePdfUtilsH@sSup3rL0ngPasswords4Humans";
            string filename = $"{firstFile.FileName}-readonly.pdf";
            using (MemoryStream ms = new MemoryStream())
            {
				WriterProperties writerProperties = new WriterProperties();
                writerProperties.SetStandardEncryption(null, UTF8Encoding.UTF8.GetBytes(DefaultPassword), EncryptionConstants.ALLOW_PRINTING | EncryptionConstants.ALLOW_FILL_IN, EncryptionConstants.ENCRYPTION_AES_128);
                using (PdfDocument pdf = new PdfDocument(new PdfWriter(ms, writerProperties)))
                {
                    pdf.GetWriter().SetCompressionLevel(9);
                    using (PdfReader reader = new PdfReader(firstFile.InputStream))
                    {
                        PdfDocument srcDoc = new PdfDocument(reader);
                        srcDoc.CopyPagesTo(1, srcDoc.GetNumberOfPages(), pdf);
                    }
                    pdf.Close();
                    return File(ms.ToArray(), "application/pdf", filename);
                }
            }
        }
        public ActionResult Stamp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Stamped(HttpPostedFileBase pdfFile, string IssuedBy)
        {
            using (PdfReader pdfReader = new PdfReader(pdfFile.InputStream))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (PdfWriter pdfWriter = new PdfWriter(ms))
                    {
                        PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                        iText.Layout.Document document = new iText.Layout.Document(pdfDocument);

                        using (MemoryStream msimg = new MemoryStream())
                        {
                            Guid instance = Guid.NewGuid();
                            string fullurl = Url.Action("Verify", "Home", new { @area = string.Empty, @id = instance }, "https");
                            QRCodeData data = new QRCodeGenerator().CreateQrCode(
                                $"{{ 'id': '{instance}', 'issuer': '{IssuedBy}', 'stamped': '{DateTime.UtcNow.ToString("d MMM yyyy HH:mm:ss")}', 'author': 'The Convergency Hub PDF Verifier', 'url': '{fullurl}' }}", QRCoder.QRCodeGenerator.ECCLevel.Q);
                            BitmapByteQRCode qrCode = new BitmapByteQRCode(data);

                            ImageData imageData = ImageDataFactory.Create(qrCode.GetGraphic(40));

                            float left = pdfDocument.GetDefaultPageSize().GetLeft();
                            float bottom = pdfDocument.GetDefaultPageSize().GetBottom();
                            Image image = new Image(imageData)
                                    .ScaleToFit(80, 80)
                                    .SetFixedPosition(1, left, bottom);
                            document.Add(image);
                            document.Close();
                            pdfWriter.Close();

                            using (Stream copy = new MemoryStream(ms.ToArray()))
                            {
                                ConvergencyCloudFile uploadedSOA = GetCloudFileHandler()
                                    .UploadSingleFile("stamped"
                                    , instance.ToString()
                                    , copy);
                            }
                        }
                        return File(ms.ToArray(), "application/pdf", $"stamped-{pdfFile.FileName}");
                    }
                }
            }
        }
        protected CloudFileHandler GetCloudFileHandler()
        {
            return new CloudFileHandler(OpenSourceConfig.CloudStorageConnectionString, OpenSourceConfig.CloudStorageContainerReference);
        }
        [HttpPost]
        public ActionResult Stitch(HttpPostedFileBase firstFile, HttpPostedFileBase secondFile)
        {
            string filename = $"{firstFile.FileName}-merged.pdf";
            using (MemoryStream ms = new MemoryStream())
            {
                using (PdfDocument pdf = new PdfDocument(new PdfWriter(ms)))
                {
                    pdf.GetWriter().SetCompressionLevel(9);
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