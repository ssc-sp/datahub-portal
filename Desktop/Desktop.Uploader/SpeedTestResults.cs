
using Datahub.Core.DataTransfers;

namespace Datahub.Maui.Uploader
{
	public class SpeedTestResults
    {
        public UploadCredentials Credentials { get; internal set; }
        public double? UploadSpeedMbps { get; internal set; }
        public double? DownloadSpeedMpbs { get; internal set; }
    }
}
