using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace LJournizer
{
    public static class SearchOps
    {
        static string[] searchPatterns;
        static SearchOption searchOption;
        
        static SearchOps()
        {
            searchOption = SearchOption.TopDirectoryOnly;
        }


        static async Task SearchFilesAsync(ICollection<string> files, string path, CancellationToken ct)
        {
            await Task.Run(async () =>
                           {
                               foreach (var sp in searchPatterns)
                                   foreach (var f in Directory.GetFiles(path, sp, searchOption))
                                   {
                                       if (ct.IsCancellationRequested)
                                           ct.ThrowIfCancellationRequested();
                                       if (!files.Contains(f))
                                           files.Add(f);
                                   }
                               foreach (var dir in Directory.GetDirectories(path))
                                   await SearchFilesAsync(files, dir, ct);
                           }, ct);
        }

        static async Task CheckFileAsync(ICollection<string> files, string path, CancellationToken ct)
        {
            await Task.Run(() =>
                           {
                               foreach (var sp in searchPatterns)
                               {
                                   if (sp.Substring(1) == Path.GetExtension(path) && !files.Contains(path))
                                       files.Add(path);
                               }
                           }, ct);
        }

        public static async Task FilterFilesAsync(ObservableCollection<string> files, string[] paths, string searchPattern, CancellationToken ct)
        {
            searchPatterns = searchPattern.Split('|');
            await Task.Run(async () =>
                           {
                               foreach (var path in paths)
                                   if (File.GetAttributes(path) == FileAttributes.Directory)
                                       await SearchFilesAsync(files, path, ct);
                                   
                                   else
                                       await CheckFileAsync(files, path, ct);
                           }, ct);
        }
    }
}