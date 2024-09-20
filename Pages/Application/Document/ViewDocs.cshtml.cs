using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using System.Net.WebSockets;
using WebTrackED_CHED_MIMAROPA.Hubs;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Implementation;
using WebTrackED_CHED_MIMAROPA.Model.Service;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document
{
	[Authorize]
	[ValidateAntiForgeryToken]
	public class ViewDocsModel : PageModel
	{
		private readonly IDocumentAttachmentRepository _docRepo;
		private readonly IBaseRepository<DocumentTracking> _docTrackRepo;
        private readonly IBaseRepository<AppIdentityUser> _revAccRepo;
		private readonly ISenderRepository _senderRepo;
		private readonly UserManager<AppIdentityUser> _userManager;
		private readonly ICHEDPersonelRepository _reviewerRepo;
		private readonly FileUploader _fileUploader;
		private readonly IHubContext<NotificationHub, INotificationHub> _notifHub;
		private readonly IBaseRepository<Notification> _notificationRepo;
		private readonly IBaseRepository<Settings> _settingsRepo;
		private readonly IMapper _mapper;
		public ViewDocsModel(
			IDocumentAttachmentRepository docRepo,
			IBaseRepository<DocumentTracking> docTrackRepo,
			ISenderRepository senderRepo,
            IBaseRepository<AppIdentityUser> revAccRepo,

            UserManager<AppIdentityUser> userManager,
		   ICHEDPersonelRepository reviewerRepo,
			FileUploader fileUploader,
			IHubContext<NotificationHub, INotificationHub> notifHub,
			IBaseRepository<Notification> notificationRepo,
            IBaseRepository<Settings> settingsRepo,


            IMapper mapper)
		{
			_docRepo = docRepo;
			_docTrackRepo = docTrackRepo;
            _revAccRepo = revAccRepo;
			_senderRepo = senderRepo;
			_userManager = userManager;
			_reviewerRepo = reviewerRepo;
			_fileUploader = fileUploader;
			_notifHub = notifHub;
			_notificationRepo = notificationRepo;
			_mapper = mapper;
			_settingsRepo = settingsRepo;
		}
		public string PreviousPage { get; set; }

		[BindProperty]
		public IFormFile? NewDocuments { get; set; }

		[BindProperty]
		public Prioritization? Prioritization { get; set; }

		public DocumentAttachmentViewModel DocumentAttachment { get; set; }
		public Designation Designation { get; set; }
		public string Logo { get; set; }


		public bool HasCurrentlyReviewing { get; set; }
	public string AccountId { get; set; }

		public async Task<IActionResult> OnGetAsync(string prevPage, int pId)
		{
			PreviousPage = prevPage;
			var docsAttachments = await _docRepo.DocumentAttachments();
			var docAttachment = docsAttachments.OrderByDescending(x => x.DocumentTracking.Id).FirstOrDefault(x => x.DocumentAttachment.Id == pId);
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var getRoles = await _userManager.GetRolesAsync(user);
            if (docAttachment == null)
                return BadRequest($"Unknown document");
            else if (docAttachment.DocumentAttachment.Status == Status.PreparingRelease && !getRoles.Any(x => x == "Admin"))
                return BadRequest("You can't access this page it is only available to the record");
			DocumentAttachment = docAttachment;
			AccountId = user.Id;
            var settings = await _settingsRepo.GetAll();
            Logo = settings.OrderByDescending(x => x.Id).First().LogoFileName;

            var docsTrackings = await _docTrackRepo.GetAll();
            var docAttachments = await _docRepo.GetAll();
            var reviewers = await _revAccRepo.GetAll();

            HasCurrentlyReviewing = docAttachments.Where(x => x.Id == pId)
             .Join(docsTrackings,
             da => da.Id,
             dt => dt.DocumentAttachmentId,
             (da, dt) => new
             {
                 Document = da,
                 DocumentTracking = dt
             })
             .Join(reviewers,
             dt => dt.DocumentTracking.ReviewerId,
             r => r.Id,
             (dt, r) => new
             {
                 Document = dt.Document,
                 DocumentTracking = dt.DocumentTracking,
                 Reviewer = r
             })
             .GroupBy(r => r.Reviewer.Id)
             .Select(result => new
             {
                 ReviewerStatus = result.OrderByDescending(x => x.DocumentTracking.Id).First().DocumentTracking.ReviewerStatus
             })
             .Any(x => x.ReviewerStatus != ReviewerStatus.Reviewed || x.ReviewerStatus == ReviewerStatus.Passed);



            


            return Page();



		}
		public async Task<IActionResult> OnGetDownloadFile(string filename)
		{
			try
			{
				byte[] fileBytes = await _fileUploader.DownloadFile(filename);
				return File(fileBytes, "application/octet-stream", filename);
			}
			catch (FileNotFoundException ex)
			{
				return NotFound();
			}
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}
		#region --ViewDocument--
		/*public async Task<IActionResult>OnGetViewDocument(string filename)
        {
            try
            {
                var result = await _fileUploader.ViewFile(filename);
                return PhysicalFile(result.Item1,result.Item2);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }*/
		#endregion

		public async Task<IActionResult> OnPostChangeDocument(string prevPage, int pId)
		{
			var docAttachment = await _docRepo.GetOne(pId.ToString());

			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			docAttachment.FileName = await _fileUploader.Uploadfile(NewDocuments, "Documents");
			docAttachment.UpdatedAt = DateTime.Now;
			await _docRepo.Update(docAttachment, docAttachment.Id.ToString());
			TempData["validation-message"] = "Successfully replace document";
			return RedirectToPage("ViewDocs", new { prevPage, pId });
		}
		public async Task<IActionResult> OnPostSetPrioritization(string prevPage, int pId)
		{
			var docAttachment = await _docRepo.GetOne(pId.ToString());

			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			docAttachment.Prioritization = Prioritization;
			docAttachment.UpdatedAt = DateTime.Now;
			await _docRepo.Update(docAttachment, docAttachment.Id.ToString());
			TempData["validation-message"] = "Successfully set prioritization";
			return RedirectToPage("ViewDocs", new { prevPage, pId });
		}
		public async Task<IActionResult> OnGetMarkAsOnProcessOrDisapproved(string prevPage, string pId, Status status)
		{
			var docAttachment = await _docRepo.GetOne(pId);
			var account = await _userManager.FindByNameAsync(User.Identity?.Name);
			var reviewers = await _reviewerRepo.CHEDPersonelRecords();
			var reviewer = reviewers.FirstOrDefault(x => x.Account.Id == account?.Id);
			await _docTrackRepo.Add(new DocumentTracking
			{
				AddedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				ReviewerId = reviewer.CHEDPersonel.IdentityUserId,
				DocumentAttachmentId = docAttachment.Id,
				ReviewerStatus = status == Status.OnProcess ? ReviewerStatus.OnReview : ReviewerStatus.Disapproved
			});
			var notification = new Notification
			{
				Title = status == Status.Disapproved ? "Document Disapproval" : "Document Review and Process",
				Recepient = docAttachment.SenderId,
				IsViewed = false,
				Description = status == Status.Disapproved ? "Your document has already been disapproved due to errors found by the reviewers." : $"Your document is currently reviewing by {reviewer.Account.FirstName} {reviewer.Account.MiddleName} {reviewer.Account.LastName} {reviewer.Account.Suffixes}, from ({reviewer.Office.OfficeName}).",
				NotificationType = NotificationType.Document,
				RedirectLink = status == Status.Disapproved ? "/Application/Document/Ended/Index" : docAttachment.DocumentType != DocumentType.WalkIn ?"/Application/Document/Onprocess/Index": "/Application/Document/Outgoing/Index",
				AddedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
			};
            var settings = await _settingsRepo.GetAll();
            if (settings.OrderByDescending(x => x.Id).First().DocumentNotif)
            {
                await _notificationRepo.Add(notification);
                _notifHub.Clients.User(docAttachment.SenderId).ReceiveNotification(
                    notification.Title,
                    notification.Description.Length > 30 ? $"{notification.Description.Substring(0, 30)}..." : notification.Description,
                    notification.NotificationType.ToString(),
                    notification.AddedAt.ToString("MMMM dd, yyyy"),
                    notification.RedirectLink
                );
            }
			docAttachment.Status = status;
			TempData["validation-message"] = status == Status.OnProcess ? "Successfully perform the action." : "Successfully disapproved document";
			await _docRepo.Update(docAttachment, docAttachment.Id.ToString());
			if (status == Status.OnProcess)
				return RedirectToPage("ViewDocs", new { prevPage, pId });
			else
				return RedirectToPage("/Application/Document/Ended/Index");
		}


        public async Task<IActionResult> OnGetReviewed(string prevPage, string pId)
        {
            var docAttachment = await _docRepo.GetOne(pId);
            var account = await _userManager.FindByNameAsync(User.Identity?.Name);
            var reviewers = await _reviewerRepo.CHEDPersonelRecords();
            var reviewer = reviewers.FirstOrDefault(x => x.Account.Id == account?.Id);
            await _docTrackRepo.Add(new DocumentTracking
            {
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ReviewerId = reviewer.CHEDPersonel.IdentityUserId,
                DocumentAttachmentId = docAttachment.Id,
                ReviewerStatus = ReviewerStatus.Reviewed
            });
            var notification = new Notification
            {
                Title = "Document Reviewed",
                Recepient = docAttachment.SenderId,
                IsViewed = false,
                Description =$"Your document has been reviewed by {reviewer.Account.FirstName} {reviewer.Account.MiddleName} {reviewer.Account.LastName} {reviewer.Account.Suffixes}, from ({reviewer.Office.OfficeName}).",
                NotificationType = NotificationType.Document,
                RedirectLink = docAttachment.DocumentType != DocumentType.WalkIn ? "/Application/Document/Onprocess/Index" : "/Application/Document/Outgoing/Index",
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            var settings = await _settingsRepo.GetAll();
            if (settings.OrderByDescending(x => x.Id).First().DocumentNotif)
            {
                await _notificationRepo.Add(notification);
                _notifHub.Clients.User(docAttachment.SenderId).ReceiveNotification(
                    notification.Title,
                    notification.Description.Length > 30 ? $"{notification.Description.Substring(0, 30)}..." : notification.Description,
                    notification.NotificationType.ToString(),
                    notification.AddedAt.ToString("MMMM dd, yyyy"),
                    notification.RedirectLink
                );
            }
            TempData["validation-message"] = "Successfully perform the action.";
			await _docRepo.Update(docAttachment, docAttachment.Id.ToString());
            return RedirectToPage("ViewDocs", new { prevPage, pId });
        }




        public async Task<IActionResult> OnGetApproved(string pId)
        {
            var docAttachment = await _docRepo.GetOne(pId);
            var account = await _userManager.FindByNameAsync(User.Identity?.Name);
            await _docTrackRepo.Add(new DocumentTracking
            {
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ReviewerId = account.Id,
                DocumentAttachmentId = docAttachment.Id,
                ReviewerStatus = ReviewerStatus.Approved
            });

			if(docAttachment.DocumentType != DocumentType.WalkIn)
			{
                var notification = new Notification
                {
                    Title = "Complete/Approved Document",
                    Recepient = docAttachment.SenderId,
                    IsViewed = false,
                    Description = "Your document is successfully reviewed by the designated CHED personels. You can now check the status of your document tracking.",
                    NotificationType = NotificationType.Document,
                    RedirectLink = "/Application/Document/Ended/Index",
                    AddedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };


                var settings = await _settingsRepo.GetAll();
                if (settings.OrderByDescending(x => x.Id).First().DocumentNotif)
                {
                    await _notificationRepo.Add(notification);
                    _notifHub.Clients.User(docAttachment.SenderId).ReceiveNotification(
                        notification.Title,
                        notification.Description.Length > 30 ? $"{notification.Description.Substring(0, 30)}..." : notification.Description,
                        notification.NotificationType.ToString(),
                        notification.AddedAt.ToString("MMMM dd, yyyy"),
                        notification.RedirectLink
                    );
                }
            }
            docAttachment.Status = Status.Approved;
            TempData["validation-message"] ="Successfully approved document";
            await _docRepo.Update(docAttachment, docAttachment.Id.ToString());
            return RedirectToPage("/Application/Document/Ended/Index");
        }
    }
}
