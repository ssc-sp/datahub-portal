namespace Datahub.Infrastructure.Services;

class UploadProgressHandler : IProgress<long>
{
    private readonly Action<long> _onProgress;

    public UploadProgressHandler(Action<long> onProgress)
    {
        _onProgress = onProgress;
    }

    public void Report(long value) => _onProgress.Invoke(value);
}