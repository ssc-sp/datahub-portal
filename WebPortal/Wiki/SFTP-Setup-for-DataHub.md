# SFTP

## Objectives

- Provide secure access to Azure storage to internal & external users/systems using SFTP
- Only SSH key based authentication will be supported
- SFTP should resolve B2B communications and should not require user interactions
- SFTP should be able to handle large data sets

## Assumptions
- Departmental security approves the use of port 22 from NRCan and external networks
- Departmental security approves the use of self-managed SSH Key authentication
- NRCan Cloud team supports the use of port 22 for incoming SFTP file transfer

## Security Requirements

- Private keys are kept by the user and never shared with DataHub
- Public keys for users should be securely managed
- Public keys from users will be received through an external secure channel
- Public keys can be setup manually by the Datahub Team

## SFTP To Azure Container Mapping

- To handle large data sets, ideally, no data copy should be done
- Each user will have access to only one storage container through the SFTP
- Multiple folder configurations might be required to handle different storage containers:
  - a sftp folder can be mounted to a storage account with full access
  - if possible, a sftp folder would also be mounted to a subfolder from a gen2 container

## Diagram

<center>

::: mermaid
 graph LR;
 A[SFTP<br>Client<br>NRCan] --> B[NRCan <br>Azure LB]
 H[SFTP<br>Client<br>External] --> B
 B --> C[Linux VM]
 C --> D[Azure<br>Storage Account<br>Project A]
 C --> E[Azure<br>Storage Account<br>Project B]
 C --> F[Azure<br>Storage Account<br>Project C]

:::

</center>

## Implementation Details
- Deploy to VM and expose non-standard port by frontend of the NRCan WAF
- Create an account for each project mapping to its own storage account
- Password authentication disabled
- Client to generate SSH key pair and to share public key with DataHub team (no encryption required)
- Each project has only one common SFTP account (therefore one key pair)
- Request SSC to open NRCan outbound firewall to port 22 for SFTP/SSH
- Request Cloud team to forward port 22 to the VM
- Accounts can only access their own storage accounts
