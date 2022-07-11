using Newtonsoft.Json;
using OfferAnalyzer.DataInterface;
using OfferAnalyzer.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OfferAnalyzer.Services
{
    public class Api1Service : IApiService
    {
        private readonly string COMPANY = ApiServiceEnum.api1.ToString(); //API Owner [Company Name] or its identifier
        private const string API_URL = "http://localhost:8089/api1/v1/offer"; //API URL; should be fetched from appsettings or other source

        private readonly HttpClient _httpClient;

        public Api1ServiceRequest _api1ServiceRequest;

        public Api1Service(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void CreateRequest(string sourceAddress, string destinationAddress, Dimensions packageDimensions)
        {
            if (!string.IsNullOrEmpty(sourceAddress))
            {
                _api1ServiceRequest = new Api1ServiceRequest();

                _api1ServiceRequest.ContactAddress = sourceAddress;
                _api1ServiceRequest.WarehouseAddress = destinationAddress;
                _api1ServiceRequest.PackageDimensions = new int[] { packageDimensions.Length, packageDimensions.Width, packageDimensions.Height };
            }
        }

        public Task<HttpResponseMessage> PostAsync(string sourceAddress, string destinationAddress, Dimensions packageDimensions)
        {

            CreateRequest(sourceAddress, destinationAddress, packageDimensions);

            var dataAsString = JsonConvert.SerializeObject(_api1ServiceRequest);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Other API Credentials or Headers will be added here

            return _httpClient.PostAsync(API_URL, content);
        }

        public Offer GetOfferResponse(string dataString)
        {
            Offer offer = null;

            Console.WriteLine(dataString);

            try
            {
                var result = JsonConvert.DeserializeObject<Api1ServiceResponse>(dataString);

                if (result != null && !string.IsNullOrEmpty(result.Total))
                {
                    offer = new Offer() { Quote = Convert.ToInt32(result.Total), Company = COMPANY };
                }
            }
            catch (Exception)
            {
                // Check for any error, log and perform any other task
            }

            return offer;
        }
    }
}
