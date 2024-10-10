using Aspose.Pdf.Drawing;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Service;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel.BaseIdentityUser;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Admin.User.CHED_Personel
{
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IBaseRepository<Designation> _designationRepo;
        private readonly IBaseRepository<AppIdentityUser> _accRepo;
      
        private readonly FileUploader _fileUploader;
        private readonly ICHEDPersonelRepository _cpRepo;
        private readonly IUserStore<AppIdentityUser> _userStore;
        private readonly IUserEmailStore<AppIdentityUser> _emailStore;
        private readonly IBaseRepository<Settings> _settingsRepo;
        private IMapper _mapper { get; set; }
        public IndexModel(UserManager<AppIdentityUser> userManager,
                          ICHEDPersonelRepository cpRepo,
                          IBaseRepository<AppIdentityUser> accRepo,
                          IMapper mapper,
                          FileUploader fileUploader,
                          IUserStore<AppIdentityUser> userStore,
						  IBaseRepository<Settings> settingsRepo
						  )
        {
            _userManager = userManager;
            _accRepo = accRepo;
            _cpRepo = cpRepo;   
            _mapper = mapper;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _fileUploader = fileUploader;
            _settingsRepo = settingsRepo;
        }

        public CHEDPersonelInputModel InputModel { get; set; }
        public List<CHEDPersonelListViewModel> CHEDPersonels { get; set; }
        public UpdatePasswordInputModel UpdatePasswordInput { get; set; }

        public async  Task OnGetAsync()
        {
            var personels = await _cpRepo.CHEDPersonelRecords();
            var account = await _userManager.FindByNameAsync(User.Identity?.Name);
            CHEDPersonels = personels.Where(x => x.Account.Id != account?.Id && x.Designation != null).ToList();
          
        }
        public async Task<IActionResult> OnPostAsync(CHEDPersonelInputModel InputModel)
        {
            if(!TryValidateModel(InputModel))
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
            var cPersonel = new CHEDPersonel()
            {
                DesignationId = InputModel.DesignationId,
             
                User = account
            };
			var settings = await _settingsRepo.GetAll();
			var emailDomain = settings.OrderByDescending(x => x.Id).First().EmailDomain;

			if (!account.Email.Contains(emailDomain))
            {
                TempData["validation-message"] = "The provided CHED personnel email address is invalid. Please enter a valid email.";
                await OnGetAsync();
				return Page();
			}
			await _userStore.SetUserNameAsync(account, account.UserName, CancellationToken.None);
           
            
            await _emailStore.SetEmailAsync(account, account.Email, CancellationToken.None);

            var isSuccess = await _userManager.CreateAsync(account, InputModel.Password);
            if (isSuccess.Succeeded)
            {
                await _cpRepo.Add(cPersonel);
                await _userManager.AddToRoleAsync(account,"Reviewer");
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
                var chedPersonels = await _cpRepo.GetAll();
                var filtered = chedPersonels.FirstOrDefault(x => x.IdentityUserId == Id);
                await _cpRepo.Delete(filtered?.Id.ToString());
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

			/*
			public async Task<IActionResult> OnGetDelete(string Id)
			{
				if (Id == null)
					return BadRequest("Id is Null");
				try
				{
					var chedPersonels = await _cpRepo.GetAll();
					var filtered = chedPersonels.FirstOrDefault(x => x.IdentityUserId== Id);
					var userToDelete = await _accRepo.GetOne(Id);
					await _userManager.UpdateSecurityStampAsync(userToDelete);

					await _cpRepo.Delete(filtered.Id.ToString());
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

			*/

			public async Task<IActionResult> OnPostUpdatePassword(UpdatePasswordInputModel UpdatePasswordInput, string userId)
        {
            var acc = await _accRepo.GetOne(userId);
            if (!TryValidateModel(UpdatePasswordInput))
                return BadRequest(ModelState);
            if (acc == null)
                return BadRequest($"{userId} is invalid userId");
			var userToDelete = await _accRepo.GetOne(userId);
			await _userManager.UpdateSecurityStampAsync(userToDelete);
			var result = await _userManager.ChangePasswordAsync(acc, UpdatePasswordInput.OldPassword, UpdatePasswordInput.ConfirmPassword);
            if (result.Succeeded)
                TempData["validation-message"] = "Successfully change the password";
            else
                TempData["validation-message"] = result.Errors.First().Description;

            return RedirectToPage();
        }
    }
}
