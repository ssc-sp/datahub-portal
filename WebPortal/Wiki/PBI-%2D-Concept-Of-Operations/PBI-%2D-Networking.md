# Networking Configuration

## PowerBI Web Client - Report Reader/Editor

The report readers will be reading the reports using [support web browsers](https://docs.microsoft.com/en-us/power-bi/report-server/browser-support)
Chrome and Edge are recommended on the NRCan desktop environment.

Authentication to the service will follow existing M365 procedure which require MFA and VPN connection.

![image.png](/.attachments/image-148c1970-7f20-495b-b8c6-e0a32f6a9fc7.png)

The same configuration allows users to edit PowerBI reports online. It features same capabilities as the desktop version however it does not allow the creation of new data set or connection to new data sources.

## PowerBI Desktop - Report Author 

To develop reports with new data sources, users will need to create the reports on the PowerBI Desktop tool. The desktop powerBI deployment is handled by the NRCan desktop team and is out of the scope of this project.

In PowerBI Desktop, the users will follow the standard Microsoft 365 authentication to authenticate and give users permissions to access datasets and workspaces in PowerBI.

PowerBI Desktop will require direct connectivity to the data sources:
- SQL Server - requires port 1433 for connecting securely to the SSL gateways. This document details the networking requirement from the NRCan side. https://docs.microsoft.com/en-us/azure/azure-sql/database/connectivity-architecture
- Databricks - databricks uses standard HTTPS connections


![image.png](/.attachments/image-3734d62f-628b-4615-be32-3b970434cb9a.png)