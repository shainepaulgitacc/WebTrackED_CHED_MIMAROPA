using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using NuGet.Protocol.Plugins;
using System.Linq;
using System.Linq.Expressions;
using WebTrackED_CHED_MIMAROPA.Data;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Implementation
{
    public class DocumentAttachmentRepository : BaseRepository<DocumentAttachment>, IDocumentAttachmentRepository
    {
        private readonly ApplicationDbContext _db;
        public DocumentAttachmentRepository(ApplicationDbContext db, IWebHostEnvironment env) : base(db)
        {
            _db = db;
        }
        public async Task<IEnumerable<DocumentAttachmentViewModel>> DocumentAttachments()
        {
            var docTrackings = _db.DocumentTrackings.ToList();
            var documents =  _db.DocumentAttachments.ToList();
            var reviewers =  _db.CHEDPersonels.ToList();
            var categories = _db.Categories.ToList();
            var subcategories = _db.SubCategories.ToList();
            var senders = _db.Senders.ToList();

            var reviewerAccounts = _db.Set<AppIdentityUser>().ToList();
            var senderAccounts = _db.Set<AppIdentityUser>().ToList();

           var records =  docTrackings
                .Join(documents ,
                docTrack => docTrack.DocumentAttachmentId,
                docs => docs.Id,
                (docTrack,docs) => new
                {
                    DocTrack = docTrack,
                    Document = docs,
                })
               
                .Join(reviewers,
                 result => result.DocTrack.ReviewerId,
                 rev => rev.IdentityUserId,
                 (doc, rev) => new
                 {
                     DocTrack = doc.DocTrack,
                     Document = doc.Document,
                     Reviewer = rev
                 })
                .Join(reviewerAccounts,
                 reviewer => reviewer.Reviewer.IdentityUserId,
                revAcc => revAcc.Id,
                (result, revAcc) => new
                {
                    DocTrack = result.DocTrack,
                    Document = result.Document,
                    Reviewer = result.Reviewer,
                    ReviewerAcc = revAcc
                })
                 .Join(senders,
                 result => result.Document.SenderId,
                 sender => sender.IdentityUserId,
                 (result, sender) => new
                 {
                     DocTrack = result.DocTrack,
                     Document = result.Document,
                     Reviewer = result.Reviewer,
                     ReviewerAcc = result.ReviewerAcc,
                     Sender = sender
                 })
                 .Join(senderAccounts,
                 result => result.Sender.IdentityUserId,
                 senderAcc => senderAcc.Id,
                 (result, senderAcc) => new
                 {
                     DocTrack = result.DocTrack,
                     Document = result.Document,
                     Reviewer = result.Reviewer,
                     ReviewerAcc = result.ReviewerAcc,
                     Sender = result.Sender,
                     SenderAcc = senderAcc
                 })
                .Join(categories,
                result => result.Document.CategoryId,
                categ => categ.Id,
                (result, categ) => new
                {
                    DocTrack = result.DocTrack,
                    Document = result.Document,
                    Reviewer = result.Reviewer,
                    ReviewerAcc = result.ReviewerAcc,
                    Sender = result.Sender,
                    SenderAcc = result.SenderAcc,
                    Category = categ
                })
                .Join(subcategories,
                results => results.Document.SubCategoryId,
                subcategs => subcategs.Id,
                (result, subcateg) => new DocumentAttachmentViewModel
                {
                    DocumentTracking = result.DocTrack,
                    DocumentAttachment = result.Document,
                    Reviewer = result.Reviewer,
                    ReviewerAccount = result.ReviewerAcc,
                    SenderAccount = result.SenderAcc,
                    Category = result.Category,
                    SubCategory = subcateg
                }).ToList();



            var sample = docTrackings
                .Join(documents,
                docTrack => docTrack.DocumentAttachmentId,
                docs => docs.Id,
                (docTrack, docs) => new
                {
                    DocTrack = docTrack,
                    Document = docs,
                })

                .Join(reviewers,
                 result => result.DocTrack.ReviewerId,
                 rev => rev.IdentityUserId,
                 (doc, rev) => new
                 {
                     DocTrack = doc.DocTrack,
                     Document = doc.Document,
                     Reviewer = rev
                 })
                .Join(reviewerAccounts,
                 reviewer => reviewer.Reviewer.IdentityUserId,
                revAcc => revAcc.Id,
                (result, revAcc) => new
                {
                    DocTrack = result.DocTrack,
                    Document = result.Document,
                    Reviewer = result.Reviewer,
                    ReviewerAcc = revAcc
                })
                .Join(senderAccounts,
                docTrack => docTrack.Document.SenderId,
                senderId => senderId.Id,
                (doc,sender) => new
                {
					DocTrack = doc.DocTrack,
					Document = doc.Document,
					Reviewer = doc.Reviewer,
					ReviewerAcc = doc.ReviewerAcc,
					SenderAcc = sender
				}).
                Join(categories,
				result => result.Document.CategoryId,
				categ => categ.Id,
				(result, categ) => new
				{
					DocTrack = result.DocTrack,
					Document = result.Document,
					Reviewer = result.Reviewer,
					ReviewerAcc = result.ReviewerAcc,
					SenderAcc = result.SenderAcc,
					Category = categ
				})
				.Join(subcategories,
				results => results.Document.SubCategoryId,
				subcategs => subcategs.Id,
				(result, subcateg) => new DocumentAttachmentViewModel
				{
					DocumentTracking = result.DocTrack,
					DocumentAttachment = result.Document,
					Reviewer = result.Reviewer,
					ReviewerAccount = result.ReviewerAcc,
					SenderAccount = result.SenderAcc,
					Category = result.Category,
					SubCategory = subcateg
				}).ToList();


            var finRec = sample
                .OrderByDescending(x => x.DocumentTracking.Id)
                .GroupBy(x => x.DocumentAttachment.Id)
                .Select(result => new DocumentAttachmentViewModel
                {
                    DocumentAttachment = result.First().DocumentAttachment,
                    DocumentTracking = result.First().DocumentTracking,
                    Category = result.First().Category,
                    Reviewer = result.First().Reviewer,
                    ReviewerAccount = result.First().ReviewerAccount,
                    SenderAccount = result.First().SenderAccount,
                    SubCategory = result.First().SubCategory,
                    DocumentTrackings = docTrackings.Where(x => x.DocumentAttachmentId == result.Key).ToList()

                })
                .ToList();
            return finRec;
        }

        public async Task<ReportsRecords> GetRecordsPiginated(
           int? subCategory,
           Prioritization? prioritization,
           Status? docsStatus
            )
        { 
            var rec = await DocumentAttachments();
            var fRec = new List<DocumentAttachmentViewModel>();
            if(subCategory != null && prioritization!= null && docsStatus != null)
            {
                fRec = rec
                     .Where(x => x.SubCategory.Id == subCategory && x.DocumentAttachment.Prioritization == prioritization && x.DocumentAttachment.Status == docsStatus)
                     .ToList();
            }
            else
            {
                fRec = rec.ToList();
            }
               /* 
                .Where(x => 
               subCategory != null ? x.SubCategory.Id == subCategory : true &&
               prioritization != null ? x.DocumentAttachment.Prioritization == prioritization : true &&
               docsStatus != null ? x.DocumentAttachment.Status == docsStatus : true)
                .ToList();*/
            var finRecords =  fRec
                .ToList();
            return new ReportsRecords
            {
                Records = finRecords,
                Status = docsStatus,
                Prioritization = prioritization,
                SubCategory = subCategory,
                
            };

        }



    }
}
