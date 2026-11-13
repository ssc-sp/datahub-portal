namespace Datahub.Application.Exceptions;

/// <summary>
/// Thrown when a Protected B workspace is requested but no subnet group is available to assign to it.
/// </summary>
public class NoSubnetGroupAvailableException : Exception
{
    public NoSubnetGroupAvailableException() : base() { }
    public NoSubnetGroupAvailableException(string? message) : base(message) { }
    public NoSubnetGroupAvailableException(string? message, Exception? innerException) : base(message, innerException) { }
}
