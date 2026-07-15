using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO
{
    public record UserInfo(Guid Id, string? Login, UserRole Role);
}
