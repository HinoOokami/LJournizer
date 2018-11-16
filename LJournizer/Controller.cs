using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Ookii.Dialogs.Wpf;

namespace LJournizer
{
    public class Controller
    {
        string path;
        DirectoryInfo di;
        string searchPattern;
        static string filesSelectedStr;
        static string filesProcessedStr;
        internal static MainWindow main;
        public static ObservableCollection<string> files;
        static int processedCounter;

        //bool hasDiffDirs;
        CancellationTokenSource cts;

        public Controller(MainWindow mainWindow)
        {
            processedCounter = 0;
            main = mainWindow;
            searchPattern = "*.jpg|*.jpeg|*.png|*.bmp|*.gif";
            filesSelectedStr = "Выбрано файлов: ";
            filesProcessedStr = "Обработано файлов: ";

            main.lblInfo.Content = "Выберите папку с изображениями или перетащите папки и/или файлы в окно программы";
            //hasDiffDirs = false;
            
            di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            path = di.FullName;

            files = new ObservableCollection<string>();
            LblCount(files.Count);
            files.CollectionChanged += FilesChanged;
        }

        void FilesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LblCount(files.Count);
            //CheckDiffDirs();
        }

        //void CheckDiffDirs()
        //{
        //    if (!hasDiffDirs && !files.Contains(path)) hasDiffDirs = true;
        //}

        internal static void LblCount(int count)
        {
            main.Dispatcher.Invoke(() => { main.lblCount.Content = filesSelectedStr + count; });
        }

        internal static void LblProcessed(int processedCounter, int count)
        {
            main.Dispatcher.Invoke(() => { main.lblCount.Content = filesProcessedStr + processedCounter + @"/" + count; });
        }

        internal async Task FileBrowseAsync()
        {
            try
            {
                var vfbd = new VistaFolderBrowserDialog { SelectedPath = path, Description = "Выберите файл(ы) или папку(и)", ShowNewFolderButton = false };
                if (vfbd.ShowDialog() == true)
                {
                    main.Dispatcher.Invoke(() =>
                                           {
                                               main.lblInfo.Content = "Поиск файлов";
                                               main.btnBrowse.IsEnabled = false;
                                               main.btnStart.IsEnabled = false;
                                               main.btnCancel.IsEnabled = true;
                                           });
                    cts = new CancellationTokenSource();
                    path = vfbd.SelectedPath;
                    main.lblInfo.Content = path;
                    di = new DirectoryInfo(path);
                    await SearchOps.FilterFilesAsync(files, new[] { path }, searchPattern, cts.Token).ContinueWith(n => SearchComplete());
                }

            }
            catch (OperationCanceledException ex){}
        }

        internal async Task FilesDropAsync(DragEventArgs e)
        {
            try
            {
                main.Dispatcher.Invoke(() =>
                                       {
                                           main.lblInfo.Content = "Поиск файлов";
                                           main.btnBrowse.IsEnabled = false;
                                           main.btnStart.IsEnabled = false;
                                           main.btnCancel.IsEnabled = true;
                                       });
                cts = new CancellationTokenSource();
                string[] paths = (string[]) e.Data.GetData(DataFormats.FileDrop, true);
                await SearchOps.FilterFilesAsync(files, paths, searchPattern, cts.Token).ContinueWith(n => SearchComplete());
            }
            catch (OperationCanceledException ex){}
        }

        void SearchComplete()
        {
            main.Dispatcher.Invoke(() =>
                                   {
                                       main.lblInfo.Content = "Поиск завершён";
                                       main.btnBrowse.IsEnabled = true;
                                       main.btnStart.IsEnabled = true;
                                       main.btnCancel.IsEnabled = true;
                                   });
        }

        void ConvertationComplete()
        {
            processedCounter = 0;
            main.Dispatcher.Invoke(() =>
                                   {
                                       main.lblInfo.Content = "Конвертация завершена";
                                       main.btnBrowse.IsEnabled = true;
                                       main.btnStart.IsEnabled = false;
                                       main.btnCancel.IsEnabled = false;
                                   });
            files.Clear();
            //hasDiffDirs = false;
        }

        internal void Reset()
        {
            cts.Cancel();
            processedCounter = 0;
            //hasDiffDirs = false;
            main.Dispatcher.Invoke(() =>
                                   {
                                       main.lblInfo.Content = "Операция отменена";
                                       main.btnBrowse.IsEnabled = true;
                                       main.btnStart.IsEnabled = false;
                                       main.btnCancel.IsEnabled = false;
                                   });
            files.Clear();
        }

        public async Task StartConvert(int restriction)
        {
            try
            {
                main.Dispatcher.Invoke(() =>
                                       {
                                           main.lblInfo.Content = "Конвертация файлов";
                                           main.btnBrowse.IsEnabled = false;
                                           main.btnStart.IsEnabled = false;
                                           main.btnCancel.IsEnabled = true;
                                       });
                cts = new CancellationTokenSource();
                await ImageConvert(files, restriction, cts.Token).ContinueWith(n => ConvertationComplete());
            }
            catch (OperationCanceledException ex){}
        }

        internal static async Task ImageConvert(ObservableCollection<string> files, int restriction,
                                                CancellationToken ct)
        {
            if (files.Count == 0) return;
            await Task.Run(() => Parallel.ForEach(files, path =>
                                                         {
                                                             ImageOps.ModifyImage(path, restriction, ct);
                                                             Interlocked.Increment(ref processedCounter);
                                                             LblProcessed(processedCounter, files.Count);
                                                         }), ct);
        }
    }
}