using System.ComponentModel.DataAnnotations;
using System;

namespace Datahub.Shared.EFCore
{

    public enum Storage_Type
    {
        AzureGen1, AzureGen2
    }
    public class Project_Storage
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string AccountName { get; set; }

        public Storage_Type Storage_Type { get; set; }
    }
}