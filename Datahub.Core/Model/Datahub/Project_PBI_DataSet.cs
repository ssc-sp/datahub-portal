using System.ComponentModel.DataAnnotations;
using System;

namespace Datahub.Shared.EFCore
{
    public class Project_PBI_DataSet
    {
        [Key]
        public Guid Id { get; set; }

        public Guid Workspace { get; set; }
        public Datahub_Project Project { get; set; }

        public string DatasetName { get; set; }
    }
}