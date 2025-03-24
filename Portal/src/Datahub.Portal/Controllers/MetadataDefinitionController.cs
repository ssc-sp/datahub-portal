using Datahub.Application.Services.Metadata;
using Datahub.Metadata.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;

namespace Datahub.Portal.Controllers;

[ApiController]
public class MetadataDefinitionController(
    IMetadataBrokerService metadataBrokerService, 
    ILoggerFactory loggerFactory) : Controller
{
    private readonly IMetadataBrokerService _metadataBrokerService = metadataBrokerService;
    private readonly ILogger<MetadataDefinitionController> _logger = loggerFactory.CreateLogger<MetadataDefinitionController>();

    private static readonly string EXPORT_FILENAME = "metadataDefinitions.json";

    /// <summary>
    /// Returns a JSON containing metadata field definitions and profiles
    /// </summary>
    /// <returns></returns>
    [HttpGet("metadata/definitions/get")]
    [Authorize]
    public async Task<IActionResult> GetMetadataDefinitions()
    {
        try
        {
            var profiles = await _metadataBrokerService.GetProfiles();
            var defs = await _metadataBrokerService.GetFieldDefinitions();
            var metadataDto = MetadataDTO.Create(profiles, defs.Fields);

            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream, Encoding.UTF8);
            var jsonWriter = new JsonTextWriter(streamWriter);

            var serializer = new JsonSerializer();
            serializer.Serialize(jsonWriter, metadataDto);
            await jsonWriter.FlushAsync();
            stream.Seek(0, SeekOrigin.Begin);

            var outputStream = Stream.Synchronized(stream);
            
            return File(outputStream, MediaTypeNames.Application.Json, EXPORT_FILENAME);
        } 
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting metadata definitions");
            return NotFound();
        }
    }

}