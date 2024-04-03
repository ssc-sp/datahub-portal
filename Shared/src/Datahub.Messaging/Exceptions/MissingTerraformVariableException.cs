namespace ResourceProvisioner.Domain.Exceptions;

public class MissingTerraformVariableException : Exception
{
    public MissingTerraformVariableException(string message) : base(message)
    {
    }
}