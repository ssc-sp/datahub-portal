using BlazorDownloadFile;

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NRCan.Datahub.Portal.Services;
using NRCan.Datahub.Shared.Data;
using NRCan.Datahub.Shared.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Tewr.Blazor.FileReader;
using Xunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.JSInterop;
using System.Threading;
using Azure.Storage.Files.DataLake;
using Azure.Storage;
using System.IO;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Files.DataLake.Models;

using System.Linq;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Indexes;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using NRCan.Datahub.Portal.EFCore;
using NRCan.Datahub.Shared.EFCore;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DatahubTest
{
    public class Metadata
    {
        public string folderowner { get; set; }
        public string folderid { get; set; }
        public string createdby { get; set; }
        public string lastmodifiedby { get; set; }
        public string filename { get; set; }
        public string fileformat { get; set; }
        public string securityclass { get; set; }
        public string ownedby { get; set; }
    }

    public class MyArray
    {
        public string versionid { get; set; }
        public Metadata metadata { get; set; }
        public string timestamp { get; set; }
    }

    public class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ContentRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string EnvironmentName { get => "Development"; set => throw new NotImplementedException(); }
    }

    public class FakeJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args)
        {
            throw new NotImplementedException();
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args)
        {
            throw new NotImplementedException();
        }
    }
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IUserInformationService, OfflineUserInformationService>();
            services.AddSingleton<IMSGraphService, MSGraphService>();
            services.AddSingleton<IKeyVaultService, KeyVaultService>();
            services.AddScoped<DataLakeClientService>();
            services.AddSingleton<CognitiveSearchService>();
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<ApiCallService>();
            services.AddScoped<IWebHostEnvironment, FakeWebHostEnvironment>();
            services.AddScoped<IJSRuntime, FakeJSRuntime>();
            //services.AddScoped<DataUpdatingService>();
            //services.AddScoped<DataSharingService>();
            services.AddScoped<DataCreatorService>();
            //services.AddScoped<DataRetrievalService>();
            //services.AddScoped<DataRemovalService>();
            services.AddSingleton<DatahubTools>();
            services.AddScoped<NotificationsService>();
            services.AddScoped<UIControlsService>();
            services.AddHttpClient();
            services.AddFileReaderService();
            services.AddBlazorDownloadFile();
            services.AddScoped<NotifierService>();

        }

    }
    public class APIServiceTest
    {

        private readonly IApiService _apiService;
        //public APIServiceTest(IApiService ApiService)
        //{
        //    _apiService = ApiService;
        //}


        [Fact]
        public void ParseCustomFieldsToJson()
        {

            List<Customfield> customFields = new List<Customfield>();
            Customfield field = new Customfield() { key = "key1", value = "value1" };
            customFields.Add(field);
            field = new Customfield() { key = "key2", value = "value2" };
            customFields.Add(field);


            var json = JsonConvert.SerializeObject(customFields);
            Assert.True(json == @"[{""key"":""key1"",""value"":""value1""},{""key"":""key2"",""value"":""value2""}]");
        }

        [Fact]
        public void GivenVersionJSONresponse_ThenParseCorrectlyToClass()
        {
            string json = @"[{""versionid"":""2020 - 11 - 16T17: 45:43.9741390Z"",""metadata"":{ ""folderowner"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""folderid"":""ownedroot - 0403528c - 5abc - 423f - 9201 - 9c945f628595"",""createdby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""lastmodifiedby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""filename"":""ResXManager.VSIX.vsix"",""fileformat"":""vsix"",""securityclass"":""Unclassified"",""ownedby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595""},""timestamp"":""2020 - 11 - 16T17: 45:43 + 00:00""},{""versionid"":""2020 - 11 - 16T17: 46:14.6966275Z"",""metadata"":{ ""folderowner"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""folderid"":""ownedroot - 0403528c - 5abc - 423f - 9201 - 9c945f628595"",""createdby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""lastmodifiedby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""filename"":""ResXManager.VSIX.vsix"",""fileformat"":""vsix"",""securityclass"":""Protected A"",""ownedby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595""},""timestamp"":""2020 - 11 - 16T17: 46:14 + 00:00""},{""versionid"":""2020 - 11 - 16T20: 16:56.4849377Z"",""metadata"":{ ""folderowner"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""folderid"":""ownedroot - 0403528c - 5abc - 423f - 9201 - 9c945f628595"",""createdby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""lastmodifiedby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""filename"":""ResXManager.VSIX.vsix"",""fileformat"":""vsix"",""securityclass"":""Unclassified"",""ownedby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595""},""timestamp"":""2020 - 11 - 16T20: 16:56 + 00:00""},{""versionid"":""2020 - 11 - 17T18: 51:07.7526279Z"",""metadata"":{ ""folderowner"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""folderid"":""ownedroot - 0403528c - 5abc - 423f - 9201 - 9c945f628595"",""createdby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""lastmodifiedby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595"",""filename"":""ResXManager.VSIX.vsix"",""fileformat"":""vsix"",""securityclass"":""Unclassified"",""ownedby"":""0403528c - 5abc - 423f - 9201 - 9c945f628595""},""timestamp"":""2020 - 11 - 17T18: 51:07 + 00:00""}]";
            var versions = JsonConvert.DeserializeObject<List<NRCan.Datahub.Shared.Data.Version>>(json);

            Assert.True(versions.Count > 0);
        }

        [Fact]
        public void GivenGen2URL_GenerateABFSUri()
        {
             var fileSystemName = "datahub";
            var accountName = "datahubdatalakedev";
            var folderpath = "NRCan-RNCan.gc.ca/nabeel.bader";
            var filename = "favicon-192.png";

            string uri = $"abfs://{fileSystemName}@{accountName}.dfs.core.windows.net/{folderpath}/{filename}";

            Assert.True(uri == "abfs://datahub@datahubdatalakedev.dfs.core.windows.net/NRCan-RNCan.gc.ca/nabeel.bader/favicon-192.png");
        }

        [Fact]
        public void SerializeVersions()
        {
            Metadata metadata = new Metadata();
            metadata.createdby = "nab";
            metadata.fileformat = "pdf";
            metadata.filename = "test.pdf";
            metadata.folderid = "1234566";
            metadata.folderowner = "nab";
            metadata.lastmodifiedby = "nab";
            metadata.ownedby = "nab";
            metadata.securityclass = "unclassified";

            MyArray myArray = new MyArray();
            myArray.metadata = metadata;
            myArray.timestamp = DateTime.Now.ToString();
            myArray.versionid = string.Empty;

            var json = JsonConvert.SerializeObject(myArray);
            Assert.NotNull(json);
        }

        string storageAccountName = "datahubdatalakedev";
        string storageAccountKey = @"3GURb30PvhqD3L4aD+27fDxhNmde5oY0kpu5G0imTMdgwExq9MafQOgpWDnElgLQFHjpY6tkekf28SAdIjiSNQ==";
        string fileSystemName = "datahub";
        string userId = "0403528c-5abc-423f-9201-9c945f628595";
        string storageAccountNameFlat = "dhcanmetrobodev";
        string storageAccountKeyFlat = @"FvGP17Hc8RlR5ztEjmwafUU/MFmILU8v5f+JQOf9bW+QZWYRoayUMyX38XxNrLbbICwrWnLLIGPlXi/b60gnBQ==";
        string cxnstring = @"DefaultEndpointsProtocol=https;AccountName=dhcanmetrobodev;AccountKey=FvGP17Hc8RlR5ztEjmwafUU/MFmILU8v5f+JQOf9bW+QZWYRoayUMyX38XxNrLbbICwrWnLLIGPlXi/b60gnBQ==;EndpointSuffix=core.windows.net";


        [Fact]
        public async Task ListFilesInGen2Flat()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(cxnstring);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(fileSystemName);


            Assert.NotNull(containerClient);

            var resultSegment = containerClient.GetBlobsAsync()
            .AsPages(default, 30);

            // Enumerate the blobs returned for each page.
            await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    Console.WriteLine("Blob name: {0}", blobItem.Name);
                }

                Console.WriteLine();
            }

        }

       


        [Fact]
        public async Task ConnectToGen2SAFlat()
        {
            
            var sharedKeyCredential = new StorageSharedKeyCredential(storageAccountNameFlat, storageAccountKeyFlat);
            string dfsUri = "https://" + storageAccountNameFlat + ".dfs.core.windows.net";
            DataLakeServiceClient dataLakeServiceClient = new DataLakeServiceClient(new Uri(dfsUri), sharedKeyCredential);

            Assert.NotNull(dataLakeServiceClient);


            var fileSystemClient = dataLakeServiceClient.GetFileSystemClient(fileSystemName);

            Assert.NotNull(fileSystemClient);

            IAsyncEnumerator<PathItem> enumerator =
                fileSystemClient.GetPathsAsync(string.Empty).GetAsyncEnumerator();

            
            int count = 0;

            while (await enumerator.MoveNextAsync())
            {
                var item = enumerator.Current;
                count++;
            }
            

            Assert.True(count == 2);

        }




        [Fact]
        public async Task ConnectToGen2SA()
        {
            // Theres two options here. Theres the AD option using the service principal 
            // OR using the account key directly


            var sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            string dfsUri = "https://" + storageAccountName + ".dfs.core.windows.net";
            DataLakeServiceClient dataLakeServiceClient = new DataLakeServiceClient(new Uri(dfsUri), sharedKeyCredential);

            Assert.NotNull(dataLakeServiceClient);

            //create a container
            var filesystems = dataLakeServiceClient.GetFileSystems();

            var createresponse = await dataLakeServiceClient.CreateFileSystemAsync(fileSystemName);
            Assert.NotNull(createresponse);

            DataLakeFileSystemClient fileSystemClient = dataLakeServiceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = await fileSystemClient.CreateDirectoryAsync("my-directory");

            //Possible to rename or move directories-

            await UploadFile(fileSystemClient);

            //var deleteresponse = await dataLakeServiceClient.DeleteFileSystemAsync("nabeel-container");
            //Assert.NotNull(deleteresponse);
        }

        [Fact]
        public async Task ListDirectories()
        {
            var sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            string dfsUri = "https://" + storageAccountName + ".dfs.core.windows.net";
            DataLakeServiceClient dataLakeServiceClient = new DataLakeServiceClient(new Uri(dfsUri), sharedKeyCredential);

            var filesystems = dataLakeServiceClient.GetFileSystemsAsync().AsPages(default, 20);
            List<FileSystemItem> fileSystemItems = new List<FileSystemItem>();

            await foreach (Azure.Page<FileSystemItem> fileSystemPage in filesystems)
            {
                foreach (var item in fileSystemPage.Values)
                {
                    fileSystemItems.Add(item);
                }
            }

            List<PathItem> pathItems = new();
            DataLakeFileSystemClient fileSystemClient = dataLakeServiceClient.GetFileSystemClient(fileSystemName);
            var directories = fileSystemClient.GetPathsAsync(null, true).AsPages(default, 20);

            await foreach (Azure.Page<PathItem> directoryPage in directories)
            {
                foreach (var item in directoryPage.Values)
                {
                    pathItems.Add(item);
                }
            }


            //DataLakeFileSystemClient fileSystemClient = dataLakeServiceClient.GetFileSystemClient(fileSystemName).getz
            //var subdirectories = 

            Assert.True(pathItems.Count > 0);

            var directoryClient = fileSystemClient.GetDirectoryClient("NRCan-RNCan.gc.ca/nabeel.bader/test");
            var subdirectories = directoryClient.GetPathsAsync().AsPages(default, 20);

            List<PathItem> subpathItems = new();
            await foreach (Azure.Page<PathItem> directoryPage in subdirectories)
            {
                foreach (var item in directoryPage.Values.Where(p => p.IsDirectory == true))
                {
                    subpathItems.Add(item);
                }
            }

            Assert.True(subpathItems.Count > 0);

            bool isSubDir = false;
            bool isNotSubDir = false;
            if (pathItems.Where(i => i.Name == "my-directory/sub-dir").Any())
            {
                isSubDir = true;
            }

            if (pathItems.Where(i => i.Name == "my-directory/sub-dir2").Any())
            {
                isNotSubDir = true;
            }

            Assert.False(isNotSubDir);
            Assert.True(isSubDir);

            if (!isNotSubDir)
            {
                DataLakeDirectoryClient createDirectoryClient = await fileSystemClient.CreateDirectoryAsync("my-directory/sub-dir2");
            }

            Assert.True(fileSystemItems.Count > 0);
        }


        [Fact]
        public async Task ListFilePermissions()
        {
            var sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            string dfsUri = "https://" + storageAccountName + ".dfs.core.windows.net";
            DataLakeServiceClient dataLakeServiceClient = new DataLakeServiceClient(new Uri(dfsUri), sharedKeyCredential);
            DataLakeFileSystemClient fileSystemClient = dataLakeServiceClient.GetFileSystemClient(fileSystemName);

            await ManageFileACLs(fileSystemClient);
            //Assert.NotNull(FileAccessControl);
        }

        [Fact]
        public async Task Rerun_Indexer()
        {
            var azureKeyCreds = new AzureKeyCredential("21D5756DF91AE0E5E65C47D41DDE3ACF");
            var indexerClient = new SearchIndexerClient(new Uri("https://datahub-search-dev.search.windows.net"), azureKeyCreds);

            var response = await indexerClient.RunIndexerAsync("datahub-azuredatalake-indexer");

            Assert.Equal(202, response.Status);

        }

        [Fact]
        public void Get_Index()
        {
            var azureKeyCreds = new AzureKeyCredential("21D5756DF91AE0E5E65C47D41DDE3ACF");

            var searchIndexClient = new SearchIndexClient(new Uri("https://datahub-search-dev.search.windows.net"), azureKeyCreds);

            var index = searchIndexClient.GetIndex("datahub-file-index");

            var searchClient = searchIndexClient.GetSearchClient("datahub-file-index");

            List<string> idsToDelete = new List<string>() { "MjNiNjUwMjgtMGQwMy00NDI4LWI2OWMtZTUxZDgyMDAyYThi0" };
            var response = searchClient.DeleteDocuments("fileid", idsToDelete);
            Assert.NotNull(searchIndexClient);
            Assert.NotNull(index);
            //Assert.True(response.Value.Results.Count > 0);
        }

        [Fact]
        public async Task DeleteFile()
        {
            var sharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            string dfsUri = "https://" + storageAccountName + ".dfs.core.windows.net";
            DataLakeServiceClient dataLakeServiceClient = new DataLakeServiceClient(new Uri(dfsUri), sharedKeyCredential);

            DataLakeFileSystemClient fileSystemClient = dataLakeServiceClient.GetFileSystemClient(fileSystemName);
            DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient("NRCan-RNCan.gc.ca/nabeel.bader");

            DataLakeFileClient fileClient = directoryClient.GetFileClient("walker.csv");

            Assert.NotNull(fileClient);

            var response = await fileClient.DeleteAsync();
            Assert.True(response.Status == 200);
        }


        [Fact]
        public async Task GivenSearchParameters_RetrieveJsonFromCognitiveSearch()
        {
            var indexClient = CreateSearchIndexClient("azureblob-index");
            SearchResults<FileMetaData> results;

            var options = new SearchOptions()
            {
                Filter = $"ownedby eq '{userId}'"
            };
            options.Select.Add("filename");
            options.Select.Add("createdby");
            options.Select.Add("lastmodifiedby");
            options.Select.Add("securityclass");
            options.Select.Add("ownedby");
            options.Select.Add("filesize");
            options.Select.Add("fileformat");

            results = indexClient.Search<FileMetaData>("*", options);

            Assert.NotNull(results);

            List<FileMetaData> fileMetaDatas = new();

            await foreach (Page<SearchResult<FileMetaData>> searchResults in results.GetResultsAsync().AsPages(default, 20))
            {
                foreach (var item in searchResults.Values)
                {
                    fileMetaDatas.Add(item.Document);
                }
            }

            Assert.NotEmpty(fileMetaDatas);
        }

        [Fact]
        public Task CreateIndex()
        {
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(FileMetaData));
            var azureKeyCreds = new AzureKeyCredential("21D5756DF91AE0E5E65C47D41DDE3ACF");
            var adminClient = new SearchIndexClient(new Uri("https://datahub-search-dev.search.windows.net"), azureKeyCreds);


            var definition = new SearchIndex("filemetadata-index", searchFields);

            var suggester = new SearchSuggester("sg", new[] { "ownedby", "filename" });
            definition.Suggesters.Add(suggester);

            adminClient.CreateOrUpdateIndex(definition);
            return Task.CompletedTask;
        }

       
        private SearchClient CreateSearchIndexClient(string indexName)
        {
            var azureKeyCreds = new AzureKeyCredential("21D5756DF91AE0E5E65C47D41DDE3ACF");

            SearchClient indexClient = new SearchClient(new Uri("https://datahub-search-dev.search.windows.net"), indexName, azureKeyCreds);
            return indexClient;
        }

        private async Task ManageFileACLs(DataLakeFileSystemClient fileSystemClient)
        {
            DataLakeDirectoryClient directoryClient =
                fileSystemClient.GetDirectoryClient("NRCan-RNCan.gc.ca/nabeel.bader");

            DataLakeFileClient fileClient =
                directoryClient.GetFileClient("datahub features.xlsx");

            PathAccessControl FileAccessControl =
                await fileClient.GetAccessControlAsync();

            //return FileAccessControl;
            //foreach (var item in FileAccessControl.AccessControlList)
            //{
            //    Console.WriteLine(item.ToString());
            //}


            IList<PathAccessControlItem> accessControlList
                = PathAccessControlExtensions.ParseAccessControlList
                ($"user:{userId}:r--");

            fileClient.SetAccessControlList(accessControlList);
        }

        static async Task UploadFile(DataLakeFileSystemClient fileSystemClient)
        {
            DataLakeDirectoryClient directoryClient =
                fileSystemClient.GetDirectoryClient("my-directory");

            DataLakeFileClient fileClient = await directoryClient.CreateFileAsync("uploaded-file.txt");

            FileStream fileStream =
                File.OpenRead(@"C:\Users\nbader\Documents\upload.txt");

            long fileSize = fileStream.Length;

            Dictionary<string, string> metadata = new Dictionary<string, string>();
            //metadata.Add("x-ms-blob-type", "BlockBlob");
            metadata.Add("folderowner", "Nabeel");
            metadata.Add("folderid", "Main Id");

            fileClient.SetMetadata(metadata);

            await fileClient.AppendAsync(fileStream, offset: 0);

            await fileClient.FlushAsync(position: fileSize);

        }

    }

}
