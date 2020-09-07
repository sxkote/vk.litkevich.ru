using System.Collections.Generic;
using LitskevichVK.Contracts;
using LitskevichVK.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LitskevichVK.Pages
{
    public class AboutModel : PageModel
    {
        private readonly IDataProvider _dataProvider;

        public IEnumerable<ImageModel> Photos { get; set; }

        public AboutModel(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public void OnGet()
        {
            Photos = _dataProvider.GetPhotos();
        }
    }
}
