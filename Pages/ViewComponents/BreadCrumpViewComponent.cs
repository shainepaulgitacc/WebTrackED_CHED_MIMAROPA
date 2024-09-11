using Microsoft.AspNetCore.Mvc;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ViewComponentModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.ViewComponents
{
    public class BreadCrumpViewComponent: ViewComponent
    {
        public BreadCrumpViewComponent(){}
        public async Task<IViewComponentResult> InvokeAsync(BreadCrumpViewModel model)
        {
            return View(model);
        }
    }
}
