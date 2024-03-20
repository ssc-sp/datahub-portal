using Datahub.Core.Model.Datahub;

namespace Datahub.Core.Utils
{
    public record OpenDataPublishingStepStatus<T>(T Step, bool Started, bool Completed);

    public static class OpenDataPublishingUtils
    {
        public static bool IsDateSetAndPassed(DateTime? d) => d.HasValue && d.Value <= DateTime.Today;

        public static OpenDataPublishingStepStatus<T> NotStarted<T>(T step) => new(step, false, false);
        public static OpenDataPublishingStepStatus<T> Incomplete<T>(T step) => new(step, true, false);
        public static OpenDataPublishingStepStatus<T> Complete<T>(T step) => new(step, true, true);
    }

    public static class TbsOpenGovPublishingUtils
    {
        public static OpenDataPublishingStepStatus<TbsOpenGovSubmission.ProcessSteps> CheckStepStatus(TbsOpenGovSubmission submission, TbsOpenGovSubmission.ProcessSteps step) =>
        step switch
        {
            TbsOpenGovSubmission.ProcessSteps.AwaitingMetadata => submission.MetadataComplete ?
                OpenDataPublishingUtils.Complete(step) :
                OpenDataPublishingUtils.Incomplete(step),
            TbsOpenGovSubmission.ProcessSteps.AwaitingApprovalCriteria => OpenDataPublishingUtils.IsDateSetAndPassed(submission.OpenGovCriteriaMetDate) ?
                OpenDataPublishingUtils.Complete(step) :
                OpenDataPublishingUtils.Incomplete(step),
            TbsOpenGovSubmission.ProcessSteps.AwaitingFiles => DetermineAwaitingFilesStatus(submission),
            TbsOpenGovSubmission.ProcessSteps.CheckingDataQuality => submission.LocalDQCheckPassed ?
                OpenDataPublishingUtils.Complete(step) :
                submission.LocalDQCheckStarted ?
                    OpenDataPublishingUtils.Incomplete(step) :
                    OpenDataPublishingUtils.NotStarted(step),
            TbsOpenGovSubmission.ProcessSteps.Uploading => DetermineFileUploadStatus(submission),
            TbsOpenGovSubmission.ProcessSteps.AwaitingRemoteDqCheck => submission.OpenGovDQCheckPassed ?
                OpenDataPublishingUtils.Complete(step) :
                OpenDataPublishingUtils.IsDateSetAndPassed(submission.InitialOpenGovSubmissionDate) ?
                    OpenDataPublishingUtils.Incomplete(step) :
                    OpenDataPublishingUtils.NotStarted(step),
            TbsOpenGovSubmission.ProcessSteps.AwaitingImsoApproval => submission.ImsoApproved ?
                OpenDataPublishingUtils.Complete(step) :
                submission.ImsoApprovalRequested || CheckStepStatus(submission, TbsOpenGovSubmission.ProcessSteps.AwaitingRemoteDqCheck).Completed ?
                    OpenDataPublishingUtils.Incomplete(step) :
                    OpenDataPublishingUtils.NotStarted(step),
            TbsOpenGovSubmission.ProcessSteps.Publishing => OpenDataPublishingUtils.IsDateSetAndPassed(submission.OpenGovPublicationDate) ?
                OpenDataPublishingUtils.Complete(step) :
                CheckStepStatus(submission, TbsOpenGovSubmission.ProcessSteps.AwaitingImsoApproval).Completed ?
                    OpenDataPublishingUtils.Incomplete(step) :
                    OpenDataPublishingUtils.NotStarted(step),
            TbsOpenGovSubmission.ProcessSteps.Published => OpenDataPublishingUtils.IsDateSetAndPassed(submission.OpenGovPublicationDate) ?
                OpenDataPublishingUtils.Complete(step) :
                OpenDataPublishingUtils.NotStarted(step),
            _ => OpenDataPublishingUtils.NotStarted(step),
        };

        public static TbsOpenGovSubmission.ProcessSteps GetCurrentStatus(TbsOpenGovSubmission submission)
        {
            if (CheckStepStatus(submission, TbsOpenGovSubmission.ProcessSteps.Published).Completed)
            {
                return TbsOpenGovSubmission.ProcessSteps.Published;
            }
            else
            {
                return Enum.GetValues<TbsOpenGovSubmission.ProcessSteps>()
                    .Select(s => CheckStepStatus(submission, s))
                    .FirstOrDefault(s => s.Started && !s.Completed).Step;
            }
        }

        private static OpenDataPublishingStepStatus<TbsOpenGovSubmission.ProcessSteps> DetermineFileUploadStatus(TbsOpenGovSubmission submission)
        {
            var files = submission.Files;

            if (files == null || files.Count < 1)
            {
                return OpenDataPublishingUtils.NotStarted(TbsOpenGovSubmission.ProcessSteps.Uploading);
            }

            // IMSO approval is not uploaded so not included in this check
            var relevantPurposes = new HashSet<string>()
            {
                TbsOpenGovSubmission.DATASET_FILE_TYPE,
                TbsOpenGovSubmission.GUIDE_FILE_TYPE
            };
            var relevantFiles = files.Where(f => relevantPurposes.Contains(f.FilePurpose));

            if (relevantFiles.All(f => f.UploadStatus == OpenDataPublishFileUploadStatus.NotStarted))
            {
                return OpenDataPublishingUtils.NotStarted(TbsOpenGovSubmission.ProcessSteps.Uploading);
            }
            else if (relevantFiles.All(f => f.UploadStatus == OpenDataPublishFileUploadStatus.Completed))
            {
                return OpenDataPublishingUtils.Complete(TbsOpenGovSubmission.ProcessSteps.Uploading);
            }
            else
            {
                return OpenDataPublishingUtils.Incomplete(TbsOpenGovSubmission.ProcessSteps.Uploading);
            }
        }

        private static OpenDataPublishingStepStatus<TbsOpenGovSubmission.ProcessSteps> DetermineAwaitingFilesStatus(TbsOpenGovSubmission submission)
        {
            var files = submission.Files;
            var requiredPurposes = new HashSet<string>()
            {
                TbsOpenGovSubmission.DATASET_FILE_TYPE,
                TbsOpenGovSubmission.GUIDE_FILE_TYPE
            };

            var requiredFiles = files.Where(f => requiredPurposes.Contains(f.FilePurpose));

            if (files == null || files.Count < 1)
            {
                // no files => not started, unless criteria is met
                return CheckStepStatus(submission, TbsOpenGovSubmission.ProcessSteps.AwaitingMetadata).Completed ?
                    OpenDataPublishingUtils.Incomplete(TbsOpenGovSubmission.ProcessSteps.AwaitingFiles) :
                    OpenDataPublishingUtils.NotStarted(TbsOpenGovSubmission.ProcessSteps.AwaitingFiles);
            }
            else if (!requiredPurposes.Except(files.Select(f => f.FilePurpose)).Any() && requiredFiles.All(f => f.UploadStatus != OpenDataPublishFileUploadStatus.NotStarted))
            {
                // all required files => complete
                return OpenDataPublishingUtils.Complete(TbsOpenGovSubmission.ProcessSteps.AwaitingFiles);
            }
            else
            {
                // some but not all required files => started
                return OpenDataPublishingUtils.Incomplete(TbsOpenGovSubmission.ProcessSteps.AwaitingFiles);
            }
        }
    }
}
