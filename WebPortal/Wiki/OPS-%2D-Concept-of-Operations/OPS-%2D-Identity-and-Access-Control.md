# Identity and Access Control

## Definition

The DataHub Portal application delegates the authentication function to Azure Active Directory (AAD), which holds all NRCan users as a replication from NRCan on-premise Active Directory for the Windows
credentials. It is up to the NRCan's department AAD standard to define authentication mechanism such as password and Multiple Factor Authentication (MFA). The trust between DataHub Portal application and
AAD is configured by NRCan but it is managed by the Microsoft Azure platform using system managed identity for the application. During operation, all users visiting the DataHub portal application require to authenticate with AAD. The DataHub Portal application itself also assumes AAD system managed identity to access other services such as Azure Key Vault.

Application level authorization is managed within the DataHub portal application. User identity is retrieved from AAD authenticated session and will persist in Azure Cosmos DB containers. Likewise, file and folder sharing information is also persisted in Azure Cosmos DB. The DataHub portal application code manages the persistence and interpretation of the authorization data. Other system accessing DataHub's Blob Storage relies on standard Azure Blob mechanism for managing the scope and level of access.

Azure Identity and Access Management Overview.docx is documented in the following document

<https://gcdocs.gc.ca/nrcan-rncan/llisapi.dll/Open/44977349>

## Data Projects

Data Projects in DataHub feature a simple model to simplify the onboarding of new users to specific tools in a data project.

### Request access to resource

The Data Projects workflow let users request access to:
- Project specific Databricks workspace (*)
- Project specific WebForm
- Project specific PowerBI workspace (*)

Access request to resources is open to all NRCan users.

If a user needs access to one of those service it can result in the following two workflows (only for resources *):
- **Workflow 1:** Request for resource creation. This request will be reviewed by the DataHub team and if approved, a resource will be added to the system Terraform and deployed on the next system update.
- **Workflow 2:** Request access to an existing resource. If approved by the project administrators, the DataHub system automation will add the user to the resource with limited access. Additional ACL for the resource access need to be managed within the resource itself.
 
### Project Owners

Each data project has a list of project owners. That list of project owners is maintained by the project owners of the DataHub project tracker which includes the key members of DataHub.

The project owners consists of a list of Azure usernames and lets user approve other user access to a resource (see list above).



