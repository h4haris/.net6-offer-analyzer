using OfferAnalyzer.DataInterface;
using OfferAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfferAnalyzer.Processor
{
    public class OfferDataAnalyzer
    {
        private readonly string[] apiServices = { ApiServiceEnum.api1.ToString(), ApiServiceEnum.api2.ToString(), ApiServiceEnum.api3.ToString() }; //APIs to call; should be fetched from appsettings or other source

        private readonly IApiProcessor _apiProcessor;

        public OfferDataAnalyzer(IApiProcessor apiProcessor)
        {
            _apiProcessor = apiProcessor;
        }

        public async Task<Offer> ProcessRequest(OfferAnalyzerRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (string.IsNullOrEmpty(request.SourceAddress))
                throw new ArgumentNullException("request.SourceAddress");

            if (string.IsNullOrEmpty(request.DestinationAddress))
                throw new ArgumentNullException("request.DestinationAddress");

            if (request.Carton == null)
                throw new ArgumentNullException("request.Carton");

            var availableOffers = await _apiProcessor.GetAllOffersAsync(request.SourceAddress, request.DestinationAddress, request.Carton, apiServices);

            Offer bestDeal = GetBestDeal(availableOffers);

            return bestDeal;
        }

        private Offer GetBestDeal(List<Offer> availableOffers)
        {
            Offer bestDeal = null;

            if (availableOffers != null && availableOffers.Count() > 0)
            {
                // Take min quote offer, if multiple offers found it will take first or can be modified to return multiple values 
                bestDeal = availableOffers.OrderBy(offer => offer.Quote).First();
            }

            return bestDeal;
        }
    }
}
