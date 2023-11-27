using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Datahub
{

    public class OpenDataPublishFile
    {
        public long Id { get; set; }
        public long SubmissionId { get; set; }
        public string FilePurpose { get; set; }
        public string FileName { get; set; }
        public string FolderPath { get; set; }
        public string FileId { get; set; }
        public OpenDataPublishFileUploadStatus UploadStatus { get; set; } = OpenDataPublishFileUploadStatus.NotStarted;
        public string UploadMessage { get; set; }

        public OpenDataSubmission Submission { get; set; }

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
        InProgress,
        Completed,
        Failed
    }
}
