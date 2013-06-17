#region

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using BetterImageProcessorQuantization;

#endregion

namespace ED47.BusinessAccessLayer.Tools
{
    /// <summary>
    /// Class responsable for create image thumbnails 
    /// </summary>
    public static class ImageUtilities
    {
        public static Size GetSize(FileInfo file)
        {
            if (!file.Exists) return new Size(0,0);
            var img = System.Drawing.Image.FromFile(file.FullName);
            return img.Size;
        }

        public static void Crop(Stream imageStream, Rectangle rectangle, string filename)
        {
            var originalimg = System.Drawing.Image.FromStream(imageStream);
            double w = originalimg.Width;
            double h = originalimg.Height;
            using (System.Drawing.Image thumbnail = new Bitmap(Convert.ToInt32(rectangle.Width), Convert.ToInt32(rectangle.Height)))
            {
                var objGraphics = Graphics.FromImage(thumbnail);
                objGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                objGraphics.SmoothingMode = SmoothingMode.HighQuality;
                objGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                objGraphics.CompositingQuality = CompositingQuality.HighQuality;
                objGraphics.DrawImage(originalimg, new Rectangle(0, 0, rectangle.Width, rectangle.Height), rectangle, GraphicsUnit.Pixel);
                thumbnail.Save(filename, ImageFormat.Jpeg);
            }

        }

        public static void CutImage(string fileName, int tilecount, DirectoryInfo target)
        {
            var originalimg = System.Drawing.Image.FromFile(fileName);
            double w = originalimg.Width;
            double h = originalimg.Height;

            var sizex =Convert.ToInt32(Math.Floor(w / tilecount));
            var sizey =Convert.ToInt32( Math.Floor(h / tilecount));

            for (int i = 0; i < tilecount; i++)
            {
                for (int j = 0; j < tilecount; j++)
                {

                    using (System.Drawing.Image thumbnail = new Bitmap(Convert.ToInt32(sizex), Convert.ToInt32(sizey)))
                    {
                        //new Bitmap(width_, height_);
                        var objGraphics = Graphics.FromImage(thumbnail);
                        objGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        objGraphics.SmoothingMode = SmoothingMode.HighQuality;
                        objGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        objGraphics.CompositingQuality = CompositingQuality.HighQuality;
                        objGraphics.DrawImage(originalimg, new Rectangle(0, 0, sizex, sizey),new Rectangle(i*sizex,j*sizey,sizex,sizey),GraphicsUnit.Pixel);

                        var dir = new DirectoryInfo(target.FullName + "\\" + i );
                        if(!dir.Exists) dir.Create();
                    
                        thumbnail.Save(dir.FullName + "\\" + j + ".png", ImageFormat.Png);
                    }
                }
            }


        }

        public static void Resize(string fileName, Stream inputStream, Stream outputStream, double maxW, double maxH)
        {
            using (var originalimg = System.Drawing.Image.FromStream(inputStream))
            {

                double w = originalimg.Width;
                double h = originalimg.Height;
                //Redimensionne la largeur
                if (w > maxW)
                {
                    h = (Math.Ceiling(h*(maxW/w)));
                    w = maxW;
                }

                //Redimensionne la hauteur
                if (h > maxH)
                {
                    w = (Math.Ceiling(w*(maxH/h)));
                    h = maxH;
                }

                using (Image thumbnail = new Bitmap(Convert.ToInt32(w), Convert.ToInt32(h)))
                {
                    //new Bitmap(width_, height_);
                    using (var objGraphics = Graphics.FromImage(thumbnail))
                    {
                        objGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        objGraphics.SmoothingMode = SmoothingMode.HighQuality;
                        objGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        objGraphics.CompositingQuality = CompositingQuality.HighQuality;
                        objGraphics.DrawImage(originalimg, 0, 0, Convert.ToInt32(w), Convert.ToInt32(h));
                    }
                    var extension = Path.GetExtension(fileName).ToLower();

                    switch (extension)
                    {
                        case ".gif":
                            using (thumbnail)
                            {
                                var quantizer = new OctreeQuantizer(255, 8);
                                using (var quantized = quantizer.Quantize(thumbnail))
                                {
                                    quantized.Save(outputStream, ImageFormat.Gif);
                                }
                            }
                            break;
                        case ".jpeg":
                        case ".jpg":
                            {
                                var encoderParameters = new EncoderParameters(1);
                                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                                thumbnail.Save(outputStream, ImageFormat.Jpeg);
                            }
                            break;
                        case ".png":
                            {
                                var encoderParameters = new EncoderParameters(1);
                                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                                thumbnail.Save(outputStream, ImageFormat.Png);
                            }
                            break;

                    }


                }

                originalimg.Dispose();
            }

        }

        public static void Resize(string fileName, string newFileName, double maxW, double maxH)
        {
            var fi = new FileInfo(fileName);
            if (!fi.Exists) return;

            using (var input = fi.OpenRead())
            {
                var f2 = new FileInfo(newFileName);
                if(f2.Exists) f2.Delete();
                using (var output = f2.OpenWrite())
                {
                    Resize(fileName, input, output, maxW, maxH);
                    output.Close();
                }
            }

        }


        private static bool ThumbnailCallback()
        {
            return false;
        }
    }
}