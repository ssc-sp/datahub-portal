using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs;
using System.Collections;
using System.Text;

namespace Datahub.Maui.Uploader
{
	internal class Uploader
    {
        public static async Task<bool> UploadBlocksAsync(
            BlobContainerClient blobContainerClient,
            string localFilePath,
            string targetName,
            int blockSize, int maxBlockSize, Func<double,Task> progressFunction, 
            Func<int, Task> updateBlockSize, CancellationToken cancellationToken)
        {

            var tgtSize = await CheckFileExistsAsync(blobContainerClient, targetName);
            if (tgtSize.HasValue)
            {
                var localFileSize = new FileInfo(localFilePath).Length;
                if (localFileSize == tgtSize.Value)
                {
                    //await progressFunction.Invoke(1.0);
                    return true;
                }
            }
            BlockBlobClient blobClient = blobContainerClient.GetBlockBlobClient(targetName);
            FileStream fileStream = File.OpenRead(localFilePath);
            ArrayList blockIDArrayList = new ArrayList();
            byte[] buffer;

            var bytesLeft = fileStream.Length - fileStream.Position;
            DateTime? lastBlock = null;
            while (bytesLeft > 0)
            {
                if (cancellationToken.IsCancellationRequested) { return false; }
                if (lastBlock.HasValue && DateTime.Now - lastBlock >  TimeSpan.FromSeconds(1))
                {
                    blockSize = Math.Max(blockSize / 2, 1 * 1000 * 1000);//don't go lower than 1Mib block
                    if (updateBlockSize != null)
                    {
                        await updateBlockSize.Invoke(blockSize);
                    }
                }
                if (lastBlock.HasValue && DateTime.Now - lastBlock < TimeSpan.FromSeconds(0.1))
                {
                    blockSize = Math.Min(blockSize * 2, maxBlockSize);//don't go higher than initial block size
                    if (updateBlockSize != null)
                    {
                        await updateBlockSize.Invoke(blockSize);
                    }
                }
                if (bytesLeft >= blockSize)
                {
                    buffer = new byte[blockSize];
                    await fileStream.ReadAsync(buffer, 0, blockSize);
                }
                else
                {
                    buffer = new byte[bytesLeft];
                    await fileStream.ReadAsync(buffer, 0, Convert.ToInt32(bytesLeft));
                    bytesLeft = fileStream.Length - fileStream.Position;
                }

                using (var stream = new MemoryStream(buffer))
                {
                    string blockID = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

                    blockIDArrayList.Add(blockID);
                    try
                    {
                        await blobClient.StageBlockAsync(blockID, stream, cancellationToken: cancellationToken);
                    } catch (Exception ex)
                    {
                        return false;
                    }
                }
                bytesLeft = fileStream.Length - fileStream.Position;
                if (progressFunction != null)
                    await progressFunction.Invoke(fileStream.Position * 1.0 / fileStream.Length);
                lastBlock = DateTime.Now;
            }

            string[] blockIDArray = (string[])blockIDArrayList.ToArray(typeof(string));

            await blobClient.CommitBlockListAsync(blockIDArray);
            return true;
        }

        public static async Task<long?> CheckFileExistsAsync(BlobContainerClient containerClient, string fileName)
        {
            try
            {

                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                var exists = await blobClient.ExistsAsync();

                if (exists)
                {
                    var properties = await blobClient.GetPropertiesAsync();
                    return properties.Value.ContentLength;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the check process.
                //Console.WriteLine($"Error checking the existence of the file: {ex.Message}");
                return null;
            }
        }
    }
}
