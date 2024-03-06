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
            { "Department", form.DepartmentNAME },
            { "Sector", form.SectorNAME },
            { "Branch", form.BranchNAME },
            { "Division", form.DivisionNAME },
            { "Section", form.SectionNAME },
            { "Name", form.NameNAME},
            { "Phone", form.PhoneTXT },
            { "Email", form.EmailEMAIL },
            { "Title", form.DatasetTitleTXT },
            { "Data", GetCheckBox("Data" == form.TypeOfDataTXT) },
            { "Info", GetCheckBox("Info" == form.TypeOfDataTXT) },

            { "Legal1", GetCheckBox(form.CopyrightRestrictionsFLAG) },
            { "Legal2", GetCheckBox(!form.CopyrightRestrictionsFLAG) },

            { "Auth1", GetCheckBox(form.AuthorityToReleaseFLAG) },
            { "Auth2", GetCheckBox(!form.AuthorityToReleaseFLAG) },

            { "Privacy1", GetCheckBox(form.PrivatePersonalInformationFLAG) },
            { "Privacy2", GetCheckBox(!form.PrivatePersonalInformationFLAG) },

            { "Access1", GetCheckBox(form.SubjectToExceptionsOrEclusionsFLAG) },
            { "Access2", GetCheckBox(!form.SubjectToExceptionsOrEclusionsFLAG) },

            { "Security1", GetCheckBox(form.NotClasifiedOrProtectedFLAG) },
            { "Security2", GetCheckBox(!form.NotClasifiedOrProtectedFLAG) },

            { "Cost1", GetCheckBox(form.CanBeReleasedForFreeFLAG) },
            { "Cost2", GetCheckBox(!form.CanBeReleasedForFreeFLAG) },

            { "FormatA1", GetCheckBox(form.MachineReadableFLAG) },
            { "FormatA2", GetCheckBox(!form.MachineReadableFLAG) },

            { "FormatB1", GetCheckBox(form.NonPropietaryFormatFLAG) },
            { "FormatB2", GetCheckBox(!form.NonPropietaryFormatFLAG) },

            { "FormatC1", GetCheckBox(form.LocalizedMetadataFLAG) },
            { "FormatC2", GetCheckBox(!form.LocalizedMetadataFLAG) },

            { "BlkApprov01", GetCheckBox(form.RequiresBlanketApprovalFLAG) },
            { "BlkApprov02", GetCheckBox(!form.RequiresBlanketApprovalFLAG) },

            { "BlkApprov1", GetCheckBox(form.UpdatedOnGoingBasisFLAG) },
            { "BlkApprov2", GetCheckBox(form.CollectionOfDatasetsFLAG) },
            { "BlkApprov3", GetCheckBox(form.ApprovalInSituFLAG) },
            { "BlkApprov4", GetCheckBox(form.ApprovalOtherFLAG) },

            { "BlkApprovOther", form.ApprovalOtherTXT }
        };
    }

    static string GetCheckBox(bool value) => value ? $"☒" : $"☐";

    static Stream CompleteDocument(Stream inputDoc, Dictionary<string, string> content)
    {
        MemoryStream mem = new();

        inputDoc.CopyTo(mem);
        mem.Seek(0, SeekOrigin.Begin);

        var document = WordprocessingDocument.Open(mem, true);

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
        document.Close();

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