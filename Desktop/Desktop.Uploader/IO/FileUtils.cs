using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Maui.Uploader.IO
{
    public class FileUtils
    {
        private readonly IStringLocalizer<App> localizer;

        public FileUtils(IStringLocalizer<App> localizer)
        {
            this.localizer = localizer;
        }

        public long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException();

            long totalSize = 0;

            try
            {
                // Get the size of files in the current directory
                string[] files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle any permission-related exceptions
                Console.WriteLine($"Access denied: {path}");
            }

            // Get the size of subdirectories in parallel
            string[] directories = Directory.GetDirectories(path);
            Parallel.ForEach(directories, directory =>
            {
                try
                {
                    long directorySize = GetDirectorySize(directory);
                    Interlocked.Add(ref totalSize, directorySize);
                }
                catch (UnauthorizedAccessException)
                {
                    // Handle any permission-related exceptions
                    Console.WriteLine($"Access denied: {directory}");
                }
                catch (DirectoryNotFoundException)
                {
                    // Handle if a subdirectory was deleted during the operation
                    Console.WriteLine($"Directory not found: {directory}");
                }
            });

            return totalSize;
        }

        public string GetFriendlyFileSize(long fileSizeBytes)
        {
            string[] sizeSuffixes = { localizer["B"], localizer["KB"], localizer["MB"], localizer["GB"], localizer["TB"] };
            int suffixIndex = 0;
            double size = fileSizeBytes;

            while (size >= 1024 && suffixIndex < sizeSuffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:0.##} {sizeSuffixes[suffixIndex]}";
        }

    }
}
