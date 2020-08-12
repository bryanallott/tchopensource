using System;
using System.Collections.Generic;
using System.Text;

namespace tchopensourcecli
{
    public class OpenSourcePdfPrinterClient
    {
        readonly string BaseUrl = "https://localhost:44384/";
        public byte[] GetPdf(string html)
        {
            RestSharp.RestRequest request = new RestSharp.RestRequest("/PdfUtils/MakePdf", RestSharp.Method.POST);
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("Content", html, RestSharp.ParameterType.GetOrPost);
            return Execute(request);
        }

        public byte[] StitchPdfs(byte[] file1, byte[] file2)
        {
            RestSharp.RestRequest request = new RestSharp.RestRequest("/PdfUtils/Stitch", RestSharp.Method.POST);
            request.AddHeader("content-type", "multipart/form-data");
            request.AddFile("firstFile", file1, "first.pdf");
            request.AddFile("secondFile", file2, "second.pdf");
            return Execute(request);
        }

        //https://stackoverflow.com/questions/11513607/restsharp-unable-to-cast-object-of-system-string-to-type-system-generic-idicti
        /*In that code path the library attempts to assign your value to an IDictionary<string,object> regardless of what type it is.*/
        public byte[] Execute(RestSharp.RestRequest request)
        {
            var client = new RestSharp.RestClient();
            client.BaseUrl = new Uri(BaseUrl);
            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                throw new ApplicationException(message, response.ErrorException);
            }
            return response.RawBytes;
        }
    }
}
