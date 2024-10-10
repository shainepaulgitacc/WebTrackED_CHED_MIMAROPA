using WebTrackED_CHED_MIMAROPA.Data;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Implementation
{
    public class CHEDPersonelRepository : BaseRepository<CHEDPersonel>, ICHEDPersonelRepository
    {
        private readonly ApplicationDbContext _db;
        public CHEDPersonelRepository(
            ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<IEnumerable<CHEDPersonelListViewModel>> CHEDPersonelRecords()
        {
            var personels = _db.CHEDPersonels.ToList();
            var designations = _db.Designations.ToList();
            var accounts = _db.Set<AppIdentityUser>();


            return personels
                .Join(accounts,
                p => p.IdentityUserId,
                a => a.Id,
                (p, a) => new
                {
                    Reviewer = p,
                    Account = a
                })
                .GroupJoin(designations,
                r => r.Reviewer.DesignationId,
                d => d.Id,
                (r,d) => new
                {
                    Reviewer = r.Reviewer,
                    Account = r.Account,
                  
                    Designation = d
                })
                .Select(x => new CHEDPersonelListViewModel
                {
                    CHEDPersonel = x.Reviewer,
                    Account = x.Account,
                    Designation = x.Designation.FirstOrDefault()
                })
                .ToList();
            
        }
    }
}
