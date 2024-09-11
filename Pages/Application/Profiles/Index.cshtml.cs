using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Runtime.InteropServices;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Service;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Profiles
{
    public class ChangeProfileInputModel
    {
        public IFormFile ProfileFile { get; set; }
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly ICHEDPersonelRepository _chedRepo;
        private readonly ISenderRepository _senderRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IBaseRepository<AppIdentityUser> _accRepo;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly FileUploader _fileUploader;

        public IndexModel(
            ICHEDPersonelRepository chedRepo,
            ISenderRepository senderRepo,
            UserManager<AppIdentityUser> userManager,
            IBaseRepository<AppIdentityUser> accRepo,
            SignInManager<AppIdentityUser> signInManager,
            FileUploader fileUploader)
        {
            _chedRepo = chedRepo;
            _senderRepo = senderRepo;
            _userManager = userManager;
            _accRepo = accRepo;
            _signInManager = signInManager;
            _fileUploader = fileUploader;
        }

        public string PreviousPage { get; set; }
        public SenderListViewModel SenderUser { get; set; }
        public CHEDPersonelListViewModel CHEDPUser { get; set; }
        public ChangeProfileInputModel ChangeProfileInput { get; set; }
        public UpdatePasswordInputModel UpdatePasswordInput { get; set; }

        public string? AccId { get; set; }

        public IEnumerable<AppIdentityUser> Accounts { get; set; }

        public async Task OnGetAsync(string accId = null)
        {
            Accounts = await _accRepo.GetAll();
            if (accId != null)
            {
                AccId = accId;
                var senders = await _senderRepo.SenderRecords();
                SenderUser = senders.FirstOrDefault(x => x.User.Id == accId);
            }
            else
            {
                var acc = await _userManager.FindByNameAsync(User.Identity.Name);
                if (User.IsInRole("Reviewer") || User.IsInRole("Admin"))
                {
                    var chedPersonels = await _chedRepo.CHEDPersonelRecords();
                    CHEDPUser = chedPersonels.FirstOrDefault(x => x.Account.Id == acc.Id);
                }
                else
                {
                    var senders = await _senderRepo.SenderRecords();
                    SenderUser = senders.FirstOrDefault(x => x.User.Id == acc.Id);
                }
            }

        }
        public async Task<IActionResult> OnPostChangeProfilePicture(ChangeProfileInputModel ChangeProfileInput,string userId)
        {
            this.ChangeProfileInput = ChangeProfileInput;   
            var acc = await _accRepo.GetOne(userId);
            if (!TryValidateModel(this.ChangeProfileInput))
                return BadRequest(ModelState);
            if (acc == null)
                return BadRequest($"{userId} is invalid userId");
            acc.ProfileFileName = await _fileUploader.Uploadfile(this.ChangeProfileInput.ProfileFile, "ProfilePicture");
            await _accRepo.Update(acc,acc.Id);
            TempData["validation-message"] = "Successfully change the profile picture";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdatePassword(UpdatePasswordInputModel UpdatePasswordInput, string userId)
        {
            this.UpdatePasswordInput = UpdatePasswordInput;
            var acc = await _accRepo.GetOne(userId);
            if (!TryValidateModel(this.UpdatePasswordInput))
                return BadRequest(ModelState);
            if (acc == null)
                return BadRequest($"{userId} is invalid userId");
            var result = await _userManager.ChangePasswordAsync(acc, this.UpdatePasswordInput.OldPassword, this.UpdatePasswordInput.ConfirmPassword);
            if (result.Succeeded)
                TempData["validation-message"] = "Successfully change the password";
            else
                TempData["validation-message"] = result.Errors.First().Description;

            return RedirectToPage();
        }
        public async Task<IActionResult> OnGetDelete(string Id)
        {
            if (Id == null)
                return BadRequest("Id is Null");
            try
            {
                var chedPersonels = await _chedRepo.GetAll();
                var filtered = chedPersonels.FirstOrDefault(x => x.IdentityUserId == Id);
                await _chedRepo.Delete(filtered?.Id.ToString());
                await _accRepo.Delete(Id);
                TempData["validation-message"] = "Successfully deleted";
                await _signInManager.SignOutAsync();
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

    }
}
