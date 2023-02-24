namespace Datahub.Infrastructure.Services.Azure;

#nullable disable

class StorageUsedResponse
{
    public double cost { get; set; }
    //public string timespan { get; set; }
    public string interval { get; set; }
    public List<MetricValue> value { get; set; }
    //public string @namespace { get; set; }
    //public string resourceregion { get; set; }
}

class MetricValue
{
    //public string id { get; set; }
    //public string type { get; set; }
    //public ValueName name { get; set; }
    //public string displayDescription { get; set; }
    public string unit { get; set; }
    public List<Timeseries> timeseries { get; set; }
    public string errorCode { get; set; }
}

class Timeseries
{
    //public List<object> metadatavalues { get; set; }
    public List<TimeseriesValue> data { get; set; }
}

//class ValueName
//{
//    public string value { get; set; }
//    public string localizedValue { get; set; }
//}

public class TimeseriesValue
{
    public DateTime timeStamp { get; set; }
    public double average { get; set; }
}

#nullable enable