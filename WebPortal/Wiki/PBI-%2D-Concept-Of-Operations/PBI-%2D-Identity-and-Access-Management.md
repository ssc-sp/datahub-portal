# PowerBI Access Control

## Authentication

PowerBI leverages the Microsoft 365 users and authentication - there are no external users or other authentication mechanisms. 

## PowerBI Users (consumer)

A PowerBI user is an authenticated who is using the PowerBI portal to read reports. 

Once the SA&A has been approved, all NRCan users will be granted by default a PowerBI Free User license. This license will only let a user access approved reports published in the PowerBI premium workspaces. Without a proper license, user cannot access any feature from the PowerBI portal.

The default access to reports will depend on the groups and permissions configured to existing reports. 

## PowerBI Authors

PowerBI Authors require:
- PowerBI Desktop installed
- PowerBI Pro License
- Access to existing PowerBI datasets or access to the sources 

The networking setup is detailed in [Networking Configuration](/PBI-%2D-Concept-Of-Operations/PBI-%2D-Networking)

PowerBI Pro Licenses are configured by the Cloud Team and managed by the DataHub team.

## Key Controls for Reports & Workspaces

PowerBI reports have multiple controls for permissions on a report and controls can be put in place to limit the type of interactions and capabilities on (e.g. Exporting Data, Sharing/Collaboration) Reports.

See the following links for more details:
- Workspace permissions https://docs.microsoft.com/en-us/power-bi/collaborate-share/service-new-workspaces
- Sharing Reports and Dashboards 
https://docs.microsoft.com/en-us/power-bi/collaborate-share/service-share-dashboards
- Dataset permissions
https://docs.microsoft.com/en-us/power-bi/connect-data/service-datasets-build-permissions

## Workspaces

Workspaces are places to collaborate with colleagues to create collections of dashboards, reports, datasets, and paginated reports.

- Workspaces will be created by the DataHub team
- Administrative permissions on the workspaces will be granted to the project owners and they will be in charge of managing the permissions inside the workspace.
- The DataHub team will guide the project owners for the configuration of permissions and detail their responsibilities
- For common permissions to large groups (10 users), AD Groups will be created by the cloud team for each roles (e.g. read only).

## DataSets

Power BI datasets represent a source of data ready for reporting and visualization. 

- Dataset attributes and permissions will be configured by the Datahub team.

## Reports

- The client will be the business owner of the deployed PowerBI reports.
- The client will be responsible for configuring the permissions on the report.
- DataHub team will provide guidance and recommendations for the configuration of permissions
- Sharing of reports publicly will be reviewed and approved by the DataHub team

