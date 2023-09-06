namespace Datahub.Core.Model.Catalog;

public class CatalogObject
{
    public int Id { get; set; }
    public CatalogObjectType ObjectType { get; set; }
    public string ObjectId { get; set; }
    public string Name_English { get; set; }
    public string Name_French { get; set; }
    public string Desc_English { get; set; }
    public string Desc_French { get; set; }
    public string Location { get; set; }
}

public enum CatalogObjectType
{
    User,
    Workspace,
    Repository
}