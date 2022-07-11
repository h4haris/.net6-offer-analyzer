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
    public class Api1ServiceTests
    {
        private OfferAnalyzerRequest _request;
        private Mock<HttpMessageHandler> _httpHandlerMock;
        private HttpClient _httpclient;
        private Api1Service _api1Service;

        [TestInitialize]
        public void Initialize()
        {
            _request = new OfferAnalyzerRequest()
            {
                SourceAddress = "ABC ST",
                DestinationAddress = "123 Apt",
                Carton = new Dimensions() { Width = 4, Height = 4, Length = 6 }
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
                   Content = new StringContent("{'total': 1560}"),
               })
               .Verifiable();

            _httpclient = new HttpClient(_httpHandlerMock.Object);
            _api1Service = new Api1Service(_httpclient);
        }

        [TestMethod]
        public void ShouldInitializeRequestUsingCreateRequestMethod()
        {
            // Initial Value Assert
            Assert.IsNull(_api1Service._api1ServiceRequest);

            // Act
            _api1Service.CreateRequest(
                    _request.SourceAddress,
                    _request.DestinationAddress,
                    _request.Carton);

            // Assert
            Assert.IsNotNull(_api1Service._api1ServiceRequest);
        }

        [TestMethod]
        public void ShouldCreateRequestWithCorrectApiSignature()
        {
            // Act
            _api1Service.CreateRequest(
                    _request.SourceAddress,
                    _request.DestinationAddress,
                    _request.Carton);

            var requestCreated = _api1Service._api1ServiceRequest;

            // Assert
            Assert.IsNotNull(requestCreated);
            Assert.AreEqual(requestCreated.ContactAddress, _request.SourceAddress);
            Assert.AreEqual(requestCreated.WarehouseAddress, _request.DestinationAddress);
            Assert.IsNotNull(requestCreated.PackageDimensions);
            Assert.AreEqual(requestCreated.PackageDimensions.Length, 3);
            Assert.AreEqual(requestCreated.PackageDimensions[0], _request.Carton.Length);
            Assert.AreEqual(requestCreated.PackageDimensions[1], _request.Carton.Width);
            Assert.AreEqual(requestCreated.PackageDimensions[2], _request.Carton.Height);
        }

        [TestMethod]
        public void ShouldParseApiResponseCorrectly()
        {
            // Act
            var offer = _api1Service.GetOfferResponse("{'total':1360}");

            // Assert
            Assert.IsNotNull(offer);
            Assert.IsInstanceOfType(offer, typeof(Offer));
            Assert.AreEqual(offer.Quote, 1360);
        }

        [TestMethod]
        public void ShouldReturnNullIfIncorrectApiResponseOrAnyError()
        {
            // Act
            var offer = _api1Service.GetOfferResponse("{'error':'Server Error'}");

            // Assert
            Assert.IsNull(offer);
        }

        [TestMethod]
        public void ShouldCallApiWithCorrectVerb()
        {
            // Act
            var result = _api1Service.PostAsync(
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
            var result = _api1Service.PostAsync(
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
