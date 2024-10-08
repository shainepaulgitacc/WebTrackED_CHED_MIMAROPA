using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel.BaseIdentityUser;
using Microsoft.AspNetCore.Authorization;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Profiles
{
	[Authorize]
	[ValidateAntiForgeryToken]
	public class UpdateProfileModel : PageModel
	{
		private readonly ICHEDPersonelRepository _chedRepo;
		private readonly ISenderRepository _senderRepo;
		private readonly UserManager<AppIdentityUser> _userManager;
		private readonly SignInManager<AppIdentityUser> _signInManager;	
		private readonly IBaseRepository<AppIdentityUser> _accRepo;
		private readonly IBaseRepository<Office> _officeRepo;
		private readonly IBaseRepository<Designation> _designationRepo; 
		private readonly IMapper _mapper;
		

		public UpdateProfileModel(
			ICHEDPersonelRepository chedRepo,
			ISenderRepository senderRepo,
			UserManager<AppIdentityUser> userManager,
			SignInManager<AppIdentityUser> signInManager,
			IBaseRepository<AppIdentityUser> accRepo,
			IBaseRepository<Office> officeRepo,
			IBaseRepository<Designation> designationRepo,
			IMapper mapper
			)
		{
			_chedRepo = chedRepo;
			_senderRepo = senderRepo;
			_userManager = userManager;
			_signInManager = signInManager;
			_accRepo = accRepo;
			_officeRepo = officeRepo;
			_designationRepo = designationRepo;
			_mapper = mapper;
		}

		public string PreviousPage { get; set; }
		public SenderListViewModel SenderUser { get; set; }
		public CHEDPersonelListViewModel CHEDPUser { get; set; }
		public ChangeProfileInputModel ChangeProfileInput { get; set; }
		public UpdatePasswordInputModel UpdatePasswordInput { get; set; }

		public IEnumerable<AvailableOffice> Offices { get; set; }
		public IEnumerable<AvailableDesignation> Designations { get; set; }

		// Input models
		public CHEDPersonelInputModel ReviewerInput { get; set; }
		public SenderInputModel SenderInput { get; set; }
		public BaseIdentityUserInputModel AccInput { get; set; }

		public async Task OnGetAsync(string accId = null)
		{
			var reviewers = await _chedRepo.GetAll();
			var offices = await _officeRepo.GetAll();	
			var designations = await _designationRepo.GetAll();
            Offices = offices.
               GroupJoin(reviewers,
               o => o.Id,
               r => r.OfficeId,
               (o, r) => new
               {
                   Office = o,
                   Reviewer = r.FirstOrDefault()
               })
             //  .Where(x => x.Reviewer == null)
               .Select(r => new AvailableOffice
               {
                   OfficeId = r.Office.Id,
                   OfficeName = r.Office.OfficeName
               });
            Designations = designations
                .GroupJoin(reviewers,
                d => d.Id,
                r => r.DesignationId,
                (d, r) => new
                {
                    Designation = d,
                    Reviewer = r.FirstOrDefault()
                })
               // .Where(x => x.Reviewer == null)
                .Select(r => new AvailableDesignation
                {
                    Id = r.Designation.Id,
                    DesignationName = r.Designation.DesignationName,
                });

            if (accId != null)
			{
				var senders = await _senderRepo.SenderRecords();
				var sender = senders.FirstOrDefault(x => x.User.Id == accId)?.Sender;
				SenderInput = _mapper.Map<SenderInputModel>(sender);
			}
			else
			{
				var acc = await _userManager.FindByNameAsync(User.Identity.Name);
				if (User.IsInRole("Reviewer") || User.IsInRole("Admin"))
					{
					var chedPersonels = await _chedRepo.CHEDPersonelRecords();
					var reviewer = chedPersonels.FirstOrDefault(x => x.Account.Id == acc.Id);


					ReviewerInput = new CHEDPersonelInputModel
					{
						IdentityUserId = reviewer.Account.Id,
						Username = reviewer.Account.UserName,
						Email = reviewer.Account.Email,
						FirstName = reviewer.Account.FirstName,
						MiddleName = reviewer.Account.MiddleName,
						LastName = reviewer.Account.LastName,
						Suffixes = reviewer.Account.Suffixes,
						PhoneNumber = reviewer.Account.PhoneNumber,
						Address = reviewer.Account.Address,
						DateOfBirth = reviewer.Account.DateOfBirth,
						Sex = (Sex)reviewer.Account.Sex,

						ReviewerId = (int)reviewer?.CHEDPersonel.Id,
						OfficeId = reviewer.Office?.Id,
						DesignationId = reviewer.Designation?.Id,
					};
				}
				else
				{
					var senders = await _senderRepo.SenderRecords();
					var sender = senders.FirstOrDefault(x => x.User.Id == acc.Id);
					SenderInput = new SenderInputModel
					{
						IdentityUserId = sender.User.Id,
						Username = sender.User.UserName,
						Email = sender.User.Email,
						FirstName = sender.User.FirstName,
						MiddleName = sender.User.MiddleName,
						LastName = sender.User.LastName,
						Suffixes = sender.User.Suffixes,
						PhoneNumber = sender.User.PhoneNumber,
						Active = sender.User.Active,
						Address = sender.User.Address,
						DateOfBirth = sender.User.DateOfBirth,
						Sex = (Sex)sender.User.Sex,


						SenderId = (int)sender.Sender.Id,
						Designation = sender.Sender.Designation,
						Department = sender.Sender.Department,
						DateOfHire = sender.Sender.DateOfHire,
						EmploymentStatus = (EmploymentStatus)sender.Sender.EmploymentStatus,
						WorkLocationOffice = sender.Sender.WorkLocationOffice,
						ProjectAssignment = sender.Sender.ProjectAssignment,
						SkillCompetencies = sender.Sender.SkillCompetencies,
						AchievementsAwards = sender.Sender.AchievementsAwards
					};
				}
			}
		}

		public async Task<IActionResult> OnPostUpdateSender(SenderInputModel senderInput)
		{
			SenderInput = senderInput;
			if (!TryValidateModel(SenderInput))
				return BadRequest(ModelState);

			var converted = _mapper.Map<Sender>(SenderInput);
			converted.Id = (int)SenderInput.SenderId;
			converted.UpdatedAt = DateTime.Now;
			await _senderRepo.Update(converted, converted.Id.ToString());

			

			var user = await _userManager.FindByIdAsync(SenderInput.IdentityUserId);
			await _signInManager.SignOutAsync();
			user.Email = SenderInput.Email;
			user.NormalizedEmail = SenderInput.Email.ToUpper();
			user.UserName = SenderInput.Username;
			user.NormalizedUserName = SenderInput.Username.ToUpper();	
			user.FirstName = SenderInput.FirstName;
			user.LastName = SenderInput.LastName;
			user.MiddleName = SenderInput.MiddleName;
			user.Suffixes = SenderInput.Suffixes;
			user.Address = SenderInput.Address;
			user.Sex = SenderInput.Sex;
			user.MaritalStatus = SenderInput.MaritalStatus;
			user.PhoneNumber = SenderInput.PhoneNumber;
			user.DateOfBirth = SenderInput.DateOfBirth;
			await _accRepo.Update(user, user.Id);

			TempData["validation-message"] = "Successfully updated";
			await _signInManager.SignInAsync(user, isPersistent: true);

			return RedirectToPage();
		}


		public async Task<IActionResult> OnPostUpdateCHEDPersonel(CHEDPersonelInputModel reviewerInput)
		{
			ReviewerInput = reviewerInput;
			if (!TryValidateModel(ReviewerInput))
				return BadRequest(ModelState);

			var converted = _mapper.Map<CHEDPersonel>(ReviewerInput);
			converted.Id = (int)ReviewerInput.ReviewerId;
			converted.UpdatedAt = DateTime.Now;
			await _chedRepo.Update(converted, converted.Id.ToString());

			var user = await _userManager.FindByIdAsync(ReviewerInput.IdentityUserId);
			await _signInManager.SignOutAsync();
			user.Email = ReviewerInput.Email;
			user.NormalizedEmail = ReviewerInput.Email.ToUpper();
			user.UserName = ReviewerInput.Username;
			user.NormalizedUserName = ReviewerInput.Username.ToUpper();
			user.FirstName = ReviewerInput.FirstName;
			user.LastName = ReviewerInput.LastName;
			user.MiddleName = ReviewerInput.MiddleName;
			user.Suffixes = ReviewerInput.Suffixes;
			user.Address = ReviewerInput.Address;
			user.Sex = ReviewerInput.Sex;
			user.MaritalStatus = ReviewerInput.MaritalStatus;
			user.PhoneNumber = ReviewerInput.PhoneNumber;
			user.DateOfBirth = ReviewerInput.DateOfBirth;
			await _accRepo.Update(user, user.Id);

			TempData["validation-message"] = "Successfully updated";
			await _signInManager.SignInAsync(user, isPersistent: true);

			return RedirectToPage();
		}
	}
}
