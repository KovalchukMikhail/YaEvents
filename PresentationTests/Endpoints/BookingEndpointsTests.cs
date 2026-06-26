using Application.DTO;
using Application.Services.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Moq;
using Presentation.Presentation.Endpoints;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace PresentationTests.Endpoints
{
    public class BookingEndpointsTests
    {
        private readonly Mock<IBookingService> _mockBookingService;
        private readonly Mock<HttpContext> _mockContext;
        private readonly Mock<ClaimsPrincipal> _ClaimsPrincipal;

        public BookingEndpointsTests()
        {
            _mockBookingService = new Mock<IBookingService>();
            _mockContext = new Mock<HttpContext>();
            _ClaimsPrincipal = new Mock<ClaimsPrincipal>();
        }
    
        [Fact]
        public async Task GetBooking_CorrectParams_Code200()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var claim = new Claim(ClaimTypes.NameIdentifier, guid.ToString());
            var bookingInfo = new BookingInfo(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending, DateTime.Now, null, Guid.NewGuid());
            _mockBookingService.Setup(m => m.GetBooking(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(bookingInfo);
            _mockContext.Setup(m => m.User).Returns(_ClaimsPrincipal.Object);
            _ClaimsPrincipal.Setup(u => u.FindFirst(It.IsAny<string>())).Returns(claim);


            //Act
            var result = await BookingEndpoints.GetBooking(bookingInfo.Id, _mockBookingService.Object, _mockContext.Object);
    
            //Assert
            Assert.NotNull(result as Microsoft.AspNetCore.Http.HttpResults.Ok<BookingInfo>);
        }
    
       [Fact]
       public async Task GetBooking_NotExistingBooking_ThrowNotFoundException()
       {
            //Arrange
            var guid = Guid.NewGuid();
            var claim = new Claim(ClaimTypes.NameIdentifier, guid.ToString());
            _mockBookingService.Setup(m => m.GetBooking(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((BookingInfo?) null);
            _mockContext.Setup(m => m.User).Returns(_ClaimsPrincipal.Object);
            _ClaimsPrincipal.Setup(u => u.FindFirst(It.IsAny<string>())).Returns(claim);

            //Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await BookingEndpoints.GetBooking(Guid.NewGuid(), _mockBookingService.Object, _mockContext.Object));
       }
    }
}
