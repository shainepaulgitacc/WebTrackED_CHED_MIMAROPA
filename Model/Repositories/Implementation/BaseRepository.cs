using Microsoft.EntityFrameworkCore;
using WebTrackED_CHED_MIMAROPA.Data;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;

namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Implementation
{
    public class BaseRepository<T> : IBaseRepository<T>
        where T : class
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<T> _table;
        public BaseRepository(ApplicationDbContext db)
        {
            _db = db;
            _table = db.Set<T>();
        }
        public async Task Add(T model)
        {
            await _table.AddAsync(model);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(string Id)
        {
            var record = await GetOne(Id);
            if (record != null)
            {
                _table.Remove(record);
                await _db.SaveChangesAsync();   
            }
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _table.ToListAsync();
        }

        public async Task<T> GetOne(string Id)
        {
            if(int.TryParse(Id, out int IdParsed))
            {
                return await _table.FindAsync(IdParsed);
            }
            else
            {
                return await _table.FindAsync(Id);
            }
           
        }

        public async Task Update(T model, string Id)
        {
            var record = await GetOne(Id);
            if(record != null)
            {
                _table.Entry(record).CurrentValues.SetValues(model);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAll(string Ids)
        {
            var ids = Ids.Split(",");
            foreach (var id in ids)
            {
                await Delete(id);
            }
            await _db.SaveChangesAsync();
        }

       
    }
}
