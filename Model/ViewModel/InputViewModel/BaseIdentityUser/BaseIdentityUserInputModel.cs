using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel.BaseIdentityUser
{
    public class BaseIdentityUserInputModel
    {
        [DisplayName("Type Of User")]
        public TypeOfUser TypeOfUser { get; set; }
		public DateTime AddedAt { get; set; } = DateTime.Now;
		public DateTime UpdatedAt { get; set; } = DateTime.Now;

		public bool Active { get; set; } = true;

        public string? IdentityUserId { get; set; }

        [Required, DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Middle Name")]
        public string? MiddleName { get; set; }
        public string? Suffixes { get; set; }

        [DisplayName("Last Name")]
        public string? LastName { get; set; }


        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        #region
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

		#endregion
		[Display(Name = "Profile Picture")]
        public IFormFile? ProfileFile { get; set; }
		[Required]
		public string Address { get; set; }
        public Sex? Sex{ get; set; }
        [DisplayName("Date Of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [DisplayName("Marital Status")]
        public MaritalStatus? MaritalStatus { get; set; }
        [StringLength(100,MinimumLength = 11),DisplayName("Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}
