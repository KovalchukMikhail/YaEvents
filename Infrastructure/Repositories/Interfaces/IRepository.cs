namespace YaEvents.Infrastructure.Repositories.Interfaces
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T? Get(int id);
        T Add(T entity);
        void Change(T entity);
        bool Delete(int id);

    }
}
