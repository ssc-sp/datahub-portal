using Datahub.Metadata.Model;

namespace Datahub.Metadata.Utils;

/// <summary>
/// Stores a catalog digest as a DTO record,
/// extracted from the metadata of an object.
/// </summary>
public record CatalogDigest
(
    string TitleEnglish, 
    string TitleFrench, 
    string Contact, 
    int Sector, 
    int Branch, 
    string EnglishCatalog, 
    string FrenchCatalog, 
    ClassificationType Classification
);