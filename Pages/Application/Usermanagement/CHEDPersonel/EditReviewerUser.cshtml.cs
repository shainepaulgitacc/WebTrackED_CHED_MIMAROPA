using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Service;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel.BaseIdentityUser;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Admin.Usermanagement.Reviewer
{
	[Authorize(Roles = "Admin")]
	[ValidateAntiForgeryToken]
	public class EditReviewerUserModel : PageModel
    {
        private readonly ICHEDPersonelRepository _chedRepo;
        private readonly IBaseRepository<Office> _officeRepo;
        private readonly IBaseRepository<Designation> _desigRepo;
        private readonly IBaseRepository<AppIdentityUser> _accRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly FileUploader _fileUploader;
        private readonly IUserStore<AppIdentityUser> _userStore;
        private readonly IUserEmailStore<AppIdentityUser> _emailStore;
        public EditReviewerUserModel(
            ICHEDPersonelRepository chedRepo,
            IBaseRepository<Office> officeRepo,
            IBaseRepository<Designation> desigRepo,
            IBaseRepository<AppIdentityUser> accRepo,
            UserManager<AppIdentityUser> userManager,
            IUserStore<AppIdentityUser> userStore,
            FileUploader fileUploader)
        {
            _chedRepo = chedRepo;
            _officeRepo = officeRepo;
            _desigRepo = desigRepo;
            _accRepo = accRepo;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _userManager = userManager;
            _fileUploader = fileUploader;
        }
        [BindProperty]
        public CHEDPersonelInputModel Input { get; set; }

        public IEnumerable<Office> Offices { get; set; }
        public IEnumerable<Designation> Designations { get; set; }
        public async Task<IActionResult> OnGetAsync(int Id)
        {
            var personels =  await _chedRepo.CHEDPersonelRecords();
            var personel = personels.FirstOrDefault(x => x.CHEDPersonel.Id  == Id);
            if (personel == null)
                return RedirectToPage($"{Id} is not valid");
            Input = new CHEDPersonelInputModel
            {
                ReviewerId = (int)personel.CHEDPersonel.Id,
                AddedAt = personel.CHEDPersonel.AddedAt,
                IdentityUserId = personel.Account.Id,
                Username = personel.Account.UserName,
                Email = personel.Account.Email,
                FirstName = personel.Account.FirstName,
                MiddleName = personel.Account.MiddleName,
                LastName = personel.Account.LastName,
                Suffixes = personel.Account.Suffixes,
                OfficeId = personel.Office != null ? personel.Office.Id : null,
                DesignationId = personel.Designation != null ? personel.Designation.Id : null,    
                PhoneNumber = personel.Account.PhoneNumber,
                Active = personel.Account.Active,
                Address = personel.Account.Address, 
                DateOfBirth = (DateTime)personel.Account.DateOfBirth,
                Sex = (Sex)personel.Account.Sex,
                TypeOfUser = personel.Account.TypeOfUser,
                Password = personel.Account.PasswordHash,
                ConfirmPassword = personel.Account.PasswordHash
            };

            Offices = await _officeRepo.GetAll();
            Designations = await _desigRepo.GetAll();
            return Page();
        }
        public async Task<IActionResult> OnGetActivation(string accId,bool isActive)
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

            var chedAccs = await _chedRepo.GetAll();
            int Id = (int)chedAccs.FirstOrDefault(x => x.IdentityUserId == acc.Id).Id;
            return RedirectToPage("EditReviewerUser", new {Id});
        }
        private IUserEmailStore<AppIdentityUser> GetEmailStore()
        {
            return (IUserEmailStore<AppIdentityUser>)_userStore;
        }
        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await _accRepo.GetOne(Input.IdentityUserId);
            var oldProfile = user.ProfileFileName;
            var newProfile = await _fileUploader.Uploadfile(Input.ProfileFile, "ProfilePicture");

            user.Id = Input.IdentityUserId;
            user.FirstName = Input.FirstName;
            user.MiddleName = Input.MiddleName;
            user.LastName = Input.LastName;
            user.UserName = Input.Username;
            user.Email = Input.Email;
            user.EmailConfirmed = true;
            user.PhoneNumber = Input.PhoneNumber;
            user.TypeOfUser = Input.TypeOfUser;
            user.Sex = Input.Sex;
            user.DateOfBirth = Input.DateOfBirth;
            user.Address = Input.Address;
            user.ProfileFileName = newProfile != null ? newProfile : oldProfile;
            user.Active = Input.Active;
            user.PasswordHash = Input.Password;
            var cPersonel = new CHEDPersonel()
            {
                Id =(int)Input.ReviewerId,
                DesignationId = Input.DesignationId,
                OfficeId = Input.OfficeId,
                IdentityUserId = Input.IdentityUserId
            };
            var Id = cPersonel.Id;
            try
            {
                await _accRepo.Update(user, user.Id);
            }
            catch(DbUpdateException ex)
            {
                TempData["validation-message"] = ex.InnerException?.Message;
                return RedirectToPage("EditReviewerUser", new { Id });
            }
            await _chedRepo.Update(cPersonel, cPersonel.Id.ToString());
            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);
            // Add the new role to the user
            await _userManager.AddToRoleAsync(user, user.TypeOfUser == TypeOfUser.Reviewer ? "Reviewer" : "Admin");
            TempData["validation-message"] = "Successfully Updated";

            return RedirectToPage("EditReviewerUser", new {Id});
        }
    }
}
