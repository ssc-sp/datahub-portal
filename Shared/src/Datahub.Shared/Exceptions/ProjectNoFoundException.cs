using System;

namespace Datahub.Shared.Exceptions;

public class ProjectNoFoundException : Exception
{
    public ProjectNoFoundException(string message) : base(message)
    {
    }
}