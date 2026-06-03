using Application.DTO;
using Application.Services.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Moq;
using Presentation.Presentation.Endpoints;
using System;
using System.Collections.Generic;
using System.Text;

namespace PresentationTests.Endpoints
{
    public class BookingEndpointsTests
    {
        private readonly Mock<IBookingService> _mockBookingService;

        public BookingEndpointsTests()
        {
            _mockBookingService = new Mock<IBookingService>();
        }

        [Fact]
        public async Task GetBooking_CorrectParams_Code200()
        {
            //Arrange
            var bookingInfo = new BookingInfo(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Pending, DateTime.Now, null);
            _mockBookingService.Setup(m => m.GetBookingByIdAsync(It.IsAny<Guid>())).ReturnsAsync(bookingInfo);

            //Act
            var result = await BookingEndpoints.GetBooking(bookingInfo.Id, _mockBookingService.Object);

            //Assert
            Assert.NotNull(result as Microsoft.AspNetCore.Http.HttpResults.Ok<BookingInfo>);
        }

       [Fact]
       public async Task GetBooking_NotExistingBooking_ThrowNotFoundException()
       {
           //Arrange
           _mockBookingService.Setup(m => m.GetBookingByIdAsync(It.IsAny<Guid>())).ReturnsAsync((BookingInfo?) null);

           //Act & Assert
           await Assert.ThrowsAsync<NotFoundException>(async () => await BookingEndpoints.GetBooking(Guid.NewGuid(), _mockBookingService.Object));
       }
    }
}
