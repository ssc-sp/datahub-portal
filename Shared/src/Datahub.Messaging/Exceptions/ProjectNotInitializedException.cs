namespace ResourceProvisioner.Domain.Exceptions;

public class ProjectNotInitializedException : Exception
{
    public ProjectNotInitializedException(string message) : base(message)
    {
    }
}