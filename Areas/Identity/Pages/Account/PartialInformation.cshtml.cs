// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebTrackED_CHED_MIMAROPA.Hubs;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Service;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel.BaseIdentityUser;

namespace WebTrackED_CHED_MIMAROPA.Areas.Identity.Pages.Account
{
    [ValidateAntiForgeryToken]
    public class PartialInformationModel: PageModel
    {
        private readonly IBaseRepository<CHEDPersonel> _chedPRepo;
        private readonly IBaseRepository<Sender> _senderRepo;
        private readonly IBaseRepository<AppIdentityUser> _accountRepo;
     
        private readonly IBaseRepository<Designation> _desigRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly IHubContext<NotificationHub, INotificationHub> _notifHub;
        private readonly IBaseRepository<Notification> _notificationRepo;
        private readonly FileUploader _fileUploader;
        private readonly IMapper _mapper;
        private readonly IBaseRepository<Settings> _settingsRepo;
        public PartialInformationModel(
            IBaseRepository<CHEDPersonel> chedPRepo,
            IBaseRepository<Sender> senderRepo,
            IBaseRepository<AppIdentityUser> accountRepo,
          
            IBaseRepository<Designation> desigRepo,
            IHubContext<NotificationHub, INotificationHub> notifHub,
            UserManager<AppIdentityUser> userManager,
            SignInManager<AppIdentityUser> signInManager,
            IBaseRepository<Notification> notificationRepo,
			IBaseRepository<Settings> settingsRepo,
		FileUploader fileUploader,
            IMapper mapper)
        {
            _signInManager = signInManager;
            _mapper = mapper;
            _accountRepo = accountRepo;
            _chedPRepo = chedPRepo;
            _senderRepo = senderRepo; 
       
            _desigRepo = desigRepo;
            _fileUploader = fileUploader;
            _userManager = userManager;
            _notifHub = notifHub;
            _notificationRepo = notificationRepo;
            _settingsRepo = settingsRepo;
        }
        [TempData]
        public string StatusMessage { get; set; }


        public string LogoFileName { get; set; }

        /*

        [BindProperty]
        public PartialInformationInputModel InputModel { get; set; }
        */

        public CHEDPersonelInputModel ReviewerInput { get; set; }
        public SenderInputModel SenderInput { get; set; }

       // public List<Office> Offices { get; set; }
      //  public List<Designation> Designations { get; set; }
        public string Code { get; set; }
        public bool isZeroUser { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
       
            var designations = await _desigRepo.GetAll();
            var chedpersonels = await _chedPRepo.GetAll();
            var senders = await _senderRepo.GetAll();

           // Offices = offices.ToList();
           // Designations = designations.ToList();

            var user = await _userManager.FindByIdAsync(userId);

            ReviewerInput = new CHEDPersonelInputModel
            {
                IdentityUserId = userId,
                Email = user.Email,
                Username = user.UserName,
                TypeOfUser = TypeOfUser.Admin,
                Active = true
            };

            SenderInput = new SenderInputModel
            {
                IdentityUserId = userId,
                Email = user.Email,
                Username = user.UserName,
                TypeOfUser = TypeOfUser.Sender,
                Active = true
            };

            isZeroUser = chedpersonels.Count()<= 0 && senders.Count() <= 0;


            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

           
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var reviewer = chedpersonels.FirstOrDefault(x => x.IdentityUserId == userId);
            var sender = senders.FirstOrDefault(x => x.IdentityUserId == userId);

            if(reviewer!=null || sender != null)
                return RedirectToPage("./Login");

            var settings = await _settingsRepo.GetAll();
            LogoFileName = settings.OrderByDescending(x => x.Id).First().LogoFileName;
            Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            return Page();
        }


		/*
        public async Task<IActionResult> OnPostAdmin(CHEDPersonelInputModel inp,string code)
        {
            ReviewerInput = inp;
            
            //if(!TryValidateModel(ReviewerInput))
              //  return 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
			var converted = _mapper.Map<CHEDPersonel>(InputModel);
			await _chedPRepo.Add(converted);
			
            var user = await _userManager.FindByIdAsync(InputModel.IdentityUserId);
            user.FirstName = InputModel.FirstName;
            user.LastName = InputModel.LastName;
            user.MiddleName = InputModel.MiddleName;
            user.Suffixes = InputModel.Suffixes;
            user.Active = true;
            await _accountRepo.Update(user, user.Id);
            await _userManager.ConfirmEmailAsync(user, code);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToPage();
        }
        */


