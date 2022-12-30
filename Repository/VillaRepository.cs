using System.Linq.Expressions;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Repository
{
    class VillaRepository : IVillaRepository
    {
        private readonly ApplicationDbContext _db;

        public VillaRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(Villa entity)
        {
            await _db.Villas.AddAsync(entity);
            await SaveAsync();
        }

        public async Task UpdateAsync(Villa entity)
        {
            _db.Villas.Update(entity);
            await SaveAsync();
        }

        public async Task<Villa> GetOneAsync(Expression<Func<Villa, bool>> filter, bool tracked)
        {
            if (filter != null)
            {
                IQueryable<Villa> villaQueryable = _db.Villas;

                if (!tracked)
                {
                    villaQueryable = villaQueryable.AsNoTracking();
                }
                
                return await villaQueryable.Where(filter).FirstOrDefaultAsync();
            }

            throw new ArgumentException();
        }

        public async Task<List<Villa>> GetAllAsync(Expression<Func<Villa, bool>> filter, bool tracked)
        {
            if (filter != null)
            {
                // If a filter is passed, then we convert the Villas from the DB into a Queryable type so the filter can be applied. Else we return everything.

                IQueryable<Villa> villaQueryable = _db.Villas;

                if (tracked == true)
                {
                    return await villaQueryable.Where(filter).ToListAsync();
                }
                else
                {
                    return await villaQueryable.Where(filter).AsNoTracking().ToListAsync();
                }
            }

            return await _db.Villas.AsNoTracking().ToListAsync();
        }

        public async Task RemoveAsync(Villa entity)
        {
            _db.Villas.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}