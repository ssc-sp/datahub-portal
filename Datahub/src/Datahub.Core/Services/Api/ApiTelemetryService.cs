using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

public class ApiTelemetryService
{
    public const string  FileUploadSizeMetricName = "FIleUploadSize";
    public const string  FileUploadTimeMetricName = "FIleUploadTime";
    public const string  FileUploadBPMSMetricName = "FIleUploadBPMS";
    private TelemetryConfiguration _config = TelemetryConfiguration.CreateDefault();

    private TelemetryClient _client;
    protected TelemetryClient client
    {
        get
        {
            if (_client == null)
            {
                _client = new TelemetryClient(_config);
            }

            return _client;
        }
    }
    public ApiTelemetryService()
    {
    }                

    public void LogMetric(string metricName, double value, string filename)
    {
        var dict = new Dictionary<string, string>();
        dict.Add("filename", filename);
        client.TrackMetric(metricName, value, dict);
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