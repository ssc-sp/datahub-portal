
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Maui.Uploader
{
    public class SpeedTestResults
    {
        public double? UploadSpeedMbps { get; internal set; }
        public double? DownloadSpeedMpbs { get; internal set; }
    }
}
