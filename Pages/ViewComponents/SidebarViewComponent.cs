using Microsoft.AspNetCore.Mvc;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ViewComponentModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.ViewComponents
{
    public class SidebarViewComponent:ViewComponent
    {
        public async  Task<IViewComponentResult> InvokeAsync(SidebarViewModel model)
        {
            return View(model);
        }
    }
}
