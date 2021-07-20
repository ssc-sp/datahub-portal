## Logging and Audit


### Overview

::: mermaid
graph LR
    DataBricks --> LogAnalytics
    WebApp(Web Portal) --> LogAnalytics
    WebApp(Web Portal) --> AppInsights
    AzureAD --> LogAnalytics
    AzureAD --> AppInsights
:::

### Azure App Insights

Application Insights, a feature of Azure Monitor, is an extensible Application Performance Management (APM) service for developers and DevOps professionals. Use it to monitor your live applications. It will automatically detect performance anomalies, and includes analytics tools to help you diagnose issues and to understand how users interact with the app. 

### Azure Log Analytics

Log Analytics is a tool in the Azure portal used to edit and run log queries with data in Azure Monitor Logs. You may write a simple query that returns a set of records and then use features of Log Analytics to sort, filter, and analyze them. Or you may write a more advanced query to perform statistical analysis and visualize the results in a chart to identify a particular trend. Whether you work with the results of your queries interactively or use them with other Azure Monitor features such as log query alerts or workbooks, Log Analytics is the tool that you're going to use write and test them.

### People

- **Users:** users are NRCAN employees or contractors with a validated Azure AD login.
- **DataHub Administrators:** DataHub administrators are users with special Azure permissions and authorization to access the datahub resource groups and run queries on Log Analytics.
- **NRCAN Security Team:** The NRCan security team is responsible for reporting incidents.

### Policy

To comply with NRCAN's SA&A process, Datahub needs to comply with audit requirements. This means that key user actions need to be logged and archived such as:
- user opens web portal
- user uploads file
- user opens databricks
- user runs notebook

Archived logs need to be protected from modification and authorization.

