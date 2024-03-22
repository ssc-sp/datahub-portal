namespace Datahub.Shared.Exceptions;

public class ProjectNotFoundException : Exception
{
    public ProjectNotFoundException(string message) : base(message)
    {
    }
}