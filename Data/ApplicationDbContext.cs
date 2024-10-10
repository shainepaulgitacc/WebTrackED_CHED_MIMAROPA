using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppIdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<SubCategory> SubCategories { get; set; }
        public virtual DbSet<Designation> Designations { get; set; }
      
        public virtual DbSet<CHEDPersonel> CHEDPersonels { get; set; }
        public virtual DbSet<DocumentAttachment> DocumentAttachments { get; set; }
        public virtual DbSet<Sender> Senders { get; set; }
        public virtual DbSet<DocumentTracking> DocumentTrackings { get; set; }

      
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
    }
}
