# Continuous Integration and Continuous Deployment

## Definition

Continuous integration, delivery, and deployment (CI/CD) is an integral part of modern development intended to reduce errors during integration and deployment while increasing project velocity. CI/CD is a philosophy and set of practices often augmented by robust tooling that emphasize automated testing at each stage of the software pipeline. By incorporating these ideas into your practice, you can reduce the time required to integrate changes for a release and thoroughly test each change before moving it into production.

The DataHub project leverages CI/CD techniques and toolings to:
- Accelerating application development and development lifecycles.
- Building quality and consistency into an automated build and release process
- Increasing application stability and uptime

This project stores code on Github.com under the NRCan organization. It also uses various components of Azure DevOps stack for building, deploying and potentially testing the application and infrastructure.

DataHub currently operates the following environments (URLs are subject to change as we work with Cloud team to incorporate official DNS):

- DEV: https://dh-portal-app-dev.azurewebsites.net
- TEST: https://dh-portal-app-tst.azurewebsites.net
- Production: https://dh-portal-app-prd.azurewebsites.net

## People

The following roles are involved in the CI/CD processes:

- **CI/CD Engineers** are responsible for developing CI/CD principles, iteratively maintaining CI/CD tools/platforms, developing pipeline configurations as well as automating processes;
- **Developers** are required to provide feedback and advices to developing and optimizing CI/CD pipelines;
- **Testers** perform functional and non-functional testing against specific environments and specific versions of the application and infrastructure;
- **System Support** performs operation when the system is in flight;
- **Project Leads** participates in CI/CD operation by giving directions and approval to releases and deployments;

For a standard list of Azure DevOps roles, see https://docs.microsoft.com/en-us/azure/devops/user-guide/roles?view=azure-devops

## Policy

- Maintain single source of truth and traceability. Git repos are used to trace the changes. It also provides means to enforce and audit who changed what.
- todo


## Tools

The DataHub project leverages NRCan standard Azure DevOps tools. At the time of writing, the following tools are being actively used:

- Repos
- Pipelines
- Boards
- Wiki 

For more information, see Azure DevOps at https://azure.microsoft.com/en-ca/services/devops/ 

## Process and Procedures

### Build Pipelines
The DataHub project currently runs the following build piplelines.
| Pipeline| Repo | Tigger | Target | Description |
| --- | ----------- | --- | --- | --------- |
| github.datahub-portal| github.com/NRCan/datahub-portal | Branch develop | DEV | Build and deploy to DEV |
| github.datahub-biosim | github.com/RNCan/BioSIM_Web_API_Prod | -- | DEV| Build and deploy to BioSIM DEV |
| github.datahub-portal-build | github.com/NRCan/datahub-portal| -- | -- | Build and package from master branch |

### Release Pipelines
| Pipeline| Artifacts| Approval | Trigger | Targets | Description |
| --- | ----------- | --- | --- | -- | --------- |
| github.dh-portal-app-release| github.datahub-portal-build| Required | -- | TST, PRD | Manually release DataHub Portal app |