		public async Task<IActionResult> OnPostAdmin(CHEDPersonelInputModel ReviewerInput, string code)
		{
			this.ReviewerInput = ReviewerInput;

			if (!TryValidateModel(ReviewerInput))
				return BadRequest(ModelState);


			var converted = _mapper.Map<CHEDPersonel>(ReviewerInput);
			await _chedPRepo.Add(converted);

			var user = await _userManager.FindByIdAsync(ReviewerInput.IdentityUserId);
			user.Active = ReviewerInput.Active;
			user.Email = ReviewerInput.Email;
			user.UserName = ReviewerInput.Username;
			user.TypeOfUser = ReviewerInput.TypeOfUser;
			user.FirstName = ReviewerInput.FirstName;
			user.LastName = ReviewerInput.LastName;
			user.MiddleName = ReviewerInput.MiddleName;
			user.Suffixes = ReviewerInput.Suffixes;
			user.Address = ReviewerInput.Address;
			user.Sex = ReviewerInput.Sex;
			user.MaritalStatus = ReviewerInput.MaritalStatus;
			user.PhoneNumber = ReviewerInput.PhoneNumber;
            user.DateOfBirth = ReviewerInput.DateOfBirth;
			user.ProfileFileName = await _fileUploader.Uploadfile(ReviewerInput.ProfileFile, "ProfilePicture");
            

			await _accountRepo.Update(user, user.Id);
			await _userManager.ConfirmEmailAsync(user, code);
			await _signInManager.SignInAsync(user, isPersistent: false);


			TempData["validation-message"] = "Successfully registered";

			return RedirectToPage("/Application/Dashboard/Index");
		}

		public async Task<IActionResult> OnPostSender(SenderInputModel SenderInput, string code)
		{
			this.SenderInput = SenderInput;

            if (!TryValidateModel(SenderInput))
                return BadRequest(ModelState); 
			
			var converted = _mapper.Map<Sender>(SenderInput);

			await _senderRepo.Add(converted);


			var user = await _userManager.FindByIdAsync(SenderInput.IdentityUserId);
			user.Active = SenderInput.Active;
			user.Email = SenderInput.Email;
			user.UserName = SenderInput.Username;
            user.TypeOfUser = SenderInput.TypeOfUser;   
			user.FirstName = SenderInput.FirstName;
			user.LastName = SenderInput.LastName;
			user.MiddleName = SenderInput.MiddleName;
			user.Suffixes = SenderInput.Suffixes;
            user.Address = SenderInput.Address;
            user.Sex = SenderInput.Sex;
            user.MaritalStatus = SenderInput.MaritalStatus;
			user.PhoneNumber = SenderInput.PhoneNumber;
            user.DateOfBirth = SenderInput.DateOfBirth; 
            user.ProfileFileName = await _fileUploader.Uploadfile(SenderInput.ProfileFile, "ProfilePicture");

            var settings = await _settingsRepo.GetAll();
            var requiredNotif = settings.OrderByDescending(x => x.Id).First().RegisteredUserNotif;
            if (requiredNotif)
            {
				var admins = await _userManager.GetUsersInRoleAsync("Admin");
				foreach (var admin in admins)
				{
					var notification = new Notification
					{
						Title = "Registration",
						Recepient = admin.Id,
						IsViewed = false,
						Description = "New sender has been registered",
						NotificationType = NotificationType.Registration,
						RedirectLink = "/Application/Usermanagement/SenderManagement/Index",
						AddedAt = DateTime.UtcNow.AddHours(8),
						UpdatedAt = DateTime.UtcNow.AddHours(8),
					};

					await _notificationRepo.Add(notification);
					_notifHub.Clients.User(admin.Id).ReceiveNotification(notification.Title, notification.Description.Length > 30 ? $"{notification.Description.Substring(0, 30)}..." : notification.Description, notification.NotificationType.ToString(), notification.AddedAt.ToString("MMMM dd, yyy"), notification.RedirectLink);
				}
			}
			await _accountRepo.Update(user, user.Id);
			await _userManager.ConfirmEmailAsync(user, code);
			await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["validation-message"] = "Successfully registered";
			return RedirectToPage("/Application/Dashboard/Index");
		}
	}
}
