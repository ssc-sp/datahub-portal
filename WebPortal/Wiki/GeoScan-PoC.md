# GeoScan PoC

## Objectives

- Provide a storage account to store GeoScan publication
- Create a process for ininial data loading and incredemtal data loading
- Establish cost control and optimization practices

## Assumptions
- GeoScan stores working data on a network drive
- Data processing is out-of-scope for DataHub
- Azure storage account is mainly for "archival" purpose and data is read infrequently

## Solution Overview

- Azure Storage with "cool" access tier, which cost more for data retrieval but cost less for storage when comparing to "hot" tier
- All data will be stored in blob storage
- Users will use AzCopy utility to upload and sync data from local (i.e. network drive) to Azure

## GeoScan Data Processing

<center>

::: mermaid
 graph LR;
 A[STAR<br>Repository] --> B[Transformation]
 E[Mirage] --> B
 D[Digitization Group] --> B
 B --> C[Azure Blob]

:::

</center>


## Diagram

<center>

::: mermaid
 graph LR;
 A[GeoScan<br>Network Drive] --> B[AzCopy<br>on Windows PC]
 B --> C[Azure Blob]

:::

</center>

## Implementation Details
- Download AzCopy from [Azure](https://aka.ms/downloadazcopy-v10-windows) (no installation required)
- Obtain SAS Token from DataHub team 
- Run AzCopy to sync data from local PC or network drive to Azure ```azcopy sync n:\network-drive\geoscan\stage https://blob.windows.net/geoscan-container/?token```
