using Xunit;

namespace ResourceProvisioner.SpecflowTests.Collections
{
    /// <summary>
    /// Collection definition for tests that access repositories and file system operations.
    /// Tests in this collection cannot run in parallel to avoid file system conflicts.
    /// </summary>
    [CollectionDefinition("RepositoryAccess", DisableParallelization = true)]
    public class RepositoryAccessCollection : ICollectionFixture<RepositoryAccessCollection>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}