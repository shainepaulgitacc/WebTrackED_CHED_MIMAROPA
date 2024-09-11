using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel.BaseIdentityUser
{
    public class SenderInputModel: BaseIdentityUserInputModel
    {
		public int? SenderId { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
		[DisplayName("Date Of Hire")]
		public DateTime? DateOfHire { get; set; }
		[DisplayName("Employment Status")]
		public EmploymentStatus? EmploymentStatus { get; set; }
		[DisplayName("Work Location Office")]

		public string? WorkLocationOffice { get; set; }
		[DisplayName("Project Assignment")]
		public string? ProjectAssignment { get; set; }
		[DisplayName("Skill Competencies")]
		public string? SkillCompetencies { get; set; }
		[DisplayName("Achievements Awards")]
		public string? AchievementsAwards { get; set; }
    }
}
