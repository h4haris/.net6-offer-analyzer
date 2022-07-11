using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using OfferAnalyzer.Models;
using OfferAnalyzer.Services;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OfferAnalyzerTest.Services.Tests
{
    [TestClass]
    public class Api3ServiceTests
    {
        private OfferAnalyzerRequest _request;
        private Mock<HttpMessageHandler> _httpHandlerMock;
        private HttpClient _httpclient;
        private Api3Service _api3Service;

        [TestInitialize]
        public void Initialize()
        {
            _request = new OfferAnalyzerRequest()
            {
                SourceAddress = "MNO ST",
                DestinationAddress = "789 Apt",
                Carton = new Dimensions() { Width = 10, Height = 10, Length = 10 }
            };

            _httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _httpHandlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // Prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("<xml><quote>1569</quote></xml>"),
               })
               .Verifiable();

            _httpclient = new HttpClient(_httpHandlerMock.Object);
            _api3Service = new Api3Service(_httpclient);
        }

        [TestMethod]
        public void ShouldInitializeRequestUsingCreateRequestMethod()
        {
            // Initial Value Assert
            Assert.IsNull(_api3Service._api3ServiceRequest);

            // Act
            _api3Service.CreateRequest(
                    _request.SourceAddress,
                    _request.DestinationAddress,
                    _request.Carton);

            // Assert
            Assert.IsNotNull(_api3Service._api3ServiceRequest);
        }

        [TestMethod]
        public void ShouldCreateCorrectXmlRequest()
        {
            // Arrange
            StringBuilder expectedXml = new StringBuilder();

            expectedXml.Append("<xml>");

            expectedXml.Append("<source>");
            expectedXml.Append(_request.SourceAddress);
            expectedXml.Append("</source>");

            expectedXml.Append("<destination>");
            expectedXml.Append(_request.DestinationAddress);
            expectedXml.Append("</destination>");

            expectedXml.Append("<packages>");
            expectedXml.Append("<package>");
            expectedXml.Append(_request.Carton.Length);
            expectedXml.Append("</package>");
            expectedXml.Append("<package>");
            expectedXml.Append(_request.Carton.Width);
            expectedXml.Append("</package>");
            expectedXml.Append("<package>");
            expectedXml.Append(_request.Carton.Height);
            expectedXml.Append("</package>");
            expectedXml.Append("</packages>");

            expectedXml.Append("</xml>");

            // Act
            _api3Service.CreateRequest(
                    _request.SourceAddress,
                    _request.DestinationAddress,
                    _request.Carton);

            var actualXml = _api3Service.GetRequestXml();

            // Assert
            Assert.IsNotNull(_api3Service._api3ServiceRequest);
            Assert.AreEqual(actualXml, expectedXml.ToString());
        }

        [TestMethod]
        public void ShouldParseApiResponseCorrectly()
        {
            // Act
            var offer = _api3Service.GetOfferResponse("<xml><quote>2432</quote></xml>");

            // Assert
            Assert.IsNotNull(offer);
            Assert.IsInstanceOfType(offer, typeof(Offer));
            Assert.AreEqual(offer.Quote, 2432);
        }

        [TestMethod]
        public void ShouldReturnNullIfIncorrectApiResponseOrAnyError()
        {
            // Act
            var offer = _api3Service.GetOfferResponse("<xml><error>Server Error</error></xml>");

            // Assert
            Assert.IsNull(offer);
        }

        [TestMethod]
        public void ShouldCallApiWithCorrectVerb()
        {
            // Act
            var result = _api3Service.PostAsync(
                    _request.SourceAddress,
                    _request.DestinationAddress,
                    _request.Carton);

            // Assert
            _httpHandlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // expected a POST request
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        [TestMethod]
        public void ShouldCallApiWithCorrectHeaders()
        {
            // Act
            var result = _api3Service.PostAsync(
                    _request.SourceAddress,
                    _request.DestinationAddress,
                    _request.Carton);

            // Assert
            _httpHandlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // expected a POST request
                  && req.Content.Headers.GetValues("Content-Type").FirstOrDefault() == "application/xml" // expected "Content-Type" to "application/xml"
                  && req.Headers.GetValues("Accept").FirstOrDefault() == "application/xml" // expected "Accept" to "application/xml"
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
