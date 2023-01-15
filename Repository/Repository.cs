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

        // Info - If properties of a given model are requested. For example the Villa that's related (inside) VillaNumber, we would need a code like the following one to include such property in the dataset.
        //_db.VillaNumbers.Include(v => v.Villa).ToList();
        //The above statement is what gets executed in the GetOne and GetAll methods for every property requested by the "includeProperties" parameter.


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

        public async Task<T> GetOneAsync(Expression<Func<T, bool>> filter, bool tracked, string? includeProperties)
        {
            if (filter != null)
            {
                // If a filter is passed, then we convert the Villas from the DB into a Queryable type so the filter can be applied.
                IQueryable<T> query = _dbSet;

                if (!tracked)
                {
                    query = query.AsNoTracking();
                }

                if (includeProperties != null)
                {
                    foreach(var property in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)) // If multiple properties are requested, we will process each of them and include them to the main result set.
                    {
                        query = query.Include(property);
                    }
                }

                return await query.Where(filter).FirstOrDefaultAsync();
            }

            throw new ArgumentException();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter, string? includeProperties)
        {
            IQueryable<T> query = _dbSet;

            query = query.AsNoTracking();

            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)) // If multiple properties are requested, we will process each of them and include them to the main result set.
                {
                    query = query.Include(property);
                }
            }

            if (filter != null)
            {
                return await query.Where(filter).ToListAsync();
            }

            return await query.ToListAsync();
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