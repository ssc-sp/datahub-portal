using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class DatahubTools
    {
        public static string BytesToString(string byteCountStr)
        {
            long byteCount;
            if (long.TryParse(byteCountStr, out byteCount))
            {
                return DatahubTools.BytesToString(byteCount);
            }

            return "...";
        }

        private static string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        public static string BytesToString(long byteCount)
        {
            if (byteCount == 0)
            {
                return "0" + suf[0];
            }

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);

            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public static string GetFileTypeIcon(string filetype)
        {
            switch (filetype.ToLower())
            {
                case "doc":
                case "docx":
                    return "far fa-file-word";
                case "txt":
                case "json":
                    return "far fa-file-alt";
                case "png":
                case "gif":
                case "jpg":
                case "jpeg":
                    return "far fa-file-image";
                case "pdf":
                    return "far fa-file-pdf";
                case "xls":
                case "xlsx":
                    return "far fa-file-excel";
                case "zip":
                    return "far fa-file-archive";
                default:
                    return "far fa-file";
            }
        }
    }
}
