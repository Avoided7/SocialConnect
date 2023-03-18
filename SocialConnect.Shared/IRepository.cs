using System.Linq.Expressions;

namespace SocialConnect.Shared
{
    public interface IRepository<T> where T : IEntity
    {
        Task<IEnumerable<T>> GetAsync();
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> expression);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);

        Task<T?> CreateAsync(T entity);
        Task<T?> UpdateAsync(string id, T entity);
        Task<bool> DeleteAsync(string id);
    }
}
