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
    public string Name_TXT { get; set; }
    /// <summary>
    /// Name or title of the object (french)
    /// </summary>
    public string Name_French_TXT { get; set; }
    /// <summary>
    /// Location of the object (path, url, key, id, etc)
    /// </summary>
    public string Location_TXT { get; set; }
    /// <summary>
    /// Unclassified, Protect A, Protect B
    /// </summary>
    public string SecurityClass_TXT { get; set; }
    /// <summary>
    /// Sector number
    /// </summary>
    public int Sector_NUM { get; set; }
    /// <summary>
    /// Branch number
    /// </summary>
    public int Branch_NUM { get; set; }
    /// <summary>
    /// Email, name or any way of contact with the cataloged object
    /// </summary>
    public string Contact_TXT { get; set; }
    /// <summary>
    /// Search text English
    /// </summary>
    public string Search_English_TXT { get; set; }
    /// <summary>
    /// Search text French
    /// </summary>
    public string Search_French_TXT { get; set; }
    /// <summary>
    /// Dataset url localized in English
    /// </summary>
    public string Url_English_TXT { get; set; }
    /// <summary>
    /// Dataset url localized in French
    /// </summary>
    public string Url_French_TXT { get; set; }
    /// <summary>
    /// Language by default it is bilingual.
    /// </summary>
    public CatalogObjectLanguage Language { get; set; }
    /// <summary>
    /// ClassificationType: Unclassified, Protected A or Protected B
    /// </summary>
    public ClassificationType Classification_Type { get; set; }
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