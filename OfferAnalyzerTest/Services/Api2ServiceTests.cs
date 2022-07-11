using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using OfferAnalyzer.Models;
using OfferAnalyzer.Services;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OfferAnalyzerTest.Services.Tests
{
    [TestClass]
    public class Api2ServiceTests
    {
        private OfferAnalyzerRequest _request;
        private Mock<HttpMessageHandler> _httpHandlerMock;
        private HttpClient _httpclient;
        private Api2Service _api2Service;

        [TestInitialize]
        public void Initialize()
        {
            _request = new OfferAnalyzerRequest()
            {
                SourceAddress = "XYZ ST",
                DestinationAddress = "456 Apt",
                Carton = new Dimensions() { Width = 8, Height = 8, Length = 8 }
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
                   Content = new StringContent("{'amount': 1735}"),
               })
               .Verifiable();

            _httpclient = new HttpClient(_httpHandlerMock.Object);
            _api2Service = new Api2Service(_httpclient);
        }

        [TestMethod]
        public void ShouldInitializeRequestUsingCreateRequestMethod()
        {
            // Initial Value Assert
            Assert.IsNull(_api2Service._api2ServiceRequest);

            // Act
            _api2Service.CreateRequest(
                    _request.SourceAddress,
                    _request.DestinationAddress,
                    _request.Carton);

            // Assert
            Assert.IsNotNull(_api2Service._api2ServiceRequest);
        }

        [TestMethod]
        public void ShouldCreateRequestWithCorrectApiSignature()
        {
            // Act
            _api2Service.CreateRequest(
                    _request.SourceAddress,
                    _request.DestinationAddress,
                    _request.Carton);

            var requestCreated = _api2Service._api2ServiceRequest;

            // Assert
            Assert.IsNotNull(requestCreated);
            Assert.AreEqual(requestCreated.Consignee, _request.SourceAddress);
            Assert.AreEqual(requestCreated.Consignor, _request.DestinationAddress);
            Assert.IsNotNull(requestCreated.Cartons);
            Assert.AreEqual(requestCreated.Cartons.Length, 3);
            Assert.AreEqual(requestCreated.Cartons[0], _request.Carton.Length);
            Assert.AreEqual(requestCreated.Cartons[1], _request.Carton.Width);
            Assert.AreEqual(requestCreated.Cartons[2], _request.Carton.Height);
        }

        [TestMethod]
        public void ShouldParseApiResponseCorrectly()
        {
            // Act
            var offer = _api2Service.GetOfferResponse("{'amount':1275}");

            // Assert
            Assert.IsNotNull(offer);
            Assert.IsInstanceOfType(offer, typeof(Offer));
            Assert.AreEqual(offer.Quote, 1275);
        }

        [TestMethod]
        public void ShouldReturnNullIfIncorrectApiResponseOrAnyError()
        {
            // Act
            var offer = _api2Service.GetOfferResponse("{'error':'Server Error'}");

            // Assert
            Assert.IsNull(offer);
        }

        [TestMethod]
        public void ShouldCallApiWithCorrectVerb()
        {
            // Act
            var result = _api2Service.PostAsync(
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
            var result = _api2Service.PostAsync(
                    _request.SourceAddress,
                    _request.DestinationAddress,
                    _request.Carton);

            // Assert
            _httpHandlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // expected a POST request
                  && req.Content.Headers.GetValues("Content-Type").FirstOrDefault() == "application/json" // expected "Content-Type" to "application/json"
                  && req.Headers.GetValues("Accept").FirstOrDefault() == "application/json" // expected "Accept" to "application/json"
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
