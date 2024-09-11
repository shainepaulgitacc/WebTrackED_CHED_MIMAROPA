using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Management.SubCategoryManagement
{
	[Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
	public class Subcategory_ProceduresModel : PageModel
    {
        private readonly IBaseRepository<Procedure> _pRepo;
        private readonly IBaseRepository<SubCategory> _subCategRepo;
        private readonly IDocumentAttachmentRepository _docsAttachRepo;
        private readonly IBaseRepository<DocumentProcedure> _docsProcedRepo;
        private readonly IMapper _mapper;
        public Subcategory_ProceduresModel(
            IBaseRepository<Procedure> pRepo, 
            IBaseRepository<SubCategory> subCategRepo,
            IDocumentAttachmentRepository docsAttachRepo,
            IBaseRepository<DocumentProcedure> docsProcedRepo,
            IMapper mapper) 
        {
            _pRepo = pRepo;
            _subCategRepo = subCategRepo;
            _docsAttachRepo = docsAttachRepo;
            _docsProcedRepo = docsProcedRepo;
            _mapper = mapper;
        }
        
        public int SubCategoryId { get; set; }
        public int pId { get; set; }
        [BindProperty]
        public ProcedureInputModel InputModel { get; set; }
        public List<Procedure> Records { get; set; }
        public async Task OnGetAsync(int pId)
        {
            var procedures = await _pRepo.GetAll();
            Records = procedures.Where(x => x.SubCategoryId == pId).ToList();
            SubCategoryId = pId;
            this.pId = pId;
        }
        [BindProperty]
        public string? Ids { get; set; }
        public async Task<IActionResult> OnPostDeleteAll(int pId)
        {
            if(string.IsNullOrEmpty(Ids))
                return RedirectToPage();
            
            var docProcedures = await _docsProcedRepo.GetAll();
            var docAttachments = await _docsAttachRepo.GetAll();
            if(docProcedures.Count() > 0)
            {
                var sIds = Ids.Split(',');
                foreach (var sId in sIds)
                {
                    var proced = await _pRepo.GetOne(sId);
                    var docsProceduresF = docProcedures.FirstOrDefault(x => x.ProcedureTitle == proced.ProcedureTitle && !docAttachments.Any(s => s.Id == x.DocumentAttachmentId && s.Status == Status.Disapproved || s.Status == Status.Approved));
                    if (docsProceduresF != null)
                    {
                        await _docsProcedRepo.Delete(docsProceduresF.Id.ToString());
                    }
                  
                }
            }
            await _pRepo.DeleteAll(Ids);
            TempData["validation-message"] = "Successfully deleted";
            return RedirectToPage("Subcategory_Procedures", new {pId});
        }

        public async Task<IActionResult> OnPost()
        {
           if(!ModelState.IsValid)
                return BadRequest(ModelState);
            var converted = _mapper.Map<Procedure>(InputModel);
            var procedures = await _pRepo.GetAll();
            var pId = InputModel.SubCategoryId;

            if(procedures.Any(x => x.ProcedureTitle == InputModel.ProcedureTitle && x.SubCategoryId == InputModel.SubCategoryId))
            {
				TempData["validation-message"] = "Cannot add the procedure. The title must be unique.";
				return RedirectToPage("Subcategory_Procedures", new { pId });
			}
            await _pRepo.Add(converted);
            var docAttachments = await _docsAttachRepo.GetAll();
            var filtered = docAttachments.Where(x => !(x.Status == Status.Approved || x.Status == Status.Disapproved) && x.SubCategoryId == InputModel.SubCategoryId).ToList();

            foreach(var docAttachment in filtered)
            {

                await _docsProcedRepo.Add(new DocumentProcedure
                {
                    DocumentAttachmentId = docAttachment.Id,
                    ProcedureDescription = InputModel.Description,
                    ProcedureTitle = InputModel.ProcedureTitle
                });
            }
            TempData["validation-message"] = "Successfully added";
            return RedirectToPage("Subcategory_Procedures", new { pId });

        }
    }
}
