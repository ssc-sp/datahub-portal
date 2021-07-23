# Operational & Support Environment

## PowerBI Desktop (report authoring)

PowerBI Desktop is required to author reports. Deployment of PowerBI desktop is responsibility of the desktop team at NRCan and is outside the scope of this system.

## Report read-access

Report read-access is done either through the web (with a supported browser) or through PowerBI desktop - however this use case is recommended for report authors.

## Supported Data Sources

### DataSets
Datasets is the primary and recommended model for building reports. A dataset enables a secure access to a curated data source in the PowerBI environment.

Datasets leverage the data classification options from Microsoft 365 and secures the access to the source data. With Datasets, users do not need to connect to the original source files and another layer of security is available to set ACL.

### Databases

DataHub & PowerBI support connections to the most common databases. The data classification/security zone of the database itself is out of scope of this system.

#### Connectivity requirements (Protected A & Protected B):
- SSL/Encryption has to be enabled on the database and in the connection string
- Credentials used for access should have limited read access to the tables/views that are required

#### Specific Protected B requirements
- For SQL Server, integrated authentication should be used when possible

### Blob Storage
Azure Blob Storage can be used to extract data from CSV/Excel files stored in Azure Storage Containers.

Only storage accounts from the DataHub resource groups are supported for both Protected A and Protected B data sources and it is to note that each storage account has its specific data classification.

### Hive tables

Hive Tables enable access to large datasets using Databricks and standard SQL queries. Hive Tables relies on:
- Databricks service to process data
- Storage accounts for the storage of the data

Both Databricks instances and Storage accounts have their own data classification and only Databricks and Storage Accounts in the DataHub resource groups are supported for this scenario.



### Other supported PowerBI sources

Other PowerBI data sources are supported for PowerBI with the following conditions:
- connection to the data source is using approved encryption
- the data source is stored in the DataHub environment and has an associated DataHub classification.
- only protected A data sources are allowed for data sources not listed here

## Databricks (Data Processing)

Databricks is cloud based data processing platform that is used for ingesting/extracting, transforming and loading data into relational data stores that are used downstream for analysis and reporting purpose.

DataHub supports two modes of operation for Databricks.

### Databricks Protected B

In Databricks Protected B only Spark based jobs are supported and will leverage Integrated Authentication. Storage accounts will need to be enabled with custom encryption keys.

Currently, Native R & Native Python scripts are not supported. **Ilango - can you confirm?**

### Databricks Protected A

This mode of operation supports any Protected A data source.

Role Based Access Control feature of Databricks will be used to control the access to Notebooks, Clusters and Data.

- AD Credential pass through will be used to control who has access to which data in the delta lake and storage accounts
https://docs.microsoft.com/en-us/azure/databricks/security/credential-passthrough/adls-passthrough#adls-aad-credentials

- Credentials required for database connectivity will be managed using Azure Secret Management service.
https://docs.microsoft.com/en-us/azure/databricks/security/secrets/

- Clusters will be configured to ensure that the traffic between cluster worker nodes will be encrypted in transit and rest.
https://docs.microsoft.com/en-us/azure/databricks/security/encryption/encrypt-otw

- Databricks instance will be configured to generate and store comprehensive set of audit logs to track what the users are doing on the platform.
https://docs.microsoft.com/en-us/azure/databricks/security/data-governance#audit-access

- Service Principal will be used to allow other services such as PowerBI to securely access Databricks
https://docs.microsoft.com/en-us/azure/databricks/security/data-governance#secure-access-to-azure-data-lake-storage


###Securely Accessing Data From Storage Accounts from Databricks
Azure Data Lake Storage Gen2 (ADLS) will be used for storing files (csv,json, xml ect) that will be processed by Databricks. 

ADLS provides capabilities to manage access files through the use of access control list (ACLs), which can be applied at the folder level. These permissions are then assgined to Azure Active Directory Groups or Service Principal. Users will be assigned to different groups based on the required permission, and the groups will be assigned to the ACLs.



Following two methods will be used for securely accessing the data in ADLS:

1. Using AAD Crendential Passtrhough - **Protected A & B**
AAD credential pass through feature of ADB allows users to use their own credentials to access data direcltly or create mount points within Databricks. When the user attempts to access the data using this method, the user's AAD credentials are passed through to ADLS and evaluated against the files and folder. This method limits certain features of the Databricks platform. In those scenarios, the Service Principal should be used to securely manage access to the ADLS.
![image.png](/.attachments/image-25662a39-1af5-479c-8078-5f3c076946d3.png =400x)

2. Using Service Principal - **Protected A only**
Service Principal will be used to provide other services that execute workload on the Databricks access to a particular folder in ADLS. This methad will also serve as a alternative option for managing access for the users in scenarios where the AAD Credential Passthrough is not a viable solution due to its limitations.
Users and services will be added to the Service Principal, which will be added as part of an appropriate group in AAD. This allows the users of Databricks to access data from ADLS directly or using mount points.  The credentials of the Service Principal will be seurely managed using the key vault and the secret scope feature within Databricks. To note, this option is only available for Protected A deployments.

![image.png](/.attachments/image-9d277e15-d939-45af-bec5-d18f6e1245cf.png =400x)
