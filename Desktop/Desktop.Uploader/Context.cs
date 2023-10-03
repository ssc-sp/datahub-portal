
using Datahub.Core.DataTransfers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Maui.Uploader
{
    public class Context
    {
        public double UploadSpeedMpbs { get; internal set; }
        public double DownloadSpeedMpbs { get; internal set; }
        public UploadCredentials Credentials { get; internal set; }
    }
}
