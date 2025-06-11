using Datahub.Core.Model.CloudStorage;

namespace Datahub.Core.Model.Datahub
{
    /// <summary>
    /// Represents a file to be published as part of an open data submission, associated with a specific workspace.
    /// </summary>
    public class OpenDataPublishFile
    {
        /// <summary>
        /// Gets or sets the unique identifier for the open data publish file record.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the open data submission this file belongs to.
        /// </summary>
        public long SubmissionId { get; set; }

        /// <summary>
        /// Gets or sets the purpose or reason for including this file in the open data submission.
        /// </summary>
        public string FilePurpose { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the workspace cloud storage where the file is located.
        /// </summary>
        public int? ProjectStorageId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the path to the folder containing the file within the workspace storage.
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier for the file itself (e.g., a GUID or a storage-specific ID).
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Gets or sets the name of the storage container where the file is located.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the current status of the file upload process.
        /// </summary>
        public OpenDataPublishFileUploadStatus UploadStatus { get; set; } = OpenDataPublishFileUploadStatus.NotStarted;

        /// <summary>
        /// Gets or sets a message providing details about the upload status, especially in case of errors.
        /// </summary>
        public string UploadMessage { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the parent open data submission.
        /// </summary>
        public OpenDataSubmission Submission { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the workspace cloud storage.
        /// </summary>
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

    /// <summary>
    /// Defines the possible statuses for an open data publish file upload.
    /// </summary>
    public enum OpenDataPublishFileUploadStatus
    {
        /// <summary>
        /// The upload process has not yet started.
        /// </summary>
        NotStarted,

        /// <summary>
        /// The file is ready to be uploaded.
        /// </summary>
        ReadyToUpload,

        /// <summary>
        /// The file upload is currently in progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// The file upload has completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// The file upload has failed.
        /// </summary>
        Failed
    }
}
