using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LJournizer
{
    public static class ImageOps
    {
        static (int newWidth, int newHeight) CalculateSize(float imageW, float imageH, int restriction)
        {
            int nW, nH;
            if (imageW > imageH)
            {
                nW = restriction;
                nH = (int) Math.Round(imageH * restriction / imageW);
            }
            else if (imageW < imageH)
            {
                nW = (int) Math.Round(imageW * restriction / imageH);
                nH = restriction;
            }
            else
            {
                nW = restriction;
                nH = restriction;
            }

            return (newWidth: nW, newHeight: nH);
        }

        internal static void ModifyImage(string path, int restriction, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();
            using (Stream stream = File.OpenRead(path))
            {
                using (Image image = Image.FromStream(stream, true, true))
                {
                    Image rotatedImage = RotateImage(image);
                    (int newWidth, int newHeight) newSize = CalculateSize(rotatedImage.Width, rotatedImage.Height, restriction);
                    ToJPG(Resize(rotatedImage, newSize), path, restriction);
                }
            }
        }

        static Bitmap Resize(Image image, (int newWidth, int newHeight) newSize)
        {
            var destRect = new Rectangle(0, 0, newSize.newWidth, newSize.newHeight);
            var destImage = new Bitmap(newSize.newWidth, newSize.newHeight);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        static void ToJPG(Image image, string path, int restriction)
        {
            var encoderParameters = new EncoderParameters(1)
                                    {
                                        Param = {[0] = new EncoderParameter(Encoder.Quality, 90L)}
                                    };

            string newPath = ChangeFileName(path, restriction);
            image.Save(newPath, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        static string ChangeFileName(string path, int restriction)
        {
            FileInfo fi = new FileInfo(path);
            DirectoryInfo di = fi.Directory;
            string newDir = di.CreateSubdirectory("Resized_" + restriction).FullName;
            string file = Path.GetFileNameWithoutExtension(path);
            string newPath = newDir + @"\" + file + "_" + restriction + ".jpg";

            return newPath;
        }

        static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        static Image RotateImage(Image image)
        {
            try
            {
                foreach (var prop in image.PropertyItems)
                {
                    if (prop.Id == 0x0112) //value of EXIF
                    {
                        int orientationValue =  image.GetPropertyItem(prop.Id).Value[0];
                        RotateFlipType rotateFlipType = GetOrientationToFlipType(orientationValue);
                        image.RotateFlip(rotateFlipType);
                        image.RemovePropertyItem(0x0112);
                        return image;
                    }
                }
            }
            catch (Exception ex){}

            return image;
        }

        static RotateFlipType GetOrientationToFlipType(int orientationValue)
        {
            RotateFlipType rotateFlipType;

            switch (orientationValue)
            {
                case 1:
                    rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                    break;
                case 2:
                    rotateFlipType = RotateFlipType.RotateNoneFlipX;
                    break;
                case 3:
                    rotateFlipType = RotateFlipType.Rotate180FlipNone;
                    break;
                case 4:
                    rotateFlipType = RotateFlipType.Rotate180FlipX;
                    break;
                case 5:
                    rotateFlipType = RotateFlipType.Rotate90FlipX;
                    break;
                case 6:
                    rotateFlipType = RotateFlipType.Rotate90FlipNone;
                    break;
                case 7:
                    rotateFlipType = RotateFlipType.Rotate270FlipX;
                    break;
                case 8:
                    rotateFlipType = RotateFlipType.Rotate270FlipNone;
                    break;
                default:
                    rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                    break;
            }

            return rotateFlipType;
        }
    }
}