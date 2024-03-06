using System.ComponentModel.DataAnnotations;

namespace Datahub.Portal.Model;

public class MetadataField : Attribute
{
}

public class OpenDataShareRequest
{
    [Required]
    public string fileUrl { get; set; }
    [Required]
    public string fileId { get; set; }
    [Required]
    public string fileName { get; set; }
    [Required]
    public string emailContact { get; set; }
    [Required]
    [MetadataField]
    public string collection { get; set; }
    [Required]
    [MetadataField]
    public string titleTranslatedEn { get; set; }
    [Required]
    [MetadataField]
    public string titleTranslatedFr { get; set; }
    [Required]
    [MetadataField]
    public string notesTranslatedEn { get; set; }
    [Required]
    [MetadataField]
    public string notesTranslatedFr { get; set; }
    [Required]
    [MetadataField]
    public string keywordsEn { get; set; }
    [Required]
    [MetadataField]
    public string keywordsFr { get; set; }
    [Required]
    [MetadataField]
    public string subject { get; set; }
    [Required]
    [MetadataField]
    public string frequency { get; set; }
    [Required]
    [MetadataField]
    public string datePublished { get; set; }
    [Required]
    [MetadataField]
    public string jurisdiction { get; set; }

    [MetadataField]
    public string timePeriodCoverageStart { get; set; }
    [MetadataField]
    public string timePeriodCoverageEnd { get; set; }
    [MetadataField]
    public string audience { get; set; }
    [MetadataField]
    public string digitalObjectIdentifier { get; set; }
}