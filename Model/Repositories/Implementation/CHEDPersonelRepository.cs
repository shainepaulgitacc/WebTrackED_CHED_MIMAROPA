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
            var offices = _db.Offices.ToList();
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
                .GroupJoin(offices,
                r => r.Reviewer.OfficeId,
                o => o.Id,
                (r, o) => new
                {
                    Reviewer = r.Reviewer,
                    Account = r.Account,
                    Office = o
                })
                .GroupJoin(designations,
                r => r.Reviewer.DesignationId,
                d => d.Id,
                (r,d) => new
                {
                    Reviewer = r.Reviewer,
                    Account = r.Account,
                    Office = r.Office,
                    Designation = d
                })
                .Select(x => new CHEDPersonelListViewModel
                {
                    CHEDPersonel = x.Reviewer,
                    Account = x.Account,
                    Office = x.Office.FirstOrDefault(),
                    Designation = x.Designation.FirstOrDefault()
                })
                .ToList();
            
        }
    }
}
