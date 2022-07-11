namespace OfferAnalyzer.Services
{
    public class Api2ServiceRequest
    {
        public string Consignee { get; set; }
        public string Consignor { get; set; }
        public int[] Cartons { get; set; }
    }
}
