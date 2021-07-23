# Requirement

An upload service is required so that end users can securely upload files to Azure GEN2 storage.

# Option 1 - Shared Access Signature (SAS) with Azure Storage

- Create SAS for a staging container with upload permission;
- Email the SAS URL to the user securely
- Create a pipeline to move data into Gen2 (possibly based on metadata)

## Detailed procedure
0. Have the users install Azure AzCopy from https://aka.ms/downloadazcopy-v10-windows
1. Generate SAS for a folder in Storage Account. The easies way is to use Azure Portal or Azure Storage Explorer (Storage Account > Containers > Click ... > Generate SAS). NOTE: Azure CLI can also be used to generate SAS. For example: <pre>az storage container generate-sas \
    --account-name <storage-account> \
    --name <container> \
    --permissions acdlrw \
    --expiry <date-time> \
    --auth-mode login \
    --as-user</pre>
2. Securely share the SAS URL with users (e.g. encrypted email)
3. Users can use the SAS URL in two ways:
- Launch Azure Storage Explorer and connect using SAS URI
- Run azcopy using the SAS URL (e.g. azcopy copy "C:\local\path" "SAS-URL" --recursive=true)

# Option 2 - SFTP as Docker

Create a Docker instance on Azure Container Instance (ACI) to:

- Accept user login with SSH key (keys have to created case by case)
- All files will be uploaded to dedicated staging container
- Once uploaded, files have to be moved to GEN 2 location
- File permissions have to be applied

Reference: https://docs.microsoft.com/en-us/samples/azure-samples/sftp-creation-template/sftp-on-azure/