using LitskevichVK.Contracts;
using LitskevichVK.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LitskevichVK.Services
{
    public class DataProvider : IDataProvider
    {
        public const string ENVIROMENT_VARIABLE_NAME = "DATA_ROOT";
        private const string DATA_FILENAME = "data.json";
        public const string PHOTO_FOLDER = "~/data/photo";


        private readonly IWebHostEnvironment _environment;
        private readonly string _root;

        private IEnumerable<GalleryModel> _galleryCollection = null;

        public DataProvider(IWebHostEnvironment environment)
        {
            _environment = environment;
            _root = Environment.GetEnvironmentVariable(ENVIROMENT_VARIABLE_NAME);
        }

        public IEnumerable<ImageModel> GetPhotos()
        {
            var folder = PHOTO_FOLDER;
            var directory = folder.Replace("~", _environment.WebRootPath);

            if (!Directory.Exists(directory))
                return new List<ImageModel>();

            return new DirectoryInfo(directory).GetFiles()
                .Where(f => f.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) || f.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase))
                .Select(f =>
                {
                    using (var imager = ImagerService.Create(f.FullName))
                        return new ImageModel(folder.Replace("~", ""), f.Name, imager.GetTitle(), imager.GetComment());
                })
                .ToList();
        }

        public IEnumerable<GalleryModel> GetGalleryCollection()
        {
            if (_galleryCollection != null)
                return _galleryCollection;

            var filename = $"{_root}/{DATA_FILENAME}".Replace("~", _environment.WebRootPath);

            var jsonSettings = new JsonSerializerSettings()
            {
                //ContractResolver = new DefaultContractResolver()
                //{
                //    NamingStrategy = new CamelCaseNamingStrategy()
                //}
            };

            _galleryCollection = JsonConvert.DeserializeObject<IEnumerable<GalleryModel>>(File.ReadAllText(filename), jsonSettings);

            return _galleryCollection;
        }

        public GalleryModel GetGallery(string name)
        {
            return this.GetGalleryCollection()
                .FirstOrDefault(g => g.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<ImageModel> GetGalleryImages(string name)
        {
            var gallery = GetGallery(name);
            if (gallery == null)
                throw new KeyNotFoundException("Gallery not found!");

            var directory = $"{_root}{gallery.Folder}";
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException("Gallery Folder not found!");

            return new DirectoryInfo(directory).GetFiles()
                 .Where(f => f.Extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) || f.Extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase))
                 .Select(f =>
                 {
                     return new ImageModel($"/root/{gallery.Folder}", f.Name);
                     //using (var imager = ImagerService.Create(f.FullName))
                     //    return new ImageModel(directory, f.Name, imager.GetTitle(), imager.GetComment());
                 })
                 .ToList();
        }

        public ImageModel GetGalleryIcon(string name)
        {
            var gallery = GetGallery(name);
            if (gallery == null)
                throw new KeyNotFoundException("Gallery not found!");

            var path = $"{_root}{gallery.Folder}/{gallery.Icon}";
            if (!File.Exists(path))
                throw new DirectoryNotFoundException("Gallery Icon file not found!");

            return new ImageModel($"/root/{gallery.Folder}", gallery.Icon);
        }
    }
}
