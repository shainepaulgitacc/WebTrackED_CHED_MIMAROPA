using System.ComponentModel.DataAnnotations.Schema;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

public class Message : BaseEntity
{
    // Foreign key for SenderUser
    [ForeignKey("SenderUser")]
    public string Sender { get; set; }
	public string Recipient { get; set; }
	public string MessageContent { get; set; }
    public bool IsViewed { get; set; }

    public bool IsDeleted { get; set; }
    public virtual AppIdentityUser SenderUser { get; set; }
}
