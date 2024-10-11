using Microsoft.AspNetCore.Mvc;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.RequestModel
{
    public class FilteringRequestModel
    {
        [FromQuery(Name = "s")]
        public int? ServiceId { get; set; }
        [FromQuery(Name = "f")]
        public DateTime? From { get; set; }
        [FromQuery(Name = "t")]
        public DateTime? To { get; set; }

    }
}
