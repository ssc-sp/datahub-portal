using Google.Apis.Upload;

namespace Datahub.Infrastructure.Services.Storage
{
	public class GoogleUploadProgressHandler : IProgress<IUploadProgress>
    {
        private readonly Action<long> _progress;
        public GoogleUploadProgressHandler(Action<long> progress)
        {
            _progress = progress;
        }

        public void Report(IUploadProgress value) => _progress.Invoke(value.BytesSent);
    }
}
