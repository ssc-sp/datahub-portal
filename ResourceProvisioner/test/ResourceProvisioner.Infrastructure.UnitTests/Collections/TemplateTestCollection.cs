using NUnit.Framework;

namespace ResourceProvisioner.Infrastructure.UnitTests.Collections
{
    /// <summary>
    /// Collection definition for template tests that perform file system operations.
    /// Tests in this collection cannot run in parallel to avoid file system conflicts.
    /// </summary>
    [NonParallelizable]
    [Category("TemplateTests")]
    public class TemplateTestCollection
    {
        private static readonly object _lock = new object();
        
        /// <summary>
        /// Ensures that directory cleanup is thread-safe across all template tests
        /// </summary>
        protected void SafeCleanup(string path)
        {
            lock (_lock)
            {
                Testing.VerifyDirectoryDoesNotExist(path);
            }
        }
        
        /// <summary>
        /// Provides thread-safe access to file system operations
        /// </summary>
        protected T ExecuteWithLock<T>(Func<T> operation)
        {
            lock (_lock)
            {
                return operation();
            }
        }
        
        /// <summary>
        /// Provides thread-safe access to async file system operations
        /// </summary>
        protected async Task<T> ExecuteWithLockAsync<T>(Func<Task<T>> operation)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    return operation();
                }
            });
            return await operation();
        }
    }
}