using Datahub.CKAN.Package;
using Datahub.Metadata.DTO;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Datahub.CKAN.Service
{
    public class CKANService : ICKANService
    {
        #region Static CKAN data
        private static HashSet<string> KnownFileTypes { get; }

        static CKANService()
        {
            KnownFileTypes = new HashSet<string>()
            {
                "AAC", "AIFF", "APK", "AVI", "BAG", "BMP", "BWF", "CCT", "CDF", "CDR", "COD", "CSV", "DBD", "DBF", "DICOM", 
                "DNG", "DOC", "DOCX", "DXF", "E00", "ECW", "EDI", "EMF", "EPUB3", "EPUB2", "EPS", "ESRI REST", "EXE", "FITS", 
                "GDB", "GEOPDF", "GEORSS", "GEOTIF", "GEOJSON", "GPKG", "GIF", "GML", "GRD", "GRIB1", "GRIB2", "HDF", "HTML", 
                "IATI", "IPA", "JAR", "JFIF", "JP2", "JPG", "JSON", "JSONLD", "JSONL", "KML", "KMZ", "LAS", "LYR", "TAB", 
                "MFX", "MOV", "MPEG", "MPEG-1", "MP3", "MXD", "NETCDF", "ODP", "ODS", "ODT", "PDF", "PNG", "PPT", 
                "PPTX", "RDF", "TTL", "NT", "RDFA", "RSS", "RTF", "SAR", "SAS", "SAV", "SEGY", "SHP", "SQL", "SQLITE3", "SQLITE", 
                "SVG", "TIFF", "TRIG", "TRIX", "TFW", "TXT", "VPF", "WAV", "WCS", "WFS", "WMS", "WMTS", "WMV", "WPS", "XML", "XLS", 
                "XLSM", "XLSX", "ZIP"
            };
        }
        #endregion

        readonly HttpClient _httpClient;
        readonly CKANConfiguration _ckanConfiguration;

        public CKANService(HttpClient httpClient, IOptions<CKANConfiguration> ckanConfiguration)
        {
            _httpClient = httpClient;
            _ckanConfiguration = ckanConfiguration.Value;
        }

        public async Task<CKANApiResult> CreatePackage(FieldValueContainer fieldValues, string url)
        {
            // generate the dictionary
            var packageData = (new PackageGenerator()).GeneratePackage(fieldValues, url);

            // generate json from package
            var jsonData = JsonSerializer.Serialize(packageData, GetSerializationOptions());

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            return await PostRequestAsync("package_create", content);
        }

        public async Task<CKANApiResult> AddResourcePackage(string packageId, string fileName, Stream fileData)
        {
            var boundary = System.Guid.NewGuid().ToString();
            using var content = new MultipartFormDataContent(boundary);

            content.Headers.ContentType.MediaType = "multipart/form-data";

            content.Add(new StringContent(packageId), "package_id");
            content.Add(new StringContent(fileName), "name_translated-en");
            content.Add(new StringContent(fileName), "name_translated-fr");
            content.Add(new StringContent("en"), "language");
            content.Add(new StringContent("dataset"), "resource_type");
            content.Add(new StringContent(GetFileFormat(fileName, "other")), "format");
            content.Add(new StreamContent(fileData), "upload", fileName);

            return await PostRequestAsync("resource_create", content);
        }

        private async Task<CKANApiResult> PostRequestAsync(string action, HttpContent content)
        {
            try
            {
                var baseUrl = _ckanConfiguration.BaseUrl;
                var apiKey = _ckanConfiguration.ApiKey;

                content.Headers.Add("X-CKAN-API-Key", apiKey);

                var response = await _httpClient.PostAsync($"{baseUrl}/{action}", content);
                //response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var ckanResult = JsonSerializer.Deserialize<CKANResult>(jsonResponse, GetSerializationOptions());

                var errorMessage = ckanResult.Success ? string.Empty : ckanResult.Error?.__type;
                return new CKANApiResult(ckanResult.Success, errorMessage);
            }
            catch (Exception ex)
            {
                return new CKANApiResult(false, ex.Message);
            }
        }

        private string GetFileFormat(string fileName, string defaultFormat)
        {
            var fileExt = (Path.GetExtension(fileName) ?? ".").Substring(1).ToUpper();
            return KnownFileTypes.Contains(fileExt) ? fileExt : defaultFormat;
        }

        static JsonSerializerOptions GetSerializationOptions() => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
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
}
