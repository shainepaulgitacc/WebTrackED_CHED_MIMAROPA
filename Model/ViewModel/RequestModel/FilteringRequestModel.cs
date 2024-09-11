using Microsoft.AspNetCore.Mvc;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.RequestModel
{
    public class FilteringRequestModel
    {
        [FromQuery(Name = "sC")]
        public int? SubCategory { get; set; }
        [FromQuery(Name = "p")]
        public Prioritization? Prioritization { get; set; }
        [FromQuery(Name = "s")]
        public Status? Status { get; set; }

    }
}
