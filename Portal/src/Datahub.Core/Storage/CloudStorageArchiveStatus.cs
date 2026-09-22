namespace Datahub.Core.Storage
{
    public enum CloudStorageArchiveState
    {
        Available,
        RestoreRequired,
        Restoring
    }

    public sealed record CloudStorageArchiveStatus(CloudStorageArchiveState State, DateTime? RestoredCopyExpiry = null);

    public sealed class StorageTierChangeException : Exception
    {
        public StorageTierChangeException(string message) : base(message) { }
    }
}
