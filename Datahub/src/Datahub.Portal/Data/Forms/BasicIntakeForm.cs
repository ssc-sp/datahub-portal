using System.ComponentModel.DataAnnotations;
using Datahub.Core.Data;
using Elemental.Components;

namespace Datahub.Portal.Data.Forms;

public class BasicIntakeForm
{
    [Required]
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Invalid Email Address")]
    [StringLength(20, ErrorMessage = "Identifier too long (16 character limit).")]
    [AeLabel(placeholder: "Please enter your email")]
    public string Email { get; set; }

    [Required]
    [AeLabel("Department Name", isDropDown: true)] 
    public string DepartmentName { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Project Name too long (200 character limit).")]
    [AeLabel("Project Name", "Please enter the name of your project")]
    public string ProjectName { get; set; }


    [AeFormIgnore]
    public static Dictionary<string, string> DepartmentDictionary => GovernmentDepartment.Departments;
}