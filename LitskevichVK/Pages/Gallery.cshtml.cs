using System;
using System.Collections.Generic;
using System.Linq;
using LitskevichVK.Contracts;
using LitskevichVK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LitskevichVK.Pages
{
    public class GalleryPageModel : PageModel
    {
        private readonly IDataProvider _dataProvider;

        public GalleryModel Gallery { get; set; }

        public IEnumerable<ImageModel> Images { get; set; }

        public GalleryPageModel(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public IActionResult OnGet(string name)
        {
            Gallery = _dataProvider.GetGalleryCollection()
                .FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (Gallery == null)
                return RedirectToPage("Index");

            Images = _dataProvider.GetGalleryImages(name);

            return Page();
        }
    }
}
