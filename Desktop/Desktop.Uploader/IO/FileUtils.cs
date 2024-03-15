using Datahub.Maui.Uploader.Resources;
using System.Collections.Concurrent;

namespace Datahub.Maui.Uploader.IO
{
	public class FileUtils
    {

        public FileUtils()
        {
        }

        public string ToFriendlyFormat(TimeSpan timespan)
        {
            if (timespan.TotalSeconds < 1)
            {
                return "just now";
            }

            var intervals = new (string unit, double seconds)[]
            {
            ("week", 604800),
            ("day", 86400),
            ("hour", 3600),
            ("minute", 60),
            ("second", 1)
            };

            string friendlyFormat = "";

            foreach (var (unit, seconds) in intervals)
            {
                double count = timespan.TotalSeconds / seconds;
                if (count >= 1)
                {
                    string plural = count > 1 ? "s" : "";
                    friendlyFormat += $"{(int)count} {unit}{plural} ";
                    timespan = TimeSpan.FromSeconds(timespan.TotalSeconds - ((int)count * seconds));
                }
            }

            return friendlyFormat.TrimEnd();
        }
        public (long bytes, int files, List<FileInfo> allFiles) GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException();

            long totalSize = 0;
            int fileCount = 0;
            var allFiles = new ConcurrentBag<FileInfo>();
            try
            {
                // Get the size of files in the current directory
                string[] files = Directory.GetFiles(path);
                fileCount += files.Length;
                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    allFiles.Add(fileInfo);
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
                    (long directorySize, int files, List<FileInfo> subDirFiles) = GetDirectorySize(directory);
                    subDirFiles.ForEach(file => allFiles.Add(file));
                    Interlocked.Add(ref fileCount, files);
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

            return (totalSize, fileCount, allFiles.ToList());
        }

        public string GetFriendlyFileSize(long fileSizeBytes)
        {
            string[] sizeSuffixes = { AppResources._byte, AppResources.kiloByte, AppResources.megaByte, AppResources.gigaByte, AppResources.teraByte };
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
