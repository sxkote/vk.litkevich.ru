using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LitskevichVK.Services
{
    public enum ImagerResizeType
    {
        /// <summary>
        /// Не изменять размеры изображения
        /// </summary>
        None,


        /// <summary>
        /// Новое изображение должно быть "вписано" в указанный размер без искажения с заполнением лишнего пространства цветом
        /// (скорее всего: оно будет менее указанного размера, а лишнее пространство будет заполнено цветом)
        /// </summary>
        Fit,


        /// <summary>
        /// Новое изображение должно "входить" без искажения в указанный размер, с текущими пропорции 
        /// (скорее всего: оно будет менее указанного размера, а пропорции будут сохранены)
        /// </summary>
        Scale,


        /// <summary>
        /// Новое изображение должно быть "обрезано" без искажения в рамках указанного размера
        /// (скорее всего: оно будет обрезано под размер)
        /// </summary>
        Crop,


        /// <summary>
        /// Новое изображение должно быть "растянуто" в рамки указанного размер
        /// (скорее всего: оно будет растянуто или сжато)
        /// </summary>
        Fill
    };

    public class ImagerResizeParams
    {
        #region Parametres
        public ImagerResizeType Type { get; private set; }

        public Size Size { get; private set; }
        public Color Background { get; private set; }

        public int Width { get { return ((this.Size == null) ? 0 : this.Size.Width); } }
        public int Height { get { return ((this.Size == null) ? 0 : this.Size.Height); } }
        #endregion

        #region Constructors
        public ImagerResizeParams()
        {
            this.Type = ImagerResizeType.None;
            this.Size = new Size(0, 0);
            this.Background = Color.Empty;
        }

        public ImagerResizeParams(ImagerResizeType type, Size size, Color background)
            : this()
        {
            this.Type = type;
            this.Size = size;
            this.Background = background;
        }

        public ImagerResizeParams(string type, Size size, Color background)
            : this(GetType(type), size, background) { }

        public ImagerResizeParams(string type, Size size, string background)
            : this(GetType(type), size, GetColor(background)) { }
        #endregion

        #region Statics
        static public ImagerResizeType GetType(string type)
        {
            string val = type.Trim().ToLower();

            if (val == "fit")
                return ImagerResizeType.Fit;
            if (val == "scale")
                return ImagerResizeType.Scale;
            if (val == "fill")
                return ImagerResizeType.Fill;
            if (val == "crop")
                return ImagerResizeType.Crop;

            return ImagerResizeType.None;
        }

        static public Color GetColor(string color)
        {
            if (String.IsNullOrEmpty(color) || color.Trim().ToLower() == "empty")
                return Color.Empty;
            return Color.FromName(color);
        }
        #endregion
    }

    public class ImagerService : IDisposable
    {
        #region Variables
        protected Image _image = null;
        protected PropertyItem[] _properties = null;
        #endregion

        #region Properties
        public Image Image
        {
            get { return this._image; }
            private set { this._image = value; }
        }
        #endregion

        #region Constructors
        private ImagerService()
        {
        }
        #endregion

        #region Functions
        public ImagerService Modify(ImagerResizeParams param)
        {
            //размеры текущего изображения
            int image_width = this.Image.Width;
            int image_height = this.Image.Height;

            //определяем процент сжатия, чтобы уложиться в новый размер
            float percent_width = ((float)param.Size.Width / (float)image_width);
            float percent_height = ((float)param.Size.Height / (float)image_height);

            float percent = Math.Min(percent_height, percent_width);

            //если нужно обрезать, то изображение должно быть больше указанных размеров
            if (param.Type == ImagerResizeType.Crop)
                percent = Math.Max(percent_height, percent_width);

            //если менярь размеры не нужно, то процент === 1
            if (param.Type == ImagerResizeType.None)
                percent = 1.0f;

            //окончательные размеры сжатого изображения
            int redraw_width = (int)(image_width * percent);
            int redraw_height = (int)(image_height * percent);

            //если нужно заполнить указанный размер с растяжением изображения 
            if (param.Type == ImagerResizeType.Fill)
            {
                redraw_width = param.Size.Width;
                redraw_height = param.Size.Height;
            }

            int canvas_width = param.Size.Width;
            int canvas_height = param.Size.Height;

            //если мы просто меняем размеры изображения пропорционально 
            // или не меняем размер изображения совсем
            // - холст должен совпадать с размерами измененного изображения
            if (param.Type == ImagerResizeType.None || param.Type == ImagerResizeType.Scale)
            {
                canvas_width = redraw_width;
                canvas_height = redraw_height;
            }

            //результирующий холст - новое изображение в новых размерах
            Bitmap result = new Bitmap(canvas_width, canvas_height);

            //как-то не правильно получилось - задаю CROP в квадрат - а возвращает он мне прямоугольник!!!
            ////если цвет заполнения не задан, то незачем слушаться новых размеров - просто нужно отмасштабировать
            //if (param.Type == ImageResizeType.None || param.Background == Color.Empty)
            //    result = new Bitmap(redraw_width, redraw_height);

            using (Graphics graphics = Graphics.FromImage(result))
            {
                if (param.Background != Color.Empty)
                    using (Brush br = new SolidBrush(param.Background))
                        graphics.FillRectangle(br, 0, 0, result.Width, result.Height);

                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                graphics.DrawImage(this.Image, (result.Width / 2) - (redraw_width / 2), (result.Height / 2) - (redraw_height / 2), redraw_width, redraw_height);
            }

            this.Image = result;

            return this;
        }

        public ImagerService Modify(ImagerResizeType type, Size size)
        {
            var param = new ImagerResizeParams(type, size, Color.Empty);
            return this.Modify(param);
        }

        public ImagerService Modify(ImagerResizeType type, int maxSize)
        {
            var param = new ImagerResizeParams(type, new Size(maxSize, maxSize), Color.Empty);
            return this.Modify(param);
        }

        public ImagerService Resize(int max_size)
        {
            var param = new ImagerResizeParams(ImagerResizeType.Scale, new Size(max_size, max_size), Color.Empty);
            return this.Modify(param);
        }

        public byte[] Save(long quality = 100, string mime_type = "image/jpeg")
        {
            //string content_type = ((type == null || type.Trim() == "") ? "image/jpeg" : type);

            // Encoder parameter for image quality
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            // Jpeg image codec
            ImageCodecInfo jpegCodec = ImageEncoderInfo(mime_type);
            if (jpegCodec == null)
                return null;

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            {
                this.Image.Save(ms, jpegCodec, encoderParams);
                ms.Position = 0;
                data = ms.ToArray();
                ms.Close();
            }

            return data;
        }

        public byte[] Save(ImageFormat format)
        {
            byte[] data = null;
            using (MemoryStream ms = new MemoryStream())
            {
                this.Image.Save(ms, format);
                ms.Position = 0;
                data = ms.ToArray();
                ms.Close();
            }

            return data;
        }

        public ImagerService Load(byte[] data)
        {
            this.Image = null;

            if (data != null)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    ms.Position = 0;
                    this.Image = Image.FromStream(ms);
                    this._properties = this.Image.PropertyItems;
                    ms.Close();
                }
            }

            return this;
        }

        public PropertyItem GetProperty(int id)
        {
            try
            {
                if (this.Image == null)
                    return null;

                PropertyItem property = null;

                if (this.Image.PropertyItems != null && this.Image.PropertyItems.Length > 0)
                    property = this.Image.GetPropertyItem(id);
                else if (this._properties != null && this._properties.Length > 0)
                    property = this._properties.FirstOrDefault(p => p.Id == id);

                return property;
            }
            catch { return null; }
        }

        public string GetProperty(int id, Encoding encoding)
        {
            try
            {
                var property = this.GetProperty(id);
                if (property == null || property.Value == null || property.Len <= 0)
                    return "";

                return encoding.GetString(property.Value, 0, property.Len - ((encoding.IsSingleByte) ? 1 : 2));
            }
            catch { return ""; }
        }

        public string GetTitle()
        {
            try
            {
                return ((this.GetProperty(270, Encoding.UTF8) == "")
                               ? this.GetProperty(40091, Encoding.Unicode)
                               : this.GetProperty(270, Encoding.UTF8));
            }
            catch { return ""; }
        }

        public string GetComment()
        {
            try
            {
                return this.GetProperty(40092, Encoding.Unicode);
            }
            catch { return ""; }
        }


        //public Image RoundCorners(int radius, Color background_color)
        //{
        //    radius *= 2;
        //    Bitmap result = new Bitmap(this.Image.Width, this.Image.Height);

        //    using (Graphics g = Graphics.FromImage(result))
        //    {
        //        g.Clear(background_color);
        //        g.SmoothingMode = SmoothingMode.AntiAlias;

        //        using (Brush brush = new TextureBrush(this.Image))
        //        using (GraphicsPath gp = new GraphicsPath())
        //        {
        //            gp.AddArc(0, 0, radius, radius, 180, 90);
        //            gp.AddArc(0 + result.Width - radius, 0, radius, radius, 270, 90);
        //            gp.AddArc(0 + result.Width - radius, 0 + result.Height - radius, radius, radius, 0, 90);
        //            gp.AddArc(0, 0 + result.Height - radius, radius, radius, 90, 90);
        //            g.FillPath(brush, gp);

        //            this.Image = result;
        //        }
        //    }

        //    return this.Image;
        //}

        public ImagerService Pad(Size size, Color background)
        {
            var param = new ImagerResizeParams(ImagerResizeType.Fit, size, background);

            return this.Modify(param);
        }


        public ImagerService Pad(Color background)
        {
            int max_size = Math.Max(this.Image.Height, this.Image.Width);

            return this.Pad(new Size(max_size, max_size), background);
        }

        public ImagerService Rotate(RotateFlipType type)
        {
            this.Image.RotateFlip(type);

            return this;
        }

        public ImagerService RotateCorrection()
        {
            var property = this.GetProperty(0x112);
            if (property == null || property.Value == null || property.Value.Length <= 0)
                return this;

            RotateFlipType rotate = RotateFlipType.RotateNoneFlipNone;
            if (property.Value[0] == 6)
                rotate = RotateFlipType.Rotate90FlipNone;
            if (property.Value[0] == 3)
                rotate = RotateFlipType.Rotate180FlipNone;
            if (property.Value[0] == 8)
                rotate = RotateFlipType.Rotate270FlipNone;

            this.Rotate(rotate);

            property.Value = new byte[] { 0x1 };

            this.Image.SetPropertyItem(property);

            return this;
        }
        #endregion

        #region Statics
        static public ImagerService Create(Image img)
        {
            var result = new ImagerService();
            result.Image = img;
            return result;
        }

        static public ImagerService Create(byte[] data)
        {
            var result = new ImagerService();
            result.Load(data);
            return result;
        }

        static public ImagerService Create(string filename)
        {
            var result = new ImagerService();
            result.Image = Image.FromFile(filename);
            return result;
        }

        static public ImageCodecInfo ImageEncoderInfo(string mime_type)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType.Trim().ToLower() == mime_type.Trim().ToLower())
                    return codecs[i];

            return null;
        }

        static public byte[] ResizeImage(byte[] image, int maxSize, int quality = 90, string mime_type = "image/png")
        {
            if (image == null || image.Length <= 0)
                return null;

            return ImagerService.Create(image).Resize(maxSize).Save(quality, mime_type);
        }
        #endregion

        public void Dispose()
        {
            _image = null;
            _properties = null;
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
