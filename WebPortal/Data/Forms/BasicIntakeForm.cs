using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.Portal.Data.Forms;

public class BasicIntakeForm
{
    [AeFormIgnore]
    private int Id { get; set; }
    
    [Required]
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Invalid Email Address")]
    [StringLength(20, ErrorMessage = "Identifier too long (16 character limit).")]
    [AeLabel(placeholder: "Please enter your email")]
    public string Email { get; set; }
    
    [Required]
    [StringLength(24, ErrorMessage = "Department Name too long (24 character limit).")]
    [AeLabel(placeholder:"Please enter the name of your department")]
    public string DepartmentName { get; set; }
    
    [Required]
    [StringLength(36, ErrorMessage = "Project Name too long (36 character limit).")]
    [AeLabel(placeholder:"Please enter the name of your project")]
    public string ProjectName { get; set; }
    
    [Required]
    [RegularExpression(@"^[A-Z]*$", ErrorMessage = "Please enter only unaccented alphabetical letters (A-Z)")]
    [StringLength(8, ErrorMessage = "Acronym is too long (8 character limit).")]
    [AeLabel(placeholder:"Please enter the desired acronym for your project")]
    public string ProjectAcronym { get; set; }
}