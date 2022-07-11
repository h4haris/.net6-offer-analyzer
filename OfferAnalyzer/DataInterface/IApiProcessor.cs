using OfferAnalyzer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OfferAnalyzer.DataInterface
{
    public interface IApiProcessor
    {
        Task<List<Offer>> GetAllOffersAsync(string source, string destination, Dimensions cartons, string[] apiServices);
    }
}
