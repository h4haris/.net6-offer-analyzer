namespace OfferAnalyzer.Models
{
    public class OfferAnalyzerRequest
    {
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public Dimensions Carton { get; set; }
    }
}
