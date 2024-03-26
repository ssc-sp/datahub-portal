using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace Datahub.Core.Services.Api;

public class ApiTelemetryService
{
    public const string FileUploadSizeMetricName = "FIleUploadSize";
    public const string FileUploadTimeMetricName = "FIleUploadTime";
    public const string FileUploadBPMSMetricName = "FIleUploadBPMS";
    private TelemetryConfiguration config = TelemetryConfiguration.CreateDefault();

    private TelemetryClient client;
    protected TelemetryClient Client
    {
        get
        {
            if (client == null)
            {
                client = new TelemetryClient(config);
            }

            return client;
        }
    }
    public ApiTelemetryService()
    {
    }

    public void LogMetric(string metricName, double value, string filename)
    {
        var dict = new Dictionary<string, string>();
        dict.Add("filename", filename);
        Client.TrackMetric(metricName, value, dict);
    }

    public void LogFileUploadSize(long filesize, string filename)
    {
        LogMetric(FileUploadSizeMetricName, filesize, filename);
    }
    public void LogFileUploadTime(long uploadTime, string filename)
    {
        LogMetric(FileUploadTimeMetricName, uploadTime, filename);
    }
    public void LogFileUploadBpms(long bpms, string filename)
    {
        LogMetric(FileUploadBPMSMetricName, bpms, filename);
    }
}