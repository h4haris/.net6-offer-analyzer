using OfferAnalyzer.DataInterface;
using OfferAnalyzer.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OfferAnalyzer.Services
{
    public class Api3Service : IApiService
    {
        private readonly string COMPANY = ApiServiceEnum.api3.ToString(); //API Owner [Company Name] or its identifier
        private const string API_URL = "http://localhost:8089/api3/v1/offer"; //API URL; should be fetched from appsettings or other source

        private readonly HttpClient _httpClient;

        public Api3ServiceRequest _api3ServiceRequest;

        public Api3Service(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void CreateRequest(string sourceAddress, string destinationAddress, Dimensions packageDimensions)
        {
            if (!string.IsNullOrEmpty(sourceAddress))
            {
                _api3ServiceRequest = new Api3ServiceRequest();

                _api3ServiceRequest.Source = sourceAddress;
                _api3ServiceRequest.Destination = destinationAddress;
                _api3ServiceRequest.Packages = new int[] { packageDimensions.Length, packageDimensions.Width, packageDimensions.Height };
            }
        }

        public Task<HttpResponseMessage> PostAsync(string sourceAddress, string destinationAddress, Dimensions packageDimensions)
        {
            CreateRequest(sourceAddress, destinationAddress, packageDimensions);

            var dataAsString = GetRequestXml();
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            _httpClient.DefaultRequestHeaders.Add("Accept", "application/xml");

            // Other API Credentials or Headers will be added here

            return _httpClient.PostAsync(API_URL, content);
        }

        public Offer GetOfferResponse(string dataString)
        {
            Offer offer = null;

            Console.WriteLine(dataString);

            try
            {
                XDocument resultXml = XDocument.Parse(dataString);
                var quote = resultXml.Root.Element("quote").Value;

                if (!string.IsNullOrEmpty(quote))
                {
                    offer = new Offer() { Quote = Convert.ToInt32(quote), Company = COMPANY };
                }
            }
            catch (Exception)
            {
                // Check for any error, log and perform any other task
            }

            return offer;
        }

        public string GetRequestXml()
        {
            StringBuilder xml = new StringBuilder();

            xml.Append("<xml>");

            xml.Append("<source>");
            xml.Append(_api3ServiceRequest.Source);
            xml.Append("</source>");

            xml.Append("<destination>");
            xml.Append(_api3ServiceRequest.Destination);
            xml.Append("</destination>");

            xml.Append("<packages>");

            xml.Append("<package>");
            xml.Append(_api3ServiceRequest.Packages[0]);
            xml.Append("</package>");
            xml.Append("<package>");
            xml.Append(_api3ServiceRequest.Packages[1]);
            xml.Append("</package>");
            xml.Append("<package>");
            xml.Append(_api3ServiceRequest.Packages[2]);
            xml.Append("</package>");

            xml.Append("</packages>");

            xml.Append("</xml>");

            return xml.ToString();
        }
    }
}
