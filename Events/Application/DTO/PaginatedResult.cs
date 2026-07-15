using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO
{
    public record PaginatedResult<T>(T[] Items, int CurrentPage, int TotalPages, int CurrentPageItemsCount, int TotalItems);
}
