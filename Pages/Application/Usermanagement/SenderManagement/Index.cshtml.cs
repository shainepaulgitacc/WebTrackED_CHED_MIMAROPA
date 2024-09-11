using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Service;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel.BaseIdentityUser;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Usermanagement.SenderManagement
{
    [Authorize(Roles ="Admin")]
    public class IndexModel : PageModel
    {
        private readonly ISenderRepository _sRepo;
        private readonly IBaseRepository<AppIdentityUser> _accRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly FileUploader _fileUploader;
        private readonly IUserStore<AppIdentityUser> _userStore;
        private readonly IUserEmailStore<AppIdentityUser> _emailStore;
        public IndexModel(
            ISenderRepository sRepo,
            UserManager<AppIdentityUser> userManager,
            IBaseRepository<AppIdentityUser> accRepo,
            IMapper mapper,
            FileUploader fileUploader,
            IUserStore<AppIdentityUser> userStore
           )
        {
            _sRepo = sRepo;
            _userManager = userManager;
            _accRepo = accRepo;
            _mapper = mapper;
            _fileUploader = fileUploader;
            _userStore = userStore;
            _emailStore = GetEmailStore();
        }
        public List<SenderListViewModel> Senders { get; set; }
        public async Task OnGetAsync()
        {
            var senders = await _sRepo.SenderRecords();
            Senders = senders.Where(x => x.User.UserName != User.Identity?.Name).ToList();
        }

		public async Task<IActionResult> OnPostAsync(SenderInputModel InputModel)
		{
			if (!TryValidateModel(InputModel))
				return BadRequest(ModelState);
			var account = new AppIdentityUser()
			{
				ProfileFileName = await _fileUploader.Uploadfile(InputModel.ProfileFile, "ProfilePicture"),
				FirstName = InputModel.FirstName,
				MiddleName = InputModel.MiddleName,
				Suffixes = InputModel.Suffixes,
				LastName = InputModel.LastName,
				UserName = InputModel.Username,
				Email = InputModel.Email,
				EmailConfirmed = true,
				PhoneNumber = InputModel.PhoneNumber,
				TypeOfUser = InputModel.TypeOfUser,
				Address = InputModel.Address,
				Sex = InputModel.Sex,
				MaritalStatus = InputModel.MaritalStatus,
				DateOfBirth = InputModel.DateOfBirth,
				Active = true
			};
            var converted = _mapper.Map<Sender>(InputModel);
            converted.User = account;

			await _userStore.SetUserNameAsync(account, account.UserName, CancellationToken.None);
			await _emailStore.SetEmailAsync(account, account.Email, CancellationToken.None);
			var isSuccess = await _userManager.CreateAsync(account, InputModel.Password);
			if (isSuccess.Succeeded)
			{
				await _sRepo.Add(converted);
				await _userManager.AddToRoleAsync(account, "Sender");
				TempData["validation-message"] = "Successfully added";
			}
			else
			{
				foreach (var error in isSuccess.Errors)
				{
					TempData["validation-message"] = error.Description;
				}
				await OnGetAsync();
				return Page();

			}



			return RedirectToPage();
		}


        private IUserEmailStore<AppIdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppIdentityUser>)_userStore;
        }


        public async Task<IActionResult> OnGetDelete(string Id, string? returnUrl = null, string? pId = null, bool hasMessage = true)
		{
            if (Id == null)
                return BadRequest("Id is Null");
            try
            {
                var chedPersonels = await _sRepo.GetAll();
                var filtered = chedPersonels.FirstOrDefault(x => x.IdentityUserId == Id);
                await _sRepo.Delete(filtered?.Id.ToString());
                await _accRepo.Delete(Id);
                TempData["validation-message"] = "Successfully deleted";
                return RedirectToPage();

            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlException && (sqlException.Number == 547 || sqlException.Number == 515))
                {
                    TempData["validation-message"] = "Delete the child data first";
                    return RedirectToPage();
                }
                else
                {
                    TempData["validation-message"] = "Unknown error";
                    return RedirectToPage();
                }
            }
        }
		public async Task<IActionResult> OnGetActivation(string accId, bool isActive)
        {
            var acc = await _accRepo.GetOne(accId);
            if (isActive)
            {
                acc.Active = false;
                await _userManager.UpdateSecurityStampAsync(acc);
            }
            else
            {
                acc.Active = true;
            }
            await _accRepo.Update(acc, acc.Id);
            TempData["validation-message"] = "Successfully perform action";
            return RedirectToPage();
        }
    }
}
