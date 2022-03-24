
# Welcome to the NRCan DataHub Portal

This repository contains the source code the NRCan DataHub. The DataHub is an enterprise portal designed to manage, connect and bridge existing cloud tools and simplify scientific and corporate workflows.

## What are the portal capabilities?

### Landing & Shortcuts

The DataHub landing pages provides instant jump lists to let users access their recent tools, and also the storage, Power BI and databricks areas associated with their account. The DataHub landing has saved many users from navigation exhaustion in the Power BI & Databricks menus.

### Data Projects

The DataHub makes it easy for multiple teams, labs or users to get access to Storage, Databases, Data Science and Analytical tools:

- Storage accounts: A Data Project includes a storage explorer to upload/download files
- Databricks integration: The Data Project has a direct link to Databricks workspaces and also simplifies the mounting of the storage account for the notebooks
- Power BI integration: The integration implements NRCan's governance model and connects users directly to their workspaces and key reports.
- SQL Server: A SQL Server can be associated to a project and the connection details will be directly available to the users.
- PostgreSQL: Postgresql servers can also be linked and the project tools can generate the associated Azure token for integrated authentication.
- Data Sharing: A simple workflow lets users select a file, and request data sharing (see Data Sharing for more details)
- User onboarding: Project administrators can invite other users to their project(s)

### User Management

The portal integrates with Azure Active Directory and manages roles and users. The portal has 3 types of users: Datahub administrators, Project administrators and Users. Users can access project resources, project administrators can invite other users and datahub administrators have the ability to invite administrators.

### Resource Management

The portal is designed to be used with Terraform or other IaC systems and automate the creation of resources. The system manages a list of workflows with user requests in SQL tables that can be integrated in DevOps pipelines. 

### Secure by default

Each component in the system has been designed with high security in mind. The portal doesn't require any elevated Azure roles and uses OBO permissions for all management tasks.



# Github Structure

This project includes multiple repositories
- **DataHub Web Portal:** This repository contains the code for all the portal and Azure Functions used to automate Databricks tasks.
- **DataHub Terraform:** The terraform infrastructure for the project is stored in this repository and elements of the terraform script are dynamically generated from the Data Project database. Please contact us for details on the terraform infrastructure.
- **DataHub Databricks:** Databricks is used in this project for ETL, Data Science and other data transformations. Examples from this repository can be used as template for setting up new tasks.



