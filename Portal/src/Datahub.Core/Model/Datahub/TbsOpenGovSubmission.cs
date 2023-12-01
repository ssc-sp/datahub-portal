using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Datahub
{
    public class TbsOpenGovSubmission : OpenDataSubmission
    {
        public const string DATASET_FILE_TYPE = "Dataset";
        public const string METADATA_FILE_TYPE = "Metadata";
        public const string SUPPORTING_DOC_FILE_TYPE = "SupportingDoc";
        public const string DATA_DICTIONARY_FILE_TYPE = "DataDictionary";
        public const string IMSO_APPROVAL_FILE_TYPE = "ImsoApproval";

        public const string LOCALIZATION_PREFIX = nameof(OpenDataPublishProcessType.TbsOpenGovPublishing);

        public enum ProcessSteps
        {
            Initial = 1,
            AwaitingFiles,
            CheckingDataQuality,
            Uploading,
            AwaitingRemoteDqCheck,
            AwaitingImsoApproval,
            Publishing,
            Published
        }

        public int? OpenGovCriteriaFormId { get; set; }
        public DateTime? OpenGovCriteriaMetDate { get; set; }
        public bool LocalDQCheckStarted { get; set; }
        public bool LocalDQCheckPassed { get; set; }
        public DateTime? InitialOpenGovSubmissionDate { get; set; }
        public bool OpenGovDQCheckPassed { get; set; }
        public bool ImsoApproved { get; set; }
        public DateTime? OpenGovPublicationDate { get; set; }

        public override string LocalizationPrefix => LOCALIZATION_PREFIX;
    }
}
