using Datahub.Application.Configuration;
using Datahub.Application.Services.Publishing;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Publishing.Package;
using Datahub.Metadata.DTO;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Datahub.Infrastructure.Services.Publishing;

public class CKANService : ICKANService
{
    #region Static CKAN data
    private const string PACKAGE_CREATE_ACTION = "package_create";
    private const string PACKAGE_SHOW_ACTION = "package_show";
    private const string PACKAGE_PATCH_ACTION = "package_patch";
    private const string RESOURCE_CREATE_ACTION = "resource_create";

    private const string HTTP_CLIENT_NAME = "CkanService";

    private static HashSet<string> KnownFileTypes { get; }

    static CKANService()
    {
        KnownFileTypes =
        [
            "AAC", "AIFF", "APK", "AVI", "BAG", "BMP", "BWF", "CCT", "CDF", "CDR", "COD", "CSV", "DBD", "DBF", "DICOM", 
            "DNG", "DOC", "DOCX", "DXF", "E00", "ECW", "EDI", "EMF", "EPUB3", "EPUB2", "EPS", "ESRI REST", "EXE", "FITS", 
            "GDB", "GEOPDF", "GEORSS", "GEOTIF", "GEOJSON", "GPKG", "GIF", "GML", "GRD", "GRIB1", "GRIB2", "HDF", "HTML", 
            "IATI", "IPA", "JAR", "JFIF", "JP2", "JPG", "JSON", "JSONLD", "JSONL", "KML", "KMZ", "LAS", "LYR", "TAB", 
            "MFX", "MOV", "MPEG", "MPEG-1", "MP3", "MXD", "NETCDF", "ODP", "ODS", "ODT", "PDF", "PNG", "PPT", 
            "PPTX", "RDF", "TTL", "NT", "RDFA", "RSS", "RTF", "SAR", "SAS", "SAV", "SEGY", "SHP", "SQL", "SQLITE3", "SQLITE", 
            "SVG", "TIFF", "TRIG", "TRIX", "TFW", "TXT", "VPF", "WAV", "WCS", "WFS", "WMS", "WMTS", "WMV", "WPS", "XML", "XLS", 
            "XLSM", "XLSX", "ZIP"
        ];
    }

