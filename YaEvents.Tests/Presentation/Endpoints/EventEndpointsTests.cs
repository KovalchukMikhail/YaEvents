using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using YaEvents.Application.Services.Interfaces;
using YaEvents.Data.Dto;
using YaEvents.Infrastructure.Enums;
using YaEvents.Infrastructure.Exceptions;
using YaEvents.Presentation.Endpoints;

namespace YaEvents.Tests.Presentation.Endpoints
{
    public class EventEndpointsTests
    {
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<IBookingService> _mockBookingService;
        private readonly Mock<HttpContext> _mockHttpContext;

        public EventEndpointsTests()
        {
            _mockEventService = new Mock<IEventService>();
            _mockBookingService = new Mock<IBookingService>();
            _mockHttpContext = new Mock<HttpContext>();
        }

        [Fact]
        public async Task PostBooking_CorrectParams_Code202()
        {
            //Arrange
            var requiredEvent = new EventDto(Guid.NewGuid(), "Title", "Description", DateTime.Parse("2010.01.01"), DateTime.Parse("2011.01.01"), Infrastructure.Enums.EventStatus.Existing);
            var newBookingInfo = new BookingInfo(Guid.NewGuid(), requiredEvent.Id, BookingStatus.Pending, DateTime.Now, null);
            _mockEventService.Setup(m => m.GetEvent(It.IsAny<Guid>())).ReturnsAsync(requiredEvent);
            _mockBookingService.Setup(m => m.CreateBookingAsync(It.IsAny<Guid>())).ReturnsAsync(newBookingInfo);
            _mockHttpContext.Setup(m => m.Request.Scheme).Returns("https");
            _mockHttpContext.Setup(m => m.Request.Host).Returns(new HostString("localhost:7067"));

            //Act
            var result = await EventEndpoints.PostBooking(Guid.NewGuid(), _mockEventService.Object, _mockBookingService.Object, _mockHttpContext.Object);

            //Assert
            Assert.NotNull(result as Microsoft.AspNetCore.Http.HttpResults.Accepted<BookingInfo>);

        }
    }
}
