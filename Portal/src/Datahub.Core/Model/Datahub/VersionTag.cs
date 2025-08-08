using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Model.Datahub
{
    /// <summary>
    /// Represents a version tag, which is utilized in the Terraform infrastructure.
    /// </summary>
    public class VersionTag
    {
        /// <summary>
        /// Gets or sets the unique identifier for the version tag.
        /// </summary>
        [Key]
        public int VersionTagId { get; set; }

        /// <summary>
        /// Gets or sets the tag string. This value is used by the Terraform infrastructure to identify specific versions.
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the description of the version tag.
        /// </summary>
        [Required]
        public string VersionDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the version tag is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the version tag was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who created the version tag.
        /// </summary>
        public string CreatedBy { get; set; }
    }
}
