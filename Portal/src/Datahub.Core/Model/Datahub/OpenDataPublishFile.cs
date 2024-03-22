using Datahub.Core.Model.CloudStorage;

namespace Datahub.Core.Model.Datahub
{
    public class OpenDataPublishFile
    {
        public long Id { get; set; }
        public long SubmissionId { get; set; }
        public string FilePurpose { get; set; }
        public int? ProjectStorageId { get; set; }
        public string FileName { get; set; }
        public string FolderPath { get; set; }
        public string FileId { get; set; }
        public string ContainerName { get; set; }
        public OpenDataPublishFileUploadStatus UploadStatus { get; set; } = OpenDataPublishFileUploadStatus.NotStarted;
        public string UploadMessage { get; set; }

        public OpenDataSubmission Submission { get; set; }
        public ProjectCloudStorage Storage { get; set; }

        public override int GetHashCode()
        {
            return FileId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as OpenDataPublishFile;
            return FileId.Equals(other?.FileId);
        }
    }

    public enum OpenDataPublishFileUploadStatus
    {
        NotStarted,
        ReadyToUpload,
        InProgress,
        Completed,
        Failed
    }
}
