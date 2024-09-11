namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts
{
    public interface IBaseRepository<T>
    {
        public Task<IEnumerable<T>> GetAll();
        public Task<T> GetOne(string Id);
        public Task Add(T model);
        public Task Update(T model, string Id);
        public Task Delete(string Id);
        public Task DeleteAll(string Ids);
    }
}
