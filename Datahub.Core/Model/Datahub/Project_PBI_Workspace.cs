using System.ComponentModel.DataAnnotations;
using System;

namespace Datahub.Core.EFCore
{
    public class Project_PBI_Workspace
    {
        [Key]
        public Guid Id { get; set; }

        public Datahub_Project Project { get; set; }
    }
}