using LitskevichVK.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LitskevichVK.Pages.Components.Navbar
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly IDataProvider _dataProvider;

        public NavbarViewComponent(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public IViewComponentResult Invoke()
        {
            return View("_Navbar", _dataProvider.GetGalleryCollection());
        }
    }
}
