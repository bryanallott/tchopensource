using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace tchopensourcecli
{
    public class OpenSourcePdfPrinterClient
    {
        readonly string BaseUrl = "https://localhost:44300/";
        public async Task<byte[]> GetPdf(string html)
        {
            RestRequest request = new RestRequest("/PdfUtils/MakePdf", Method.Post);
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("Content", html, ParameterType.GetOrPost);
            return await Execute(request);
        }

        public async Task<byte[]> StitchPdfs(byte[] file1, byte[] file2)
        {
            RestRequest request = new RestRequest("/PdfUtils/Stitch", Method.Post);
            //request.AddHeader("content-type", "multipart/form-data");
            //request.AlwaysMultipartFormData = true;
            request.AddFile("firstFile", file1, "first.pdf");
            //request.AddFile("firstFile", @"C:\\Users\\bryan\\source\\repos\\tchopensource\\tchopensourcecli\\bin\\Debug\\netcoreapp3.1\\report.pdf");
            request.AddFile("secondFile", file2, "second.pdf");
            request.AddParameter("Action", "stitch");
            return await Execute(request);
        }

        //https://stackoverflow.com/questions/11513607/restsharp-unable-to-cast-object-of-system-string-to-type-system-generic-idicti
        /*In that code path the library attempts to assign your value to an IDictionary<string,object> regardless of what type it is.*/
        public async Task<byte[]> Execute(RestRequest request)
        {
            var client = new RestClient(BaseUrl);
            var response = await client.ExecuteAsync(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                throw new ApplicationException(message, response.ErrorException);
            }
            return response.RawBytes;
        }
    }
}
