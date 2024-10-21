using Datahub.Metadata.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Datahub.Portal.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OpenDataApprovalController : Controller
{
    private readonly IDbContextFactory<MetadataDbContext> _contextFactory;

    public OpenDataApprovalController(IDbContextFactory<MetadataDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        using var ctx = _contextFactory.CreateDbContext();
            
        var form = await ctx.ApprovalForms.FirstOrDefaultAsync(f => f.ApprovalFormId == id);
        if (form == null)
            return NotFound();

        var formContent = GetDocumentContent(form);
            
        using var templateStream = GetDocumentTemplateStream();
        var outputStream = CompleteDocument(templateStream, formContent);

        if (outputStream.CanSeek && outputStream.Position > 0)
        {
            outputStream.Seek(0, SeekOrigin.Begin);
        }

        return new FileStreamResult(outputStream, "application/msword")
        {
            FileDownloadName = outputFileName
        };
    }

    const string outputFileName = "Open Government Approval Form.docx";

    static Dictionary<string, string> GetDocumentContent(ApprovalForm form)
    {
        return new()
        {
            { "Department", form.Department_NAME },
            { "Sector", form.Sector_NAME },
            { "Branch", form.Branch_NAME },
            { "Division", form.Division_NAME },
            { "Section", form.Section_NAME },
            { "Name", form.Name_NAME},
            { "Phone", form.Phone_TXT },
            { "Email", form.Email_EMAIL },
            { "Title", form.Dataset_Title_TXT },
            { "Data", GetCheckBox("Data" == form.Type_Of_Data_TXT) },
            { "Info", GetCheckBox("Info" == form.Type_Of_Data_TXT) },

            { "Legal1", GetCheckBox(form.Copyright_Restrictions_FLAG) },
            { "Legal2", GetCheckBox(!form.Copyright_Restrictions_FLAG) },

            { "Auth1", GetCheckBox(form.Authority_To_Release_FLAG) },
            { "Auth2", GetCheckBox(!form.Authority_To_Release_FLAG) },

            { "Privacy1", GetCheckBox(form.Private_Personal_Information_FLAG) },
            { "Privacy2", GetCheckBox(!form.Private_Personal_Information_FLAG) },

            { "Access1", GetCheckBox(form.Subject_To_Exceptions_Or_Eclusions_FLAG) },
            { "Access2", GetCheckBox(!form.Subject_To_Exceptions_Or_Eclusions_FLAG) },

            { "Security1", GetCheckBox(form.Not_Clasified_Or_Protected_FLAG) },
            { "Security2", GetCheckBox(!form.Not_Clasified_Or_Protected_FLAG) },

            { "Cost1", GetCheckBox(form.Can_Be_Released_For_Free_FLAG) },
            { "Cost2", GetCheckBox(!form.Can_Be_Released_For_Free_FLAG) },

            { "FormatA1", GetCheckBox(form.Machine_Readable_FLAG) },
            { "FormatA2", GetCheckBox(!form.Machine_Readable_FLAG) },

            { "FormatB1", GetCheckBox(form.Non_Propietary_Format_FLAG) },
            { "FormatB2", GetCheckBox(!form.Non_Propietary_Format_FLAG) },

            { "FormatC1", GetCheckBox(form.Localized_Metadata_FLAG) },
            { "FormatC2", GetCheckBox(!form.Localized_Metadata_FLAG) },

            { "BlkApprov01", GetCheckBox(form.Requires_Blanket_Approval_FLAG) },
            { "BlkApprov02", GetCheckBox(!form.Requires_Blanket_Approval_FLAG) },

            { "BlkApprov1", GetCheckBox(form.Updated_On_Going_Basis_FLAG) },
            { "BlkApprov2", GetCheckBox(form.Collection_Of_Datasets_FLAG) },
            { "BlkApprov3", GetCheckBox(form.Approval_InSitu_FLAG) },
            { "BlkApprov4", GetCheckBox(form.Approval_Other_FLAG) },

            { "BlkApprovOther", form.Approval_Other_TXT }
        };
    }

    static string GetCheckBox(bool value) => value ? $"☒" : $"☐";

    static Stream CompleteDocument(Stream inputDoc, Dictionary<string, string> content)
    {
        MemoryStream mem = new();

        inputDoc.CopyTo(mem);
        mem.Seek(0, SeekOrigin.Begin);

        using var document = WordprocessingDocument.Open(mem, true);

        var elementsToUpdate = document.MainDocumentPart?.Document.Descendants()
            .Where(d => d.LocalName == "r" && MatchesField(d.InnerText))
            .Select(d => new ReplaceRecord(GetFieldName(d.InnerText), d))
            .ToList() ?? new();

        foreach (var field in elementsToUpdate)
        {
            var replaceValue = content.ContainsKey(field.FieldName) ? content[field.FieldName] : string.Empty;
            field.Element.RemoveAllChildren<Text>();
            field.Element.AppendChild(new Text(replaceValue));
        }

        document.MainDocumentPart?.Document.Save();

        mem.Seek(0, SeekOrigin.Begin);

        return mem;
    }

    static Stream GetDocumentTemplateStream()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream("Datahub.Portal.Controllers.OpenDataApprovalForm.docx");
    }

    const string TemplateHeading = "TEMPLATE_";
    static bool MatchesField(string fieldName) => fieldName.StartsWith(TemplateHeading);
    static string GetFieldName(string fieldName) => fieldName.Substring(TemplateHeading.Length);

    record ReplaceRecord(string FieldName, OpenXmlElement Element);
}