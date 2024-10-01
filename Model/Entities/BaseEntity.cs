using System.ComponentModel.DataAnnotations.Schema;
namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public enum Sex
    {
        Male,
        Female
    }
    public enum Prioritization
    {
        Usual,
        Urgent
    }
    public enum ReviewerStatus
    {
        ToReceived,
        OnReview,
        Reviewed,
        PreparingRelease,
        Passed,
        Approved,
        Disapproved,
      

    }
    public enum Status
    {
        Pending,
        OnProcess,
        PreparingRelease,
        Approved,
        Disapproved
    }


    public enum MaritalStatus
    {
        Single,
        Married,
        Divorced,
        Widowed,
        Separated
    }

    public enum EmploymentStatus
    {
		Employed,
        Unemployed,
        NotInLaborForce

	}
    public enum DocumentType
    {
      
        WalkIn,
		OnlineSubmission
	}
    public enum NotificationType
    {
        Document,
        Registration,
    }

}
