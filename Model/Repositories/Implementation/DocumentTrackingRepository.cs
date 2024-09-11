using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebTrackED_CHED_MIMAROPA.Data;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Implementation
{
    public class DocumentTrackingRepository : BaseRepository<DocumentTracking>, IDocumentTrackingRepository
    {
        private readonly ApplicationDbContext _db;
        public DocumentTrackingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<IEnumerable<DocumentTrackingViewModel>> DocumentTrackings()
        {
            var documentTrackings = _db.DocumentTrackings.ToList();
            var documentAttachments = _db.DocumentAttachments.ToList();
            var reviewers = _db.CHEDPersonels.ToList();
            var reviewerAccount = _db.Set<AppIdentityUser>().ToList();
            var designations = _db.Designations.ToList();
            var offices = _db.Offices.ToList();

            var result = reviewers
                .GroupJoin(designations,
                    personel => personel.DesignationId,
                    designation => designation.Id,
                    (personel, designationGroup) => new
                    {
                        CHEDPersonel = personel,
                        Designation = designationGroup.FirstOrDefault() // Take the first (or default) designation
                    })
                .GroupJoin(offices,
                    personelWithDesignation => personelWithDesignation.CHEDPersonel.OfficeId,
                    office => office.Id,
                    (personelWithDesignation, officeGroup) => new 
                    {
                        CHEDPersonel = personelWithDesignation.CHEDPersonel,
                        Designation = personelWithDesignation.Designation,
                        Office = officeGroup.FirstOrDefault() // Take the first (or default) office
                    })
                .Join(documentTrackings,
                    result => result.CHEDPersonel.IdentityUserId,
                    tracking => tracking.ReviewerId,
                    (result, tracking) => new
                    {
                        DocumentTracking = tracking,
                        CHEDPersonel = result.CHEDPersonel,
                        Designation = result.Designation,
                        Office = result.Office
                    })
                .Join(documentAttachments,
                 result => result.DocumentTracking.DocumentAttachmentId,
                 docAttach => docAttach.Id,
                 (result,docAttach) => new
                 {
                     DocumentTracking = result.DocumentTracking,
                     DocumentAttachment = docAttach,
                    
                     CHEDPersonel = result.CHEDPersonel,
                     Designation = result.Designation,
                     Office = result.Office
                 })
                .Join(reviewerAccount,
                result => result.CHEDPersonel.IdentityUserId,
                revAcc => revAcc.Id,
                (result, revAcc) => new
                {
                    DocumentTracking = result.DocumentTracking,
                    DocumentAttachment = result.DocumentAttachment,
                    CHEDPersonel = result.CHEDPersonel,
                    Designation = result.Designation,
                    Office = result.Office,
                    Account = revAcc
                    
                })
                .Select(result => new DocumentTrackingViewModel
                {
                    DocumentTracking = result.DocumentTracking,
                    DocumentAttachment = result.DocumentAttachment,
                    CHEDPersonel = result.CHEDPersonel,
                    Designation = result.Designation,
                    Office = result.Office,
                    Account = result.Account
                })
                .ToList();
            return result;
        }
    }
}
