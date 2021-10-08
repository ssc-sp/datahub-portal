using Datahub.Portal.Data.Forms.ShareWorkflow;
using Datahub.Core.Utils;
using Xunit;
using MetadataModel = Datahub.Metadata.Model;

namespace DatahubTest
{
    public class ApprovalFormTests
    {
        [Fact]
        public void TestApprovalFormMapping()
        {
            var approvalFormEntity = new MetadataModel.ApprovalForm();
            var approvalFormViewModel = new ApprovalForm();

            // test mapping property from entity to viewmodel
            approvalFormEntity.ApprovalFormId = 11;
            approvalFormEntity.CopyPublicPropertiesTo(approvalFormViewModel, true);
            Assert.Equal(approvalFormEntity.ApprovalFormId, approvalFormViewModel.ApprovalFormId);

            // test mapping property from viewmodel to entity
            approvalFormViewModel.ApprovalFormId = 21;
            approvalFormViewModel.CopyPublicPropertiesTo(approvalFormEntity, true);
            Assert.Equal(approvalFormEntity.ApprovalFormId, approvalFormViewModel.ApprovalFormId);
        }
    }
}
