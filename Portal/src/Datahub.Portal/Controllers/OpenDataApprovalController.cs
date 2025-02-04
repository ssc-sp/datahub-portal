using Datahub.Application.Services.Metadata;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Metadata.Model;
using Datahub.Metadata.Utils;
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
    private readonly IDbContextFactory<MetadataDbContext> _metadataContextFactory;
    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubProjectContextFactory;
    private readonly IMetadataBrokerService _metadataService;

    private const string OUTPUT_FILE_NAME = "Open Government Approval Form.docx";
    private const string TEMPLATE_PREFIX = "TEMPLATE_";
    private const string MS_WORD_MIMETYPE = "application/msword";

    private const string DATASET_ID_FIELD_NAME = "DatasetId";
    private const string DATASET_NAME_EN_FIELD_NAME = "DatasetNameEn";
    private const string DATASET_NAME_FR_FIELD_NAME = "DatasetNameFr";
    private const string DEPARTMENT_FIELD_NAME = "Department";
    private const string SECTOR_FIELD_NAME = "Sector";
    private const string BRANCH_FIELD_NAME = "Branch";
    private const string DIVISION_FIELD_NAME = "Division";
    private const string SECTION_FIELD_NAME = "Section";
    private const string NAME_FIELD_NAME = "Name";
    private const string PHONE_FIELD_NAME = "Phone";
    private const string EMAIL_FIELD_NAME = "Email";
    private const string TITLE_FIELD_NAME = "Title";
    private const string DATA_FIELD_NAME = "Data";
    private const string INFO_FIELD_NAME = "Info";
    private const string BLANKET_APPROVAL_1_FIELD_NAME = "BlkApprov1";
    private const string BLANKET_APPROVAL_2_FIELD_NAME = "BlkApprov2";
    private const string BLANKET_APPROVAL_3_FIELD_NAME = "BlkApprov3";
    private const string BLANKET_APPROVAL_4_FIELD_NAME = "BlkApprov4";
    private const string BLANKET_APPROVAL_OTHER_FIELD_NAME = "BlkApprovOther";
    private const string FILE_NAME_EN_FIELD_NAME = "FileNameEn";
    private const string FILE_NAME_FR_FIELD_NAME = "FileNameFr";
    private const string FILE_LANG_EN_FIELD_NAME = "FileLangEn";
    private const string FILE_LANG_FR_FIELD_NAME = "FileLangFr";
    private const string FILE_FILENAME_FIELD_NAME = "FileFilename";
    private const string CONFIDENTIALITY_FIELD_NAME = "Conf";
    private const string ACCESS_FIELD_NAME = "Access";
    private const string AUTHORITY_FIELD_NAME = "Auth";
    private const string PRIVACY_A_FIELD_NAME = "PrivacyA";
    private const string PRIVACY_B_FIELD_NAME = "PrivacyB";
    private const string FORMAT_A_FIELD_NAME = "FormatA";
    private const string FORMAT_B_FIELD_NAME = "FormatB";
    private const string FORMAT_C_FIELD_NAME = "FormatC";
    private const string LANGUAGE_FIELD_NAME = "Lang";
    private const string SECURITY_FIELD_NAME = "Security";
    private const string MISC_FIELD_NAME = "Misc";
    private const string REQUIRES_BLANKET_APPROVAL_FIELD_NAME = "BlkApprov0";

    public OpenDataApprovalController(IDbContextFactory<MetadataDbContext> metadataContextFactory, 
        IDbContextFactory<DatahubProjectDBContext> datahubProjectContextFactory,
        IMetadataBrokerService metadataBrokerService)
    {
        _metadataContextFactory = metadataContextFactory;
        _datahubProjectContextFactory = datahubProjectContextFactory;
        _metadataService = metadataBrokerService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int? id = 0)
    {
        if (id is null)
        {
            return NotFound();
        }

        try
        {
            var submission = await GetSubmissionByApprovalFormIdAsync(id);
            var outputStream = await GenerateImsoForm(submission);
            return new FileStreamResult(outputStream, MS_WORD_MIMETYPE)
            {
                FileDownloadName = OUTPUT_FILE_NAME
            };
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }

    }

    private async Task<TbsOpenGovSubmission> GetSubmissionByApprovalFormIdAsync(int? approvalFormId)
    {
        using var ctx = await _datahubProjectContextFactory.CreateDbContextAsync();
        var submission = await ctx.TbsOpenGovSubmissions
            .AsNoTracking()
            .Include(s => s.Files)
            .FirstOrDefaultAsync(s => s.OpenGovCriteriaFormId == approvalFormId);
        
        if (submission is null)
        {
            throw new FileNotFoundException();
        }

        return submission;
    }

    private async Task<Stream> GenerateImsoForm(TbsOpenGovSubmission submission)
    {
        using var ctx = await _metadataContextFactory.CreateDbContextAsync();

        var form = await ctx.ApprovalForms
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.ApprovalFormId == submission.OpenGovCriteriaFormId);
        
        if (form == null)
        {
            throw new FileNotFoundException();
        }

        var formContent = GetDocumentContent(form);

        await AddSubmissionMetadataToFormContent(submission, formContent);

        using var templateStream = GetDocumentTemplateStream();
        var outputStream = await CompleteDocument(templateStream, formContent, submission);

        if (outputStream.CanSeek && outputStream.Position > 0)
        {
            outputStream.Seek(0, SeekOrigin.Begin);
        }

        return outputStream;
    }

    private async Task AddSubmissionMetadataToFormContent(TbsOpenGovSubmission submission, Dictionary<string, string> formContent)
    {
        var fieldValues = await _metadataService.GetObjectMetadataValues(submission.UniqueId);

        if (fieldValues.Count < 1)
        {
            throw new FileNotFoundException();
        }

        formContent.Add(DATASET_ID_FIELD_NAME, submission.UniqueId);
        formContent.Add(DATASET_NAME_EN_FIELD_NAME, fieldValues[FieldNames.title_translated_en].Value_TXT);
        formContent.Add(DATASET_NAME_FR_FIELD_NAME, fieldValues[FieldNames.title_translated_fr].Value_TXT);
    }

    static Dictionary<string, string> GetDocumentContent(ApprovalForm form)
    {
        var contentDict = new Dictionary<string,string>()
        {
            { DEPARTMENT_FIELD_NAME, form.Department_NAME },
            { SECTOR_FIELD_NAME, form.Sector_NAME },
            { BRANCH_FIELD_NAME, form.Branch_NAME },
            { DIVISION_FIELD_NAME, form.Division_NAME },
            { SECTION_FIELD_NAME, form.Section_NAME },
            { NAME_FIELD_NAME, form.Name_NAME},
            { PHONE_FIELD_NAME, form.Phone_TXT },
            { EMAIL_FIELD_NAME, form.Email_EMAIL },
            { TITLE_FIELD_NAME, form.Dataset_Title_TXT },
            { DATA_FIELD_NAME, GetCheckBox("Data" == form.Type_Of_Data_TXT) },
            { INFO_FIELD_NAME, GetCheckBox("Info" == form.Type_Of_Data_TXT) },


            { BLANKET_APPROVAL_1_FIELD_NAME, GetCheckBox(form.Updated_On_Going_Basis_FLAG) },
            { BLANKET_APPROVAL_2_FIELD_NAME, GetCheckBox(form.Collection_Of_Datasets_FLAG) },
            { BLANKET_APPROVAL_3_FIELD_NAME, GetCheckBox(form.Approval_InSitu_FLAG) },
            { BLANKET_APPROVAL_4_FIELD_NAME, GetCheckBox(form.Approval_Other_FLAG) },

            { BLANKET_APPROVAL_OTHER_FIELD_NAME, form.Approval_Other_TXT },

            // keep the placeholders for dataset files, to be filled in afterward
            { FILE_NAME_EN_FIELD_NAME, $"{TEMPLATE_PREFIX}{FILE_NAME_EN_FIELD_NAME}" },
            { FILE_NAME_FR_FIELD_NAME, $"{TEMPLATE_PREFIX}{FILE_NAME_FR_FIELD_NAME}" },
            { FILE_LANG_EN_FIELD_NAME, $"{TEMPLATE_PREFIX}{FILE_LANG_EN_FIELD_NAME}" },
            { FILE_LANG_FR_FIELD_NAME, $"{TEMPLATE_PREFIX}{FILE_LANG_FR_FIELD_NAME}" },
            { FILE_FILENAME_FIELD_NAME, $"{TEMPLATE_PREFIX}{FILE_FILENAME_FIELD_NAME}" }
        };

        AddCheckboxSet(contentDict, CONFIDENTIALITY_FIELD_NAME, form.Confidentiality_FLAG);
        AddCheckboxSet(contentDict, ACCESS_FIELD_NAME, form.Subject_To_Exceptions_Or_Eclusions_FLAG);
        AddCheckboxSet(contentDict, AUTHORITY_FIELD_NAME, form.Authority_To_Release_FLAG);
        AddCheckboxSet(contentDict, PRIVACY_A_FIELD_NAME, form.Privacy_Exemption_FLAG);
        AddCheckboxSet(contentDict, PRIVACY_B_FIELD_NAME, form.Private_Personal_Information_FLAG);
        AddCheckboxSet(contentDict, FORMAT_A_FIELD_NAME, form.Accessible_Format_FLAG);
        AddCheckboxSet(contentDict, FORMAT_B_FIELD_NAME, form.Machine_Readable_FLAG);
        AddCheckboxSet(contentDict, FORMAT_C_FIELD_NAME, form.Non_Propietary_Format_FLAG);
        AddCheckboxSet(contentDict, LANGUAGE_FIELD_NAME, form.Localized_FLAG);
        AddCheckboxSet(contentDict, SECURITY_FIELD_NAME, form.Security_Compliant_FLAG);
        AddCheckboxSet(contentDict, MISC_FIELD_NAME, form.Misc_Compliant_FLAG);
        AddCheckboxSet(contentDict, REQUIRES_BLANKET_APPROVAL_FIELD_NAME, form.Requires_Blanket_Approval_FLAG);

        return contentDict;
    }

    static string GetCheckBox(bool value) => value ? $"☒" : $"☐";

    private static void AddCheckboxSet(Dictionary<string, string> dict, string fieldName, bool value)
    {
        dict[$"{fieldName}1"] = GetCheckBox(value);
        dict[$"{fieldName}2"] = GetCheckBox(!value);
    }

    private static List<ReplaceRecord> BuildListOfRecordsToReplace(OpenXmlElement rootElement) => rootElement.Descendants()
            .Where(d => d.LocalName == "r" && MatchesField(d.InnerText))
            .Select(d => new ReplaceRecord(GetFieldName(d.InnerText), d))
            .ToList() ?? [];

    private static void ReplaceFieldContent(List<ReplaceRecord> elementsToUpdate, Dictionary<string, string> content)
    {
        foreach (var field in elementsToUpdate)
        {
            var replaceValue = content.TryGetValue(field.FieldName, out string value) ? value : string.Empty;
            field.Element.RemoveAllChildren<Text>();
            field.Element.AppendChild(new Text(replaceValue));
        }
    }

    private async Task<Dictionary<string,string>> GetSubmissionFileDetails(OpenDataPublishFile file)
    {
        var metadata = await _metadataService.GetObjectMetadataValues(file.FileId);
        var langValue = metadata[FieldNames.resource_language]?.Value_TXT ?? string.Empty;
        return new()
        {
            { FILE_NAME_EN_FIELD_NAME, metadata[FieldNames.name_translated_en]?.Value_TXT ?? string.Empty },
            { FILE_NAME_FR_FIELD_NAME, metadata[FieldNames.name_translated_fr]?.Value_TXT ?? string.Empty },
            { FILE_LANG_EN_FIELD_NAME, GetCheckBox(langValue.Contains("en")) },
            { FILE_LANG_FR_FIELD_NAME, GetCheckBox(langValue.Contains("fr")) },
            { FILE_FILENAME_FIELD_NAME, file.FileName },
        };
    }

    private async Task FillSubmissionFileDetails(WordprocessingDocument document, TbsOpenGovSubmission submission)
    {
        var fileInfoParagraph = document.MainDocumentPart?.Document.Descendants()
            .FirstOrDefault(d => d.LocalName == "r" && MatchesField(d.InnerText) && GetFieldName(d.InnerText) == FILE_NAME_EN_FIELD_NAME)
            ?.Parent;

        if (fileInfoParagraph is not null)
        {
            var fileInfoParent = fileInfoParagraph.Parent;
            foreach (var f in submission.Files.Where(f => f.FilePurpose == TbsOpenGovSubmission.DATASET_FILE_TYPE))
            {
                var fileInfo = fileInfoParagraph.CloneNode(true);
                var fileInfoReplacement = BuildListOfRecordsToReplace(fileInfo);
                var fileDetails = await GetSubmissionFileDetails(f);

                ReplaceFieldContent(fileInfoReplacement, fileDetails);
                fileInfoParent.InsertAfter(fileInfo, fileInfoParagraph);
            }
            fileInfoParent.RemoveChild(fileInfoParagraph);
        }
    }

    private async Task<Stream> CompleteDocument(Stream inputDoc, Dictionary<string, string> content, TbsOpenGovSubmission submission)
    {
        MemoryStream mem = new();

        inputDoc.CopyTo(mem);
        mem.Seek(0, SeekOrigin.Begin);

        using var document = WordprocessingDocument.Open(mem, true);

        var elementsToUpdate = BuildListOfRecordsToReplace(document.MainDocumentPart?.Document);
        ReplaceFieldContent(elementsToUpdate, content);

        await FillSubmissionFileDetails(document, submission);

        document.MainDocumentPart?.Document.Save();

        mem.Seek(0, SeekOrigin.Begin);

        return mem;
    }

    static Stream GetDocumentTemplateStream()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream("Datahub.Portal.Controllers.OpenDataApprovalForm.docx");
    }

    
    static bool MatchesField(string fieldName) => fieldName.StartsWith(TEMPLATE_PREFIX);
    static string GetFieldName(string fieldName) => fieldName[TEMPLATE_PREFIX.Length..];

    record ReplaceRecord(string FieldName, OpenXmlElement Element);
}