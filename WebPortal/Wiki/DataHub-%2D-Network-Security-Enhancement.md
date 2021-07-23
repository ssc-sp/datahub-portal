# Option 1. Security Without VNet

The following table illustrates the security enhancement to tighten firewall rules without the use of VNet integration.

| Service | Public<br>Access | NRCan<br>Access | Inbound<br>from<br>Other<br>Service| Notes |
| ----- | --- | --- | --- | ------------ |
|Portal<br>App Service|Yes|Yes|WAF|WAF also whitelist NRCan's public CIDR|
|Cosmos DB|No|Dev|App Service||
|Function App|No|Dev|--|
|MS<br>SQL Server|No|Yes|App Service<br>Function App<br>PowerBI<br>DataBricks||
|Search|No|Dev|App Service||
|Signal R|Yes|Yes|App Service||
|Storage Acct|Yes|Yes|App Service<br>Search||

- \* Default to standard HTTPS on port 443
- Note: All services have public IP addressing due to the lack of VNet integration. These public IPs are shared among other Azure customers due to the lack of VNet endpoints.


# Option 2. DataHub Portal Use of Azure VNet

This note provides analysis in transitioning the existing DataHub solution to use Azure VNet for Protected B support of the solution. It is generally considerred necessary to use dedicated VNet and subnets to support Protected B workload and data assets.

## Assumptions

- SQL Server must allow access from NRCan network
- Storage Account Blob must allow access from NRCan network
- Use service endpoints whenever possible to avoid the use of private endpoints for Azure services

## Subnet Requirement

The solution requires 3 subnets in the dedicated VNet:
- Subnet 1: For DataHub Portal App Service and other Azure service endpoints (subnet size 32 or /27)
- Each Databricks workspace deployed in VNet requires
  - Subnet A: Container subnet for Databricks (subnet size 32 or /27)
  - Subnet B: Host subnet for Databricks (subnet size 32 or /27)

## Service Mapping
| Service | Public<br>Access| VNet<br>Service<br>Endpoint | Notes |
| ----- | -- | --- | ------------ |
|App Service||Yes|Also enable App Service VNet Integration|
|Cosmos DB||Yes|NSG to allow from App Service only|
|Function App||Yes|Also enable App Service VNet Integration|
|MS SQL Server|Yes|Yes|Whitelist NRCan CIDR in network settings|
|Search|Yes|--|IP firewall to restrict to VNet|
|Signal R||Yes|May require Private Endpoint for Signal R|
|Storage Acct|Yes|--|Whitelist NRCan CIDR to use SAS URL|
|VM||Yes|Require ALB to support SFTP on port 22|
|Databricks||Yes|Use VNet Injection|


