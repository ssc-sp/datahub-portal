[![Gitter](https://badges.gitter.im/Science-Program/community.svg)](https://gitter.im/Science-Program/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

# Welcome to the DataHub Portal

This repository contains the source code the NRCan DataHub. The DataHub is an enterprise portal designed to manage, connect and bridge existing cloud tools and simplify scientific and corporate workflows.

## What are the portal capabilities?

### Landing & Shortcuts

The DataHub landing pages provides instant jump lists to let users access their recent tools, and also the storage, Power BI and databricks areas associated with their account. The DataHub landing has saved many users from navigation exhaustion in the Power BI & Databricks menus.

### Data Projects

The DataHub makes it easy for multiple teams, labs or users to get access to Storage, Databases, Data Science and Analytical tools:

- Storage accounts: A Data Project includes a storage explorer to upload/download files with a friendly user interface.
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

Each component in the system has been designed with high security in mind. The portal doesn't require any elevated Azure role or service principal and uses OBO permissions for all management tasks.

### Integrated help

The key pages and modules in the portal offer integrated help for guiding users with Power BI, Databricks, and Azure Storage. The current help content is located in the Wiki.

### Data Entry Framework

The portal includes a data entry framework that leverages Entity Framework, fluent code, annotations and can generate complex forms to support client business rules, multiple tables through standard SQL tables. This model enables an easy integration in Power BI or Tableau.

## Technology

The Web Portal is developed in .NET 6 and uses Blazor and ASP.Net core and several other open source libraries. The portal can run on Windows, Linux and can be deployed in any cloud environment. The styling leverages SCSS to modularize the CSS settings.

## Extensibility & Configuration

Several customizations can be done simply using the configuration. Each module in the data project can be enabled or disabled in the configuration.

The DataHub is divided into modules which connect together using the ASP.Net core IoC broker. This enables to add extensions through nuget packages or separate projects. All modules are automatically wired to the portal using assembly scanning. Each module can be disabled using the configuration files.

### Localization

The DataHub provides localization files for both English et Francais but support for other languages can be added using additional localization files.


# Github Structure

This project includes multiple projects
- **DataHub**, contains the code for the application and presentation layer of the modules.
- **Resource Provisioner** contains the code to handle the terraform infrastructure for the Datahub

## Commit Messages

The commit messages must loosely follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification. This enables to automatically generate the changelog and release notes.

The scopes are represented by the following table


| Type | Emoji | Code |
| --- | --- | --- |
| feat | ✨ | :sparkles: |
| fix | 🐛 | :bug: |
| docs | 📚 | :books: |
| style | 💎 | :gem: |
| refactor | 🔨 | :hammer: |
| deploy | 🚀 | :rocket: |
| test | 🚨 | :rotating_light: |
| build | 📦 | :package: |
| ci | 👷 | :construction_worker: |
| chore | 🔧 | :wrench: |
| work in progress (WIP) | 🚧 | :construction: |

## Branching

Branches are created from the develop branch and merged back into the develop branch. The master branch is used for releases only. Pull requests are strongly encouraged and should be reviewed by at least one other developer.