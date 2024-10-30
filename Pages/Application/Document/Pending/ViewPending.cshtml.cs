using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Service;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document.Pending
{

	[Authorize(Roles = "Sender")]
    [ValidateAntiForgeryToken]
	public class ViewPendingModel:PageModel
    {
        private readonly IDocumentAttachmentRepository _docRepo;
        private readonly IBaseRepository<DocumentTracking> _docTrackingRepo;
        private readonly ICHEDPersonelRepository _reviewerRepo;
        private readonly IBaseRepository<Category> _categRepo;
        private readonly IBaseRepository<SubCategory> _subCategRepo;
        private readonly IMapper _mapper;
        private readonly FileUploader _fileUploader;
        private readonly IBaseRepository<Designation> _desigRepo;
        public ViewPendingModel(
            IDocumentAttachmentRepository docRepo,
            IBaseRepository<DocumentTracking> docTrackingRepo,
            ICHEDPersonelRepository reviewerRepo,
            IBaseRepository<Category> categRepo,
            IBaseRepository<SubCategory> subCategRepo,
            IMapper mapper,
            FileUploader fileUplaoder,
            IBaseRepository<Designation> desigRepo)
        {
            _docRepo = docRepo;
            _docTrackingRepo = docTrackingRepo;
            _reviewerRepo = reviewerRepo;
            _categRepo = categRepo;
            _subCategRepo = subCategRepo;
            _mapper = mapper;
            _fileUploader = fileUplaoder;
            _desigRepo = desigRepo;
        }
        public IEnumerable<CHEDPersonelListViewModel> Reviewers { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<SubCategory> SubCategories { get; set; }

        public DocumentAttachmentViewModel DocumentAttachment { get; set; }
        [BindProperty]
        public ComposeInputModel InputModel { get; set; }
        public async Task<IActionResult> OnGetAsync(int pId)
        {
            var docsAttachments = await _docRepo.DocumentAttachments();
            var docsAttachment = docsAttachments.FirstOrDefault(x => x.DocumentAttachment.Id == pId);
            if (docsAttachment == null)
                return BadRequest($"{pId} invalid DocumentId");
            DocumentAttachment = docsAttachment;
          
            var designations = await _desigRepo.GetAll();
            var reviewers = await _reviewerRepo.CHEDPersonelRecords();
            var recordDesignation = designations.OrderBy(x => x.AddedAt).First();
            var reviewerId = reviewers.FirstOrDefault(x => x.Designation != null && x.Designation.Id == recordDesignation.Id).Account.Id;
         
            InputModel = new ComposeInputModel
            {
                 
                Id = pId,
                AddedAt = docsAttachment.DocumentAttachment.AddedAt,
                CategoryId = docsAttachment.Category.Id,
                SubCategoryId = docsAttachment.SubCategory.Id,
                SenderId = docsAttachment.SenderAccount.Id,
                ReviewersId = reviewerId,
                Subject = docsAttachment.DocumentAttachment.Subject,
                Description = docsAttachment.DocumentAttachment.Description,
             
                DocumentType = docsAttachment.DocumentAttachment.DocumentType,
            };

             Reviewers = await _reviewerRepo.CHEDPersonelRecords();
            Categories = await _categRepo.GetAll();
            SubCategories = await _subCategRepo.GetAll(); 
            
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateDocs()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var convert = _mapper.Map<DocumentAttachment>(InputModel);
            var docAttachment = await _docRepo.GetOne(convert.Id.ToString());
			var documentPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Documents", docAttachment.FileName);
			if (System.IO.File.Exists(documentPath))
				System.IO.File.Delete(documentPath);
			var fileName = await _fileUploader.Uploadfile(InputModel.File, "Documents");
            convert.FileName = fileName != null ? fileName: docAttachment.FileName;
            await _docRepo.Update(convert, convert.Id.ToString());
            TempData["validation-message"] = "Successfully updated";
            var pId =  convert.Id.ToString();
            return RedirectToPage("ViewPending", new {pId});
        }

    }
}
