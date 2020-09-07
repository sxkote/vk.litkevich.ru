using System.Collections.Generic;
using System.Linq;
using LitskevichVK.Contracts;
using LitskevichVK.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace LitskevichVK.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDataProvider _dataProvider;

        public List<GalleryModel> GalleryCollection { get; set; }
        public Dictionary<string, ImageModel> GalleryIcons { get; set; } = new Dictionary<string, ImageModel>();

        public IndexModel(ILogger<IndexModel> logger, IDataProvider dataProvider)
        {
            _logger = logger;
            _dataProvider = dataProvider;
        }

        public void OnGet()
        {
            GalleryCollection = _dataProvider.GetGalleryCollection().ToList();
            foreach (var gallery in GalleryCollection)
            {
                var icon = _dataProvider.GetGalleryIcon(gallery.Name);
                GalleryIcons.Add(gallery.Name, icon);
            }
        }
    }
}
