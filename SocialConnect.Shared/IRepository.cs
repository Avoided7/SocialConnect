using System.Linq.Expressions;

namespace SocialConnect.Shared
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAsync();
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> expression);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);

        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
    }
}
