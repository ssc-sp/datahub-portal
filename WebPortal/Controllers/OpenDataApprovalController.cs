using Datahub.Metadata.Model;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Datahub.Portal.Controllers
{
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
                { "Info", GetCheckBox("Info" == form.Type_Of_Data_TXT) }
            };
        }

        static string GetCheckBox(bool value) => value ? "☒" : "☐";

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
}
