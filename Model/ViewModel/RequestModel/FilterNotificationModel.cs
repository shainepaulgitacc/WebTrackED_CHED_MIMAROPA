using Microsoft.AspNetCore.Mvc;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.RequestModel
{
	public class FilterNotificationModel
	{
		[FromQuery(Name = "From")]
		public DateTime? From { get; set; }
		[FromQuery(Name = "To")]
		public DateTime? To { get; set; }
	}
}
