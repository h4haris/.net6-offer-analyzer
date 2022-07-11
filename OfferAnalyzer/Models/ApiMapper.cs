using OfferAnalyzer.DataInterface;
using System.Net.Http;
using System.Threading.Tasks;

namespace OfferAnalyzer.Models
{
    public class ApiMapper
    {
        public Task<HttpResponseMessage> ServiceHttpResponse { get; set; }
        public IApiService ServiceApi { get; set; }
    }
}
