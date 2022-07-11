using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OfferAnalyzer.DataInterface;
using OfferAnalyzer.Models;
using System;
using System.Collections.Generic;

namespace OfferAnalyzer.Processor.Tests
{
    [TestClass]
    public class OfferDataAnalyzerTests
    {
        private OfferAnalyzerRequest _request;
        private Mock<IApiProcessor> _apiProcessorMock;
        private OfferDataAnalyzer _offerAnalyzer;

        [TestInitialize]
        public void Initialize()
        {
            _request = new OfferAnalyzerRequest()
            {
                SourceAddress = "source address",
                DestinationAddress = "dest address",
                Carton = new Dimensions() { Width = 2, Height = 2, Length = 2 }
            };

            List<Offer> availableOffers = new List<Offer> {
                new Offer { Quote= 5100, Company = ApiServiceEnum.api1.ToString() },
                new Offer { Quote= 4950, Company = ApiServiceEnum.api2.ToString() },
                new Offer { Quote= 4890, Company = ApiServiceEnum.api3.ToString() }
            };

            var apiServices = new string[] { ApiServiceEnum.api1.ToString(), ApiServiceEnum.api2.ToString(), ApiServiceEnum.api3.ToString() };

            _apiProcessorMock = new Mock<IApiProcessor>();
            _apiProcessorMock.Setup(x => x.GetAllOffersAsync(
                    _request.SourceAddress,
                    _request.DestinationAddress,
                    _request.Carton,
                    apiServices
                    ))
              .ReturnsAsync(availableOffers);

            _offerAnalyzer = new OfferDataAnalyzer(_apiProcessorMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfRequestIsNull()
        {
            // Act
            _offerAnalyzer.ProcessRequest(null).GetAwaiter().GetResult();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfSourceAddressIsNull()
        {
            // Arrange
            _request = new OfferAnalyzerRequest()
            {
                SourceAddress = "",
                DestinationAddress = "dest address",
                Carton = new Dimensions() { Width = 2, Height = 2, Length = 2 }
            };

            // Act
            _offerAnalyzer.ProcessRequest(_request).GetAwaiter().GetResult();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfDestinationAddressIsNull()
        {
            // Arrange
            _request = new OfferAnalyzerRequest()
            {
                SourceAddress = "source",
                DestinationAddress = null,
                Carton = new Dimensions() { Width = 2, Height = 2, Length = 2 }
            };

            // Act
            _offerAnalyzer.ProcessRequest(_request).GetAwaiter().GetResult();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfCartonIsNull()
        {
            // Arrange
            _request = new OfferAnalyzerRequest()
            {
                SourceAddress = "source",
                DestinationAddress = "destination",
                Carton = null
            };

            // Act
            _offerAnalyzer.ProcessRequest(_request).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ShouldReturnBestOfferDeal()
        {
            // Act
            var result = _offerAnalyzer.ProcessRequest(_request).Result;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Quote, 4890);
            Assert.AreEqual(result.Company, ApiServiceEnum.api3.ToString());
        }
    }
}
