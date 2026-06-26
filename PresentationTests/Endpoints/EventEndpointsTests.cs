using Application.DTO;
using Application.Services.Interfaces;
using Castle.Core.Configuration;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Presentation.Presentation.Endpoints;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace PresentationTests.Endpoints
{
    public class EventEndpointsTests
    {
        private readonly Mock<IEventService> _mockEventService;
        private readonly Mock<IBookingService> _mockBookingService;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ClaimsPrincipal> _claimsPrincipal;

        public EventEndpointsTests()
        {
            _mockEventService = new Mock<IEventService>();
            _mockBookingService = new Mock<IBookingService>();
            _mockHttpContext = new Mock<HttpContext>();
            _claimsPrincipal = new Mock<ClaimsPrincipal>();
        }
    
        [Fact]
        public async Task PostBooking_CorrectParams_Code202()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var claim = new Claim(ClaimTypes.NameIdentifier, guid.ToString());
            var requiredEvent = new EventInfo(Guid.NewGuid(), "Title", "Description", DateTime.Parse("2010.01.01"), DateTime.Parse("2011.01.01"), EventStatus.Existing, 100, 100);
            var requiredUser = new UserInfo(Guid.NewGuid(), "Test", UserRole.User);
            var newBookingInfo = new BookingInfo(Guid.NewGuid(), requiredEvent.Id, BookingStatus.Pending, DateTime.Now, null, requiredUser.Id);
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["LimitOfActiveBookings"] = "10" }).Build();
            _mockBookingService.Setup(m => m.CreateBookingAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(newBookingInfo);
            _mockHttpContext.Setup(m => m.Request.Scheme).Returns("https");
            _mockHttpContext.Setup(m => m.Request.Host).Returns(new HostString("localhost:7067"));
            _mockHttpContext.Setup(m => m.User).Returns(_claimsPrincipal.Object);
            _claimsPrincipal.Setup(u => u.FindFirst(It.IsAny<string>())).Returns(claim);

            //Act
            var result = await EventEndpoints.PostBooking(Guid.NewGuid(), _mockEventService.Object, _mockBookingService.Object, _mockHttpContext.Object, configuration);
    
            //Assert
            Assert.NotNull(result as Microsoft.AspNetCore.Http.HttpResults.Accepted<BookingInfo>);
    
        }
    }
}
