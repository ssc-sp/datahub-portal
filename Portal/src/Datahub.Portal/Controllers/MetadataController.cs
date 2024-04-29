using Datahub.Metadata.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Datahub.Application.Services.Metadata;

namespace Datahub.Portal.Controllers
{
    [Route("[controller]")]
    [AllowAnonymous]
    public class MetadataController : Controller
    {
        private readonly ILogger<MetadataController> _logger;
        private readonly IMetadataBrokerService _metadataBrokerService;

        public MetadataController(ILogger<MetadataController> logger, IMetadataBrokerService metadataBrokerService)
        {
            _logger = logger;
            _metadataBrokerService = metadataBrokerService;
        }
        
        [HttpGet]
        [Route("definitions/get")]
        public async Task<IActionResult> GetMetadataDefinitions()
        {
            var profiles = await _metadataBrokerService.GetProfiles();
            var fieldDefinitions = await _metadataBrokerService.GetFieldDefinitions();

            var metadataDto = MetadataDTO.Create(profiles, fieldDefinitions.Fields);
            JsonSerializerOptions options = new() { WriteIndented = true };

            return new JsonResult(metadataDto, options);
        }
    }
}
