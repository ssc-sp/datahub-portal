namespace ResourceProvisioner.Domain.Entities;

public class Workspace
{
    public string Name { get; set; }
    public string Acronym { get; set; }
    public Organization Organization { get; set; }
    public List<User> Users { get; set; }
    
}