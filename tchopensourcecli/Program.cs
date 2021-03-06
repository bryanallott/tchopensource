﻿using System;
using System.IO;
using System.Reflection;

namespace tchopensourcecli
{

    class Program
    {
        private static string GetAffidavit()
        {
            return GetFile("affidavit.html");
        }
        private static byte[] GetFileBytes(string resource)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream resFilestream = assembly.GetManifestResourceStream($"tchopensourcecli.resources.{resource}"))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    resFilestream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
        private static string GetFile(string resource)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream resFilestream = assembly.GetManifestResourceStream($"tchopensourcecli.resources.{resource}"))
            {
                using (var sr = new StreamReader(resFilestream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        private static string SaveFile(byte[] buffer)
        {
            string newfile = System.IO.Path.GetTempFileName() + ".pdf";
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream fs = new FileStream(newfile, FileMode.Create, System.IO.FileAccess.Write))
            {
                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
            }
            return newfile;
        }
        static void Main(string[] args)
        {
            var printer = new OpenSourcePdfPrinterClient();
            string htmlContent = GetAffidavit();
            byte[] affidavitPdf = printer.GetPdf(htmlContent);
            Console.WriteLine(SaveFile(affidavitPdf));

            byte[] file1 = GetFileBytes("preface.pdf");
            byte[] file2 = GetFileBytes("report.pdf");
            byte[] sticthedPdf = printer.StitchPdfs(file1, file2);
            Console.WriteLine(SaveFile(sticthedPdf));

        }
    }
}
