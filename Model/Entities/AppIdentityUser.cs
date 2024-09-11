using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class AppIdentityUser:IdentityUser
    {
       
        public TypeOfUser TypeOfUser { get; set; }
        public bool Active { get; set; } = true;
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Suffixes { get; set; }
        public string? Address { get; set; }
        public Sex? Sex { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileFileName { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }

        public virtual Sender Sender { get; set; }
        public virtual CHEDPersonel CHEDPersonel { get; set; }

        public virtual ICollection<Message> Messages { get; set; } 

        public virtual ICollection<LoginHistory> LoginHistories { get; set; }
       
    }
    public enum TypeOfUser
    {
        Sender,
        Reviewer,
        Admin
    }
}
