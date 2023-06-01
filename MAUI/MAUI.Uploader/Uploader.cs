using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Maui.Uploader
{
    internal class Uploader
    {
        public static async Task UploadBlocksAsync(
            BlobContainerClient blobContainerClient,
            string localFilePath,
            int blockSize, Func<double,Task> progressFunction)
        {
            string fileName = Path.GetFileName(localFilePath);
            BlockBlobClient blobClient = blobContainerClient.GetBlockBlobClient(fileName);

            FileStream fileStream = File.OpenRead(localFilePath);
            ArrayList blockIDArrayList = new ArrayList();
            byte[] buffer;

            var bytesLeft = (fileStream.Length - fileStream.Position);

            while (bytesLeft > 0)
            {
                if (bytesLeft >= blockSize)
                {
                    buffer = new byte[blockSize];
                    await fileStream.ReadAsync(buffer, 0, blockSize);
                }
                else
                {
                    buffer = new byte[bytesLeft];
                    await fileStream.ReadAsync(buffer, 0, Convert.ToInt32(bytesLeft));
                    bytesLeft = (fileStream.Length - fileStream.Position);
                }

                using (var stream = new MemoryStream(buffer))
                {
                    string blockID = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

                    blockIDArrayList.Add(blockID);
                    await blobClient.StageBlockAsync(blockID, stream);
                }
                bytesLeft = (fileStream.Length - fileStream.Position);
                if (progressFunction != null)
                    await progressFunction.Invoke(fileStream.Position * 1.0 / fileStream.Length);
            }

            string[] blockIDArray = (string[])blockIDArrayList.ToArray(typeof(string));

            await blobClient.CommitBlockListAsync(blockIDArray);
        }
    }
}
