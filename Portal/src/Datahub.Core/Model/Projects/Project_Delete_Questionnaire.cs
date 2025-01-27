using System.ComponentModel.DataAnnotations;
using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Model.Projects
{
    public class Project_Delete_Questionnaire
    {
        [Key]
        public int Id { get; set; }
        public bool IsWorkspaceNotRequired { get; set; }
        public bool IsDataMigrated { get; set; }
        public bool IsDataNotSubjectToLitigation { get; set; }
        public bool DoesDataNotHaveArchivalValue { get; set; }
        public bool IsDeletionConfirmed { get; set; }

        public DateTime? DeletedDate { get; set; }
        public Datahub_Project Project { get; set; }
        public PortalUser DeletedBy { get; set; }
    }
}
