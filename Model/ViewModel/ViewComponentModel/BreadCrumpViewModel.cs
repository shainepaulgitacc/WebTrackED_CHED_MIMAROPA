namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.ViewComponentModel
{
    public class BreadCrumpViewModel
    {
        public string pageName { get; set; }

        //icon,previous page,page name, is this current page,the page has an id if none then null
        public (string, string?, string,bool,int?)[]? breadCrump { get; set; }

        public DateTime date { get; set; }
    }
}
