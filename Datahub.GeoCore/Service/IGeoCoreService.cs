namespace Datahub.GeoCore.Service
{
    public interface IGeoCoreService
    {
        Task<GeoCoreResult> CreatePackage(string data);
    }

    public record GeoCoreResult(bool Suceeded, string ErrorMessage);
}
