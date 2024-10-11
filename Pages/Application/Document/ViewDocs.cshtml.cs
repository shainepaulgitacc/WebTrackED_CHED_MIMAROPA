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
        private readonly QRCode_Generator _qrGenerator;
		private readonly IMapper _mapper;
        private readonly IBaseRepository<Designation> _desigRepo;
        private readonly IWebHostEnvironment _env;

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
            QRCode_Generator qrGenerator,
           IBaseRepository<Designation> desigRepo,
           IWebHostEnvironment env,

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
            _qrGenerator = qrGenerator;
			_settingsRepo = settingsRepo;
            _desigRepo = desigRepo;
            _env = env;
           
		}
		public string PreviousPage { get; set; }

		[BindProperty]
		public IFormFile? NewDocuments { get; set; }

		[BindProperty]
		public Prioritization? Prioritization { get; set; }
        public string FirstDesignationName { get; set; }

		public DocumentAttachmentViewModel DocumentAttachment { get; set; }
		public Designation Designation { get; set; }
		public string Logo { get; set; }
        public string ReviewerDesignationName { get; set; }
        public ReviewerStatus CurrentStatus { get; set; }

		//public bool HasCurrentlyReviewing { get; set; }
	public string AccountId { get; set; }

		public async Task<IActionResult> OnGetAsync(string prevPage, int pId)
		{
			PreviousPage = prevPage;
			var docsAttachments = await _docRepo.DocumentAttachments();
			var docAttachment = docsAttachments.FirstOrDefault(x => x.DocumentAttachment.Id == pId);
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
           var reviewers = await _reviewerRepo.GetAll();
            var accounts = _userManager.Users.ToList();
            var designations = await _desigRepo.GetAll();

            ReviewerDesignationName = reviewers
                .Join(designations,
                r => r.DesignationId,
                o => o.Id,
                (r, o) => new
                {
                    Reviewer = r,
                    Designation = o
                })
                .Join(accounts,
                r => r.Reviewer.IdentityUserId,
                a => a.Id,
                (r, a) => new
                {
                    Reviewer = r.Reviewer,
                    Designation = r.Designation,
                    Account = a
                })
                .FirstOrDefault(x => x.Account.Id == user.Id)?
                .Designation.DesignationName;


            FirstDesignationName = designations.OrderBy(x => x.AddedAt).First().DesignationName;
            var getRoles = await _userManager.GetRolesAsync(user);
            if (docAttachment == null)
                return BadRequest($"Unknown document");
            if (User.IsInRole("Sender") && docAttachment.DocumentAttachment.SenderId != user.Id)
                return BadRequest("Can't access this page");
			DocumentAttachment = docAttachment;
			AccountId = user.Id;
            CurrentStatus = (ReviewerStatus)docAttachment.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == user.Id)?.ReviewerStatus;
           


            var settings = await _settingsRepo.GetAll();
            Logo = settings.OrderByDescending(x => x.Id).First().LogoFileName;
            var refCode = docAttachment.DocumentAttachment.DocumentType == DocumentType.WalkIn ? $"dW{pId.ToString("00000")}" : $"dE{pId.ToString("00000")}";
            var qrCode = _qrGenerator.GenerateCode(refCode);
            string imagebase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCode)}";
            ViewData["qr-code"] = imagebase64;
            return Page();



		}
		public async Task<IActionResult> OnGetDownloadFile(string filename)
		{
			try
			{
				byte[] fileBytes = await _fileUploader.DownloadFile(filename);
                var splitedFileName = filename.Split("=");
				return File(fileBytes, "application/octet-stream", splitedFileName[splitedFileName.Length - 1]);
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

        /*
         
        public async Task<IActionResult> OnGetViewDocument(string filename)
        {
            try
            {
                // Get the file (path and content type) from your file uploader service
                var result = await _fileUploader.ViewFile(filename);

                // Split the file name if needed, assuming filename has '=' characters
                var splitFile = filename.Split('=');
                var finalFileName = splitFile[splitFile.Length - 1];

                // Serve the file for viewing
                var physicalFileResult = PhysicalFile(result.Item1, result.Item2);

                // Save the file as "SavedShaine.pdf" after serving it
                await SaveFileAs(result.Item1, "SavedShaine.pdf");

                return physicalFileResult;
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


        private async Task SaveFileAs(string originalFilePath, string newFileName)
        {
            try
            {
                // Define the folder where files are stored
                const string folderName = "Documents";

                // Path for the new file in wwwroot
                string newFilePath = Path.Combine(_env.WebRootPath, folderName, newFileName);

                // Check if the original file exists
                if (System.IO.File.Exists(originalFilePath))
                {
                    // Copy the original file to a new file with the specified name
                    await Task.Run(() => System.IO.File.Copy(originalFilePath, newFilePath, true)); // Overwrite if exists
                }
            }
            catch (Exception ex)
            {
                // Handle errors that might occur during the file save process
                Console.WriteLine($"Error while saving the file: {ex.Message}");
            }
        }

        */
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

        public async Task<IActionResult> OnGetViewDocumentAction(string prevPage, string pId,ReviewerStatus status)
        {
            var docAttachment = await _docRepo.GetOne(pId);
            var documentTrackings = await _docTrackRepo.GetAll();
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

          
            var settings = await _settingsRepo.GetAll();
            if (settings.OrderByDescending(x => x.Id).First().DocumentNotif && status == ReviewerStatus.Reviewed)
            {
                var notification = new Notification
                {
                    Title = "Document Reviewed",
                    Recepient = docAttachment.SenderId,
                    IsViewed = false,
                    Description = $"Your document has been reviewed by ({reviewer.Designation.DesignationName}).",
                    NotificationType = NotificationType.Document,
                    RedirectLink = docAttachment.DocumentType != DocumentType.WalkIn ? "/Application/Document/Onprocess/Index" : "/Application/Document/Outgoing/Index",
                    AddedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                await _notificationRepo.Add(notification);
                _notifHub.Clients.User(docAttachment.SenderId).ReceiveNotification(
                    notification.Title,
                    notification.Description.Length > 30 ? $"{notification.Description.Substring(0, 30)}..." : notification.Description,
                    notification.NotificationType.ToString(),
                    notification.AddedAt.ToString("MMMM dd, yyyy"),
                    notification.RedirectLink
                );
            }
            foreach (var tracking in documentTrackings.Where(x => x.DocumentAttachmentId == int.Parse(pId)))
            {
                _notifHub.Clients.Users(tracking.ReviewerId).ReviewerRealtime();
            }
            TempData["validation-message"] = "Successfully perform the action.";
            return RedirectToPage("ViewDocs", new { prevPage, pId });
        }


        public async Task<IActionResult> OnGetReviewed(string prevPage, string pId)
        {
            var docAttachment = await _docRepo.GetOne(pId);
            var documentTrackings = await _docTrackRepo.GetAll();
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
                Description =$"Your document has been reviewed by ({reviewer.Designation.DesignationName}).",
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

    }
}
