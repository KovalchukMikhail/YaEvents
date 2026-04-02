namespace YaEvents.Infrastructure.Repositories.Interfaces
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAll(CancellationToken token = default);
        Task<T?> Get(Guid id, CancellationToken token = default);
        Task<T> Add(T entity, CancellationToken token = default);
        Task Change(T entity, CancellationToken token = default);
        Task<bool> Delete(Guid id, CancellationToken token = default);

    }
}
