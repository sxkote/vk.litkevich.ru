using System;

namespace LitskevichVK.Models
{
    public class ImageModel
    {
        public string Folder { get; protected set; }
        public string Filename { get; protected set; }
        public string Title { get; protected set; }
        public string Comment { get; protected set; }

        public virtual string Url => String.Format($"{Folder}/{Filename}");

        public ImageModel() { }

        public ImageModel(string folder, string filename, string title = "", string comment = "")
        {
            this.Folder = folder;
            this.Filename = filename;
            this.Title = title;
            this.Comment = comment;
        }
    }
}
