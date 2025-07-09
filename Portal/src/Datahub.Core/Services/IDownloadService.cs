namespace Datahub.Core.Services;

public interface IDownloadService
{
    Task DownloadStreamAsFile(Stream stream, string fileName);
}
