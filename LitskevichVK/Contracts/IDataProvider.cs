using LitskevichVK.Models;
using System.Collections.Generic;

namespace LitskevichVK.Contracts
{
    public interface IDataProvider
    {
        /// <summary>
        /// Take all photo
        /// </summary>
        /// <returns>Collection of Image Models</returns>
        public IEnumerable<ImageModel> GetPhotos();

        /// <summary>
        /// Take all Gallery Models from .json file
        /// </summary>
        /// <returns>collection of Gallery Models</returns>
        IEnumerable<GalleryModel> GetGalleryCollection();

        /// <summary>
        /// Take specific Gallery model from .json file
        /// </summary>
        /// <param name="name">name of the gallery to take</param>
        /// <returns>Gallery Model</returns>
        GalleryModel GetGallery(string name);

        /// <summary>
        /// Take all images from Gallery's folder
        /// </summary>
        /// <param name="name">name of the Gallery to read images from</param>
        /// <returns>collection of Images Model of the specific Gallery</returns>
        IEnumerable<ImageModel> GetGalleryImages(string name);

        /// <summary>
        /// Take Icon (Image Model) for specific Gallery
        /// </summary>
        /// <param name="name">name of the Gallery</param>
        /// <returns>Image Model - Icon of the specified Gallery</returns>
        ImageModel GetGalleryIcon(string name);
    }
}
