using System.Linq.Expressions;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Repository
{
    class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> _dbSet; 

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = _db.Set<T>();
        }

        public async Task CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public async Task<T> GetOneAsync(Expression<Func<T, bool>> filter, bool tracked)
        {
            if (filter != null)
            {
                IQueryable<T> villaQueryable = _dbSet;

                if (!tracked)
                {
                    villaQueryable = villaQueryable.AsNoTracking();
                }

                return await villaQueryable.Where(filter).FirstOrDefaultAsync();
            }

            throw new ArgumentException();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter, bool tracked)
        {
            if (filter != null)
            {
                // If a filter is passed, then we convert the Villas from the DB into a Queryable type so the filter can be applied. Else we return everything.

                IQueryable<T> villaQueryable = _dbSet;

                if (tracked == true)
                {
                    return await villaQueryable.Where(filter).ToListAsync();
                }
                else
                {
                    return await villaQueryable.Where(filter).AsNoTracking().ToListAsync();
                }
            }

            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}