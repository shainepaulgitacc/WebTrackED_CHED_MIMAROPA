namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.ViewComponentModel
{
    public class SidebarViewModel
    {
        public string menuName { get; set; }
        public string icon { get; set; }

        public string? link { get; set; }
        public (string, string)[] submenus { get; set; }
    }
}
