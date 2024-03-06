namespace Datahub.Core.Model.Catalog;

public class CatalogObject
{
    public int Id { get; set; }
    public CatalogObjectType ObjectType { get; set; }
    public string ObjectId { get; set; }
    public string NameEnglish { get; set; }
    public string NameFrench { get; set; }
    public string DescEnglish { get; set; }
    public string DescFrench { get; set; }
    public string Location { get; set; }
}

public enum CatalogObjectType
{
    User,
    Workspace,
    Repository
}