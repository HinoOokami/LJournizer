using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            txtBoxDim.TextChanged += TxtBoxDim_TextChanged;
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
            controller.StartConvert();
        }

        void TxtBoxDim_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1)) e.Handled = true;
        }

        void TxtBoxDim_TextChanged(object sender, TextChangedEventArgs e)
        {
            controller.CheckTextBox();
        }
    }
}
