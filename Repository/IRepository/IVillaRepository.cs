using MagicVilla_VillaAPI.Models;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IVillaRepository
    {
        Task<List<Villa>> GetAllAsync(Expression<Func<Villa, bool>> filter = null, bool tracked = false);

        Task<Villa> GetOneAsync(Expression<Func<Villa, bool>> filter = null, bool tracked = false);

        Task CreateAsync(Villa entity);
        
        Task UpdateAsync(Villa entity);

        Task RemoveAsync(Villa entity);

        Task SaveAsync();
    }
}