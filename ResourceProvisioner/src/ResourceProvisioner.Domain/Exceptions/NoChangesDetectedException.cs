namespace ResourceProvisioner.Domain.Exceptions;

public class NoChangesDetectedException : Exception
{
    public NoChangesDetectedException(string message) : base(message)
    {
    }
}