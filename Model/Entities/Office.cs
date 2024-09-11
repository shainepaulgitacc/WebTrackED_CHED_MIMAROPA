namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class Office:BaseEntity
    {
        public string OfficeName { get; set; }
        public virtual ICollection<CHEDPersonel> Reviewers { get; set; }
    }
}
