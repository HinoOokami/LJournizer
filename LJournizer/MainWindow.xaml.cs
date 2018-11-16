using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace LJournizer
{
    public partial class MainWindow : Window
    {
        internal static MainWindow main;

        Controller controller;

        public MainWindow()
        {
            InitializeComponent();
            main = this;
            controller = new Controller(main);
            btnBrowse.IsEnabled = true;
            btnStart.IsEnabled = false;
            btnCancel.IsEnabled = false;
        }

        async void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            await controller.FileBrowseAsync();
        }

        async void Grid_Drop(object sender, DragEventArgs e)
        {
            await controller.FilesDropAsync(e);
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            controller.Reset();
        }

        void btnStart_Click(object sender, RoutedEventArgs e)
        {
            controller.StartConvert(900);
        }

        //static Image RotateImage(Image image)
        //{
        //    try
        //    {
        //        foreach (var prop in image.PropertyItems)
        //        {
        //            if (prop.Id == 0x0112)
        //            {
        //                int orientationValue = image.GetPropertyItem(prop.Id).Value[0];
        //                RotateFlipType rotateFlipType = GetOrientationToFlipType(orientationValue);
        //                image.RotateFlip(rotateFlipType);
        //                image.RemovePropertyItem(0x0112);
        //                return image;
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {

        //    }

        //    return image;
        //}

        //static RotateFlipType GetOrientationToFlipType(int orientationValue)
        //{
        //    RotateFlipType rotateFlipType;

        //    switch (orientationValue)
        //    {
        //        case 1:
        //            rotateFlipType = RotateFlipType.RotateNoneFlipNone;
        //            break;
        //        case 2:
        //            rotateFlipType = RotateFlipType.RotateNoneFlipX;
        //            break;
        //        case 3:
        //            rotateFlipType = RotateFlipType.Rotate180FlipNone;
        //            break;
        //        case 4:
        //            rotateFlipType = RotateFlipType.Rotate180FlipX;
        //            break;
        //        case 5:
        //            rotateFlipType = RotateFlipType.Rotate90FlipX;
        //            break;
        //        case 6:
        //            rotateFlipType = RotateFlipType.Rotate90FlipNone;
        //            break;
        //        case 7:
        //            rotateFlipType = RotateFlipType.Rotate270FlipX;
        //            break;
        //        case 8:
        //            rotateFlipType = RotateFlipType.Rotate270FlipNone;
        //            break;
        //        default:
        //            rotateFlipType = RotateFlipType.RotateNoneFlipNone;
        //            break;
        //    }

        //    return rotateFlipType;
        //}
    }
}
