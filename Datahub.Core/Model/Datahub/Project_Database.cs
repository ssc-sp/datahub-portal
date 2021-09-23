using System.ComponentModel.DataAnnotations;
using System;

namespace NRCan.Datahub.Shared.EFCore
{
    public class Project_Database
    {
        [Key]
        public Guid Id { get; set; }

        public Datahub_Project Project { get; set; }
    }

    public class Project_Resources
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200)]
        public string ResourceType {  get; set; }
        [StringLength(200)]
        public string ResourceName {  get; set; }
        [StringLength(200)]
        public string Attributes {  get; set; }
        public DateTime TimeRequested {  get; set; }
        public DateTime? TimeCreated {  get; set; }

        public Datahub_Project Project { get; set; }
    }
}