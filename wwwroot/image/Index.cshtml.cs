using AutoMapper;
using BOS_MinSU.Models.Domain;
using BOS_MinSU.Models.Infrastracture.Contracts;
using BOS_MinSU.Models.ViewModel.InputModel;
using BOS_MinSU.Models.ViewModel.ListModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BOS_MinSU.Pages.OutgoingDocument
{
    public class IndexModel : BasePageModel<Document, DocumentInput>
	{
		private readonly IDocumentRepository _docRepo;
		private readonly UserManager<AppIdentityUser> _userManager;
		public IndexModel(
			IDocumentRepository docRepo,
			IMapper mapper,
			UserManager<AppIdentityUser> userManager
			) : base(docRepo, mapper)
		{
			_docRepo = docRepo;
			_userManager = userManager;
		}
		public List<DocumentRecord> Documents { get; set; }
		public async Task OnGetAsync()
		{
			var documents = await _docRepo.DocumentJoined();
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			Documents = documents.Where(x => x.DocumentTracking.ReviewerId != user?.Id  && !(x.DocumentTracking.DocumentTrackingStatus == DocumentTrackingStatus.Disapproved || x.DocumentTracking.DocumentTrackingStatus == DocumentTrackingStatus.Completed)).ToList();

		}
	}
}
