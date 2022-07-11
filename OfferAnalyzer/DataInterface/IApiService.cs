using OfferAnalyzer.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace OfferAnalyzer.DataInterface
{
    public interface IApiService
    {
        Task<HttpResponseMessage> PostAsync(string sourceAddress, string destinationAddress, Dimensions packageDimensions);
        Offer GetOfferResponse(string dataString);
    }
}
