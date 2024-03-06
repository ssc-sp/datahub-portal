using System;

namespace Datahub.Metadata.Model;

public class CatalogObject
{
    public long CatalogObjectId { get; set; }
    public long ObjectMetadataId { get; set; }
    public virtual ObjectMetadata ObjectMetadata { get; set; }
    public MetadataObjectType DataType { get; set; }
    /// <summary>
    /// Name or title of the object (english)
    /// </summary>
    public string NameTXT { get; set; }
    /// <summary>
    /// Name or title of the object (french)
    /// </summary>
    public string NameFrenchTXT { get; set; }
    /// <summary>
    /// Location of the object (path, url, key, id, etc)
    /// </summary>
    public string LocationTXT { get; set; }
    /// <summary>
    /// Unclassified, Protect A, Protect B
    /// </summary>
    public string SecurityClassTXT { get; set; }
    /// <summary>
    /// Sector number
    /// </summary>
    public int SectorNUM { get; set; }
    /// <summary>
    /// Branch number
    /// </summary>
    public int BranchNUM { get; set; }
    /// <summary>
    /// Email, name or any way of contact with the cataloged object
    /// </summary>
    public string ContactTXT { get; set; }
    /// <summary>
    /// Search text English
    /// </summary>
    public string SearchEnglishTXT { get; set; }
    /// <summary>
    /// Search text French
    /// </summary>
    public string SearchFrenchTXT { get; set; }
    /// <summary>
    /// Dataset url localized in English
    /// </summary>
    public string UrlEnglishTXT { get; set; }
    /// <summary>
    /// Dataset url localized in French
    /// </summary>
    public string UrlFrenchTXT { get; set; }
    /// <summary>
    /// Language by default it is bilingual.
    /// </summary>
    public CatalogObjectLanguage Language { get; set; }
    /// <summary>
    /// ClassificationType: Unclassified, Protected A or Protected B
    /// </summary>
    public ClassificationType ClassificationType { get; set; }
    /// <summary>
    /// Grouping for linking catalog objects
    /// </summary>
    public Guid? GroupId { get; set; }
    /// <summary>
    /// Optional project id
    /// </summary>
    public int? ProjectId { get; set; }

    public CatalogObject Clone() => MemberwiseClone() as CatalogObject;
}