    private static string GetFileFormat(string fileName, string defaultFormat)
    {
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext))
        {
            return defaultFormat;
        }

        var fileExt = ext[1..].ToUpper();
        return KnownFileTypes.Contains(fileExt) ? fileExt : defaultFormat;
    }

    private static CKANApiResult DeserializePackageResult(CKANApiResult result)
    {
        if (result.Succeeded)
        {
            var ckanPackageJson = result.CkanObject?.ToString();
            if (!string.IsNullOrEmpty(ckanPackageJson))
            {
                return new CKANApiResult(result.Succeeded, result.ErrorMessage, JsonSerializer.Deserialize<CkanPackageBasic>(ckanPackageJson, SerializationOptions));
            }
        }

        return result;
    }

    public static string NewMultipartBoundary() => $"------ {DateTime.Now.Ticks:x} --------";
    
    private static CkanPackageBasic CreateTestPackageBasic(string id) => new()
    {
        Id = id,
        Name = "Test",
        State = "active",
        ImsoApproval = "false",
        ReadyToPublish = "false"
    };

    static readonly JsonSerializerOptions SerializationOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    // initial version only uses "dataset" and "guide" types; if multi-word types are added, they will need to be
    // converted to snake_case. e.g. "GeospatialMaterial" => "geospatial_material"
    static string TransformResourceType(string resourceType) => resourceType?.ToLowerInvariant();
    #endregion

    readonly IHttpClientFactory _httpClientFactory;
    readonly CkanConfiguration _ckanConfiguration;
    readonly string _apiKey;

    public CKANService(IHttpClientFactory httpClientFactory, CkanConfiguration ckanConfiguration, string? apiKey = null)
    {
        _httpClientFactory = httpClientFactory;
        _ckanConfiguration = ckanConfiguration;
        _apiKey = string.IsNullOrEmpty(apiKey) ? _ckanConfiguration.ApiKey : apiKey;
    }

    public static ICKANService CreateService(IHttpClientFactory httpClientFactory, CkanConfiguration ckanConfiguration, string? apiKey = null) => new CKANService(httpClientFactory, ckanConfiguration, apiKey);

    private bool IsTestMode => _ckanConfiguration.TestMode;

    public async Task<CKANApiResult> CreatePackage(FieldValueContainer fieldValues, bool allFields, string url)
    {
        // generate the dictionary
        var packageData = new PackageGenerator().GeneratePackage(fieldValues, allFields, url);

        // generate json from package
        var jsonData = JsonSerializer.Serialize(packageData, SerializationOptions);

        var content = new StringContent(jsonData, Encoding.UTF8, MediaTypeNames.Application.Json);

        var result = await PostRequestAsync(PACKAGE_CREATE_ACTION, content);

        return result;
    }

    private async Task<CKANApiResult> FetchPackage(string packageId)
    {
        var parameters = new Dictionary<string, object>
        {
            ["id"] = packageId
        };
        
        return await DoRequestAsync(HttpMethod.Get, PACKAGE_SHOW_ACTION, null, parameters);
    }

    public async Task<CKANApiResult> CreateOrFetchPackage(FieldValueContainer fieldValues, bool allFields)
    {
        if (IsTestMode)
        {
            return new CKANApiResult(true, string.Empty, CreateTestPackageBasic(fieldValues.ObjectId));
        }

        var result = await FetchPackage(fieldValues.ObjectId);

        if (!result.Succeeded)
        {
            result = await CreatePackage(fieldValues, allFields, null);
        }

        return DeserializePackageResult(result);
    }

    public async Task<CKANApiResult> UpdatePackageAttributes(string packageId, IDictionary<string, string> attributes)
    {
        var boundary = NewMultipartBoundary();
        using var content = new MultipartFormDataContent(boundary)
        {
            { new StringContent(packageId), "id" }
        };

        foreach (var kvp in attributes)
        {
            content.Add(new StringContent(kvp.Value), kvp.Key);
        }

        var result = await DoRequestAsync(HttpMethod.Post, PACKAGE_PATCH_ACTION, content);

        return DeserializePackageResult(result);
    }

    public async Task<CKANApiResult> AddResourcePackage(string packageId, string filename, string filePurpose, FieldValueContainer metadata, Stream fileContentStream, long? contentLength = null)
    {
        var nameEn = metadata["name_translated_en"]?.Value_TXT ?? filename;
        var nameFr = metadata["name_translated_fr"]?.Value_TXT ?? filename;
        var fileFormat = GetFileFormat(filename, "other");
        var languages = (metadata["resource_language"]?.Value_TXT ?? "en|fr").Split("|");
        var resourceType = TransformResourceType(filePurpose);

        var streamContent = new StreamContent(fileContentStream);
        streamContent.Headers.ContentLength = contentLength;


        var boundary = NewMultipartBoundary();
        using var content = new MultipartFormDataContent(boundary)
        {
            { new StringContent(packageId), "package_id" },
            { new StringContent(nameEn), "name_translated-en" },
            { new StringContent(nameFr), "name_translated-fr" },
            { new StringContent(resourceType), "resource_type" },
            { new StringContent(fileFormat), "format" }
        };
        
        foreach (var lang in languages)
        {
            content.Add(new StringContent(lang), "language");
        }

        content.Add(streamContent, "upload", filename);

        // TODO - find a way to get the upload sending successfully with stream-to-stream
        // instead of serializing the entire payload into memory
        await content.ReadAsStringAsync();

        var result = await DoRequestAsync(HttpMethod.Post, RESOURCE_CREATE_ACTION, content);

        return result;
    }

    private static string GetCkanErrorMessage(CKANResult result)
    {
        if (result.Success)
        {
            return string.Empty;
        }
        else
        {
            string[] parts =
            [
                result.Error?.__type,
                result.Error?.Message
            ];

            return string.Join(" - ", parts.Where(s => !string.IsNullOrEmpty(s)));
        }
    }

    private string CreateActionUrl(string action) => $"{_ckanConfiguration.ApiUrl}/action/{action}";

    #nullable enable
    private async Task<CKANApiResult> DoRequestAsync(HttpMethod method, string action, HttpContent? content = null, Dictionary<string,object>? parameters = null)
    {
        var httpClient = _httpClientFactory.CreateClient(HTTP_CLIENT_NAME);

        try
        {
            // this is to avoid developing on the VPN (test mode should be off in prod)
            if (IsTestMode) 
            {
                return new CKANApiResult(true, string.Empty);
            }

            var baseUrl = CreateActionUrl(action);
            var apiKey = _apiKey;

            var requestUri = new UriBuilder(baseUrl);
            if (parameters != null) 
            {
                var queryParams = HttpUtility.ParseQueryString(requestUri.Query);
                foreach(var k in parameters.Keys)
                {
                    queryParams[k] = parameters[k]?.ToString() ?? string.Empty;
                }
                requestUri.Query = queryParams.ToString();
            }

            using var httpRequest = new HttpRequestMessage(method, requestUri.Uri)
            {
                Content = content
            };
            httpRequest.Headers.Add("X-CKAN-API-Key", apiKey);

            using var response = await httpClient.SendAsync(httpRequest);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var ckanResult = JsonSerializer.Deserialize<CKANResult>(jsonResponse, SerializationOptions);

            if (ckanResult == null)
            {
                return new CKANApiResult(false, "Invalid response from API");
            }

            var errorMessage = GetCkanErrorMessage(ckanResult);
            return new CKANApiResult(ckanResult.Success, errorMessage, ckanResult.Result);
        }
        catch (Exception ex)
        {
            return new CKANApiResult(false, ex.Message);
        }
    }
    #nullable restore

    // TODO: Remove this
    public async Task<CKANApiResult> AddResourcePackageOld(string packageId, string fileName, Stream fileData, long? contentLength = null)
    {
        var boundary = NewMultipartBoundary();

        var streamContent = new StreamContent(fileData);
        if (contentLength.HasValue)
        {
            streamContent.Headers.ContentLength = contentLength.Value;
        }

        using var content = new MultipartFormDataContent(boundary)
        {
            { new StringContent(packageId), "package_id" },
            { new StringContent(fileName), "name_translated-en" },
            { new StringContent(fileName), "name_translated-fr" },
            { new StringContent("en"), "language" },
            { new StringContent("dataset"), "resource_type" },
            { new StringContent(GetFileFormat(fileName, "other")), "format" },
            { streamContent, "upload", fileName }
        };
        
        return await PostRequestAsync("resource_create", content);
    }

    public async Task<CKANApiResult> DeletePackage(string packageId)
    {
        // generate the dictionary with the package id
        Dictionary<string, object> packageData = new() { { "id", packageId } };

        // generate json from package
        var jsonData = JsonSerializer.Serialize(packageData, SerializationOptions);

        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        return await PostRequestAsync("package_delete", content);
    }

    private async Task<CKANApiResult> PostRequestAsync(string action, HttpContent content) => await DoRequestAsync(HttpMethod.Post, action, content);
}

class CKANResult
{
    public string Help { get; set; }
    public bool Success { get; set; }
    public object Result {  get; set; }
    public CKANError Error { get; set; }
}

class CKANError
{
    public string Message { get; set; }
    public string __type { get; set; }
}