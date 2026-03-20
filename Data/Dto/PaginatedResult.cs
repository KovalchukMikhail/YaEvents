namespace YaEvents.Data.Dto
{
    public record PaginatedResult<T>(T[] Items, int CurrentPage, int TotalPages, int CurrentPageItemsCount, int TotalItems);
}
