# Solution Overview

## Architecture

![image.png](/.attachments/image-7e0dfcaa-781b-49c1-927e-c0dcda8ca39c.png)

### Network Diagram


<!-- A -> S["Azure SignalR Service<br>(dh-portal-signalr-prd<br>.service.signalr.net)"]
 S -> C -->
<center>

::: mermaid
 graph LR;
 A[Web<br>Browser] --> B

 B["Azure WAF<br>(datahub<br>.nrcan-rncan.gc.ca)"] --> C["Azure App Service<br>(dh-portal-app-prd)"]
 C --> D["Gen 2 Storage Account<br>(dhportaldatalakeprd)"]
 C --> E["Azure SQL Server<br>(dh-portal-sql-prd)"]
 C --> F["Azure Cosmos DB<br>(dh-portal-cosmosdb-prd)"]
 C --> G["Azure Search<br>(dh-portal-search-prd)"]
:::

</center>

### Authentication

DataHub uses Azure Authentication which uses standard 2 factor MFA.

# DataHub Features

## Storage

The DataHub portal gives all NRCan users access to a secure storage. The storage portal also lets users share files with other NRCan users.

The storage uses in the backend an Azure Gen2 Storage Container where each user has their own folder. Each folder uses ACL and by default will restricts the access to the user and the indexing service. 

## Data Projects

Data Projects is a collection of tools used for a group of NRCan users to let them store data, process it and finally use modern BI tools to tell a story.

### Databricks

Databricks a SaaS service in Azure based on Apache Hadoop and Apache Spark. The service provides access in each notebook to Delta Lake file storage, MLflow and Koalas, popular open source projects that span data engineering, data science and machine learning. Databricks provides a web-based platform for working with Spark, that provides automated cluster management and IPython-style notebooks. 

When a Data Project is created in DataHub, a corresponding storage account is also created. That storage account leverages Gen1 storage with keys stored in the Azure KeyVault. 

### PowerBI

PowerBI workspaces can be created in the data projects and the system automation will assign the project users with the required permissions in the workspace. 
Only DataHub administrators will have administrator permissions in the workspaces.

### Web Forms

Web Forms are custom Line-of-Business (LOB) applications designed to replace spreadsheet and store structured datasets inside SQL Databases. The forms are based on the datahub form editor which lets user map the fields inside each table.

Web Forms are deployed as part of the DataHub portal and leverage the ASP.Net Core security model with roles which limits access inside the pages themselves to users that are not authoried.

### Web Form Editor

The Web Form Editor lets users design forms in a data project. The output from the editor is manually assembled by DataHub developers into a custom application when the client approves the output.