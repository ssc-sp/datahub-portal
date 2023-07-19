namespace Datahub.Infrastructure.Services.Azure;

#nullable disable

record GetStorageAccountsResponse(StorageAccountInfo[] value);

record StorageAccountInfo(string id, string name);

public class StorageCapacityResponse
{
    //public double cost { get; set; }
    //public string timespan { get; set; }
    //public string interval { get; set; }
    public List<StorageCapacityResponseValue> value { get; set; } = new();
    //public string @namespace { get; set; }
    //public string resourceregion { get; set; }
}

public class StorageCapacityResponseValue
{
    //public string id { get; set; }
    //public string type { get; set; }
    //public Name name { get; set; }
    //public string displayDescription { get; set; }
    //public string unit { get; set; }
    public List<StorageCapacityResponseTimeseries> timeseries { get; set; } = new();
    //public string errorCode { get; set; }
}

public class StorageCapacityResponseTimeseries
{
    //public List<object> metadatavalues { get; set; }
    public List<StorageCapacityResponseDatum> data { get; set; }
}

public class StorageCapacityResponseDatum
{
    //public DateTime timeStamp { get; set; }
    public double average { get; set; }
}

#nullable enable