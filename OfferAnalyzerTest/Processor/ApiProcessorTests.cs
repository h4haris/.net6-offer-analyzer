using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using OfferAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OfferAnalyzer.Processor.Tests
{
    [TestClass]
    public class ApiProcessorTests
    {
        private OfferAnalyzerRequest _request;
        private Mock<HttpMessageHandler> _httpHandlerMock;
        private HttpClient _httpclient;
        private ApiProcessor _apiProcessor;

        [TestInitialize]
        public void Initialize()
        {
            _request = new OfferAnalyzerRequest()
            {
                SourceAddress = "source address",
                DestinationAddress = "dest address",
                Carton = new Dimensions() { Width = 2, Height = 2, Length = 2 }
            };
        }

        [TestMethod]
        public void ShouldReturnNullIfNoApiServiceProvided()
        {
            // Arrange
            _httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpclient = new HttpClient(_httpHandlerMock.Object);
            _apiProcessor = new ApiProcessor(_httpclient);

            // Act
            var result = _apiProcessor.GetAllOffersAsync(
                                    _request.SourceAddress,
                                    _request.DestinationAddress,
                                    _request.Carton,
                                    null
                                ).GetAwaiter().GetResult();

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ShouldCallRelatedServiceIfSpecificApiProvided()
        {
            // Arrange
            var apiServices = new string[] { ApiServiceEnum.api2.ToString() };

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
                   Content = new StringContent("{'amount': 2500}"),
               })
               .Verifiable();

            _httpclient = new HttpClient(_httpHandlerMock.Object);
            _apiProcessor = new ApiProcessor(_httpclient);

            // Act
            var result = _apiProcessor.GetAllOffersAsync(
                                    _request.SourceAddress,
                                    _request.DestinationAddress,
                                    _request.Carton,
                                    apiServices
                                ).GetAwaiter().GetResult();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<Offer>));
            CollectionAssert.AllItemsAreInstancesOfType(result, typeof(Offer));
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Quote, 2500);

            var expectedUri = new Uri("http://localhost:8089/api2/v1/offer");

            _httpHandlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a POST request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        [TestMethod]
        public void ShouldOnlyReturnValidOffers()
        {
            // Arrange
            var apiServices = new string[] { ApiServiceEnum.api1.ToString(), ApiServiceEnum.api2.ToString() };

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
                   Content = new StringContent("{'amount': '3599'}"),
               })
               .Verifiable();

            _httpclient = new HttpClient(_httpHandlerMock.Object);
            _apiProcessor = new ApiProcessor(_httpclient);

            // Act
            var result = _apiProcessor.GetAllOffersAsync(
                                    _request.SourceAddress,
                                    _request.DestinationAddress,
                                    _request.Carton,
                                    apiServices
                                ).GetAwaiter().GetResult();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<Offer>));
            CollectionAssert.AllItemsAreInstancesOfType(result, typeof(Offer));
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Quote, 3599);
        }

        [TestMethod]
        public void ShouldReturnNullIfNoValidOfferFound()
        {
            // Arrange
            var apiServices = new string[] { ApiServiceEnum.api1.ToString(), ApiServiceEnum.api2.ToString() };

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
                   Content = new StringContent("{'error': 'Server Error'}"),
               })
               .Verifiable();

            _httpclient = new HttpClient(_httpHandlerMock.Object);
            _apiProcessor = new ApiProcessor(_httpclient);

            // Act
            var result = _apiProcessor.GetAllOffersAsync(
                                    _request.SourceAddress,
                                    _request.DestinationAddress,
                                    _request.Carton,
                                    apiServices
                                ).GetAwaiter().GetResult();

            // Assert
            Assert.IsNull(result);
        }
    }
}
