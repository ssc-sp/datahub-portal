namespace Datahub.GeoCore.Service;

public interface IGeoCoreService
{
	Task<GeoCoreResult> PublishDataset(string data);
	Task<ShemaValidatorResult> ValidateJson(string data);
	string GetDatasetUrl(string datasetId, string lang);
}

public record GeoCoreResult(bool Suceeded, string DatasetId, string ErrorMessage);

public record ShemaValidatorResult(bool Valid, string ErrorMessages);