using OfferAnalyzer.DataInterface;
using OfferAnalyzer.Models;
using OfferAnalyzer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OfferAnalyzer.Processor
{
    public class ApiProcessor : IApiProcessor
    {
        List<Offer> _offers;

        private readonly HttpClient _httpClient;

        public ApiProcessor(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Offer>> GetAllOffersAsync(string source, string destination, Dimensions cartons, string[] apiServices)
        {
            _offers = null;

            if (apiServices == null || apiServices.Length == 0)
            {
                return _offers;
            }

            List<ApiMapper> apiMapperList = new List<ApiMapper>();

            foreach (var apiServiceStr in apiServices)
            {
                Enum.TryParse(apiServiceStr, out ApiServiceEnum apiService);
                IApiService api;

                switch (apiService)
                {
                    case ApiServiceEnum.api1:
                        api = new Api1Service(_httpClient);
                        apiMapperList.Add(new ApiMapper() { ServiceApi = api, ServiceHttpResponse = api.PostAsync(source, destination, cartons) });
                        break;
                    case ApiServiceEnum.api2:
                        api = new Api2Service(_httpClient);
                        apiMapperList.Add(new ApiMapper() { ServiceApi = api, ServiceHttpResponse = api.PostAsync(source, destination, cartons) });
                        break;
                    case ApiServiceEnum.api3:
                        api = new Api3Service(_httpClient);
                        apiMapperList.Add(new ApiMapper() { ServiceApi = api, ServiceHttpResponse = api.PostAsync(source, destination, cartons) });
                        break;
                    case ApiServiceEnum.notSet:
                    default:
                        break;
                }
            }

            // Get all requests
            var requests = apiMapperList.Select(api => api.ServiceHttpResponse).ToList();

            // Wait for all the requests to finish
            await Task.WhenAll(requests);

            // Get the responses and map
            foreach (var apiMapper in apiMapperList)
            {
                // Extract the result
                var apiResult = apiMapper.ServiceHttpResponse.Result;
                var dataString = await apiResult.Content.ReadAsStringAsync();

                // Map response 
                var currentOffer = apiMapper.ServiceApi.GetOfferResponse(dataString);

                if (currentOffer != null)
                {
                    if (_offers == null)
                        _offers = new List<Offer>();

                    _offers.Add(currentOffer);
                }
            }

            return _offers;
        }
    }
}
