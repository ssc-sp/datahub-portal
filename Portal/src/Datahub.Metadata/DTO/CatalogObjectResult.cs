using Datahub.Metadata.Model;
using System;

namespace Datahub.Metadata.DTO;

public class CatalogObjectResult
{
    public long CatalogObjectId { get; set; }
    public long ObjectMetadataId { get; set; }
    public string MetadataObjectIdTXT { get; set; }
    public MetadataObjectType DataType { get; set; }
    public string NameEnglish { get; set; }
    public string NameFrench { get; set; }
    public string Location { get; set; }
    public int Sector { get; set; }
    public int Branch { get; set; }
    public string Contact { get; set; }
    public ClassificationType ClassificationType { get; set; }
    public string UrlEnglish { get; set; }
    public string UrlFrench { get; set; }
    public CatalogObjectLanguage Language { get; init; }
    public bool IsCatalogComplete { get; set; } = true;
    public bool IsFrench { get; set; }
    public Guid? GroupId { get; set; }
    public int? ProjectId { get; set; }
    public FieldValueContainer Metadata { get; set; }
}