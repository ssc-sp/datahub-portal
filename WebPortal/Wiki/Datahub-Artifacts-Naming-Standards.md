# Project Names

Project name should be discussed with the client during the intake. A long name should be descriptive and a short abbreviation should have 5 characters max.

**Sample:** _Citations And Social Mentions_ - **CASM**

## Global Artifacts

Global artifacts are expected to be shared between multiple sectors and projects.

> `GLB_<Project>`

## Project Specific Artifacts

Prefix for technical elements

> `<Sector>_<Project>`

**Sample:** CFS_CASM_CITATIONS

# Folder Structures

Folders and other artifacts that do not have length restrictions should be using Sector, Project and Long Description.

**Sample:** CFS_Citations And Social Mentions Project

# Azure Artifact Naming Standards

## Storage Accounts

<service>-<sector>-<project>

**Sample:** databricks-cfs-casm

## Internal Accounts

<service>-CIOSB-Datahub

## Tagging

All Azure artifacts should at least be tagged with following separate tags:
- Sector. e.g. CFS tag
- Project e.g. CASM tag
- Technical Contact <tech_first_last> - tech_yask_shelat
- Business Contact: <biz_first_last> - biz_francis_lougheed
- Security classification: ProtectedA, ProtectedB

# Delta Lake Architecture and Naming Standards

- Delta Lake tables are to be be created as unmanaged tables, using the storage account based on the project it is associated with.


- The medallion architecture Gold, Silver, Bronze to be used for differentiating the quality and state of the data as it is managed through the data pipelines:
![delta_lake_layers.JPG](/.attachments/delta_lake_layers-8312e164-d7a9-4289-a191-fb93925f91de.JPG)



    **Bronze:** the initial landing zone for the data. The data is stored close to its raw form as possible to easily replay the whole pipeline from the beginning, if needed

    **Silver:** the raw data get cleansed, transformed and potentially enriched with external data sets

   **Gold:** production-grade data that end-users can rely on for business intelligence, descriptive statistics, and data science / machine learning. Tables created in this layer must follow the naming standards spceified in the "Databse Naming Standards Section" below.



<br>



## Databricks



#### Workspace
Workspace Access Control should be enabled to prevent users from seeing workspace objects they do not have access to.

Cluster Access Control and Table Access Controls should be enabled to ensure that Table level access can be provided to users based on seurity requirement

Each project may have up to two security groups associated with it:

1. Write-Access-Group - Used for preventing users from unintentionally modifying resources (data, notebooks) that they are not responsible for

2. Read-Access-Group (optional) - this group is to be used on an as needed basis when there is a need to control read access to data/folders/notebooks

The naming convention to be followed the two security grorups as follows:
<project acronym>-Write-Access
<project acronym>-Read-Access

Example:
CITSM-Write-Access
CITSM-Read-Access

### Folder and Notebook Access Control
**Edit, Manage, Run** privileges to folders and notebooks must be limited to users that belong to the Write-Access-Group of that project.

By default, all users should be able to view Folders and Notebooks. If there is a specific project requirement to control who can view contents such as notebooks and data, then Read-Access-Group should be used to faciliate this requirement.



### Databricks Clusters 

It is recommended to have two clusters, a ETL Cluster and a BI Cluster for each project. 

The tags will be used to ensure the tracking of usage and cost can be converged at project, sector or branch level for each cluster regardless of its type.


1. Cluster to manage the data pipeline - ETL Cluster
- To be used by Data Engineers to build the data pipelines necessary to create the business ready data assets. 


2. Cluster to access the production ready data from the delta lake - BI Cluster
- To be used by end users who will be accessing the data using BI tools such as Power BI and Tableau 

- The BI cluster is to be configured to allow access to the data using [SQL only](https://docs.databricks.com/security/access-control/table-acls/table-acl.html#sql-only-table-access-control). 


Naming convention to be followed for cluster is as follows: Cluster-<project acronym>-<type of cluster(bi|etl>>

Example: cluster-citsm-bi


Each cluster must have the following tags attached:
Sector
Branch
Project
Cluster-Type:<bi>or<etl>

### Data Access Control

Write-Access-Group security group will be used to control which users should have the ability to write and update data belonging to a specific project

All datahub users will have read access to the data by default. If there is a project specific requirement to control who can view the data, Read-Access-Group for that project should be used to control the access.



###Secrets
Sensitive information such as passwords and API Keys must be managed using Azure Vault and referenced as 'secrets' within the notebooks



## Database Naming Standards

1. Tables
2. Columns
3. Column properties

    1. Data type
    2. Data size
    3. Defaults
4. Referential Integrity properties

1. Integrity Constraints

    1. Primary Key
    2. Unique Constraint (Alternate Key)
    3. Foreign key
    4. CHECK constraints

1. Follow the Object Naming Standards described in (x.x) for naming the tables, columns, constraints and other constructs in for creating the database schema objects

1. Use underscore (\_) to separate terms in the name.

# Database Object Naming Standards

The following is a list of guidelines to be used for naming database objects:

Lowercase words Lowercase words represent a variable and should be substituted with the specific information.

| (vertical bar) | indicates an OR within the braces or brackets. Select only one option.

\&lt; \&gt; \&lt; \&gt; indicates required information

[] (brackets) [] indicates an optional item. You can code it or skip it.

\_ (underscore) \_ is a fixed character that must be used to separate the words for readability.

## Schema

A Schema is a logical container/owner and namespace for a set of database objects, such as tables, indexes, views

| **Format:** | **\&lt;prefix\&gt;\_\&lt;name\&gt;** |
| --- | --- |
| **Where:** | **prefix** : _two to three character abbreviation for the data storage environment_
_&lt;list of abbreviations TBD&gt;_

> **name**: _descriptive name or meaningful abbreviation for the schema_
> **Max Length:**  16 

## Table

Tables will be named so that developers can easily determine what type of data is stored in the table and which environment it belongs to.

| **Format 1:**** Format 2: **|** &lt;prefix&gt; _&lt;name&gt; **** &lt;prefix&gt;_&lt;name&gt;_&lt;suffix&gt;**
**Note:** _Format 2 applies to fact and dimension tables only._ |
| --- | --- |
| **Where:** | **prefix**  **:** _the abbreviation for the application or business area or subject area where the table belongs to. Mostly the abbreviations are one to five characters long._
**name**  **:** _is a descriptive name for the table_
_ **suffix** _ **:** _the abbreviation for the table type_ _ **&lt;table types TBD&gt;** _ |
| **Max Length:** | _24_ |

**Rules:**

Table name should by default be formed from the data model entity that it represents, in accordance with the naming conventions stated above.

Table names **will** :

- be unambiguous within its application span

- reflect the true content/usage of the table

- be singular
- make use of unambiguous and commonly understood abbreviations (to be listed in Appendix)
- be no more than 24 characters long including any prefixes or suffixes

Table names **will not** :

- use abbreviations that are ambiguous or difficult to decipher
- inaccurately describe the content or usage of the table
- contain superfluous descriptions such as &#39;table&#39; or &#39;file&#39; or &#39;data&#39;

## Column

Columns will be named so that developers can easily determine the use and possibly the type of the data stored.

| **Format 1:** | **\&lt;name\&gt;\_\&lt;class\_word\&gt;**
 |
| --- | --- |
| **Where:** | **name**  **:** _must be meaningful and describe for its purpose or content_
**class\_word** : _class word that indicates the type of data stored in the column. Please refer to the Class Word section below for the list._
 |
| **Max Length:** | _30_ |
| **Example:** | _AREA\_ID __ORDER\_DATE__ ACTIVE\_FLAG_
 |

**Rules:**

Column names **will** :

- be unambiguous within its table
- reflect the true content/usage of the column
- be singular
- make use of unambiguous and commonly understood abbreviations (provide a list in Appendix) if abbreviations are necessary
- be no more than 30 characters long including any prefixes or suffixes
- be unique within an application&#39;s set of tables

Column names **will not** :

- use abbreviations that are ambiguous or difficult to decipher
- inaccurately describe the content or usage of the column
- not include a table or application mnemonic, unless the column name is a reserved word

Column size **will** :

- be at least the same size (or larger) as the columns in the source system but not necessarily the same type

A class word describes the major classification of data associated with a data element. All attributes and columns are assigned a class word. The class word is used to classify the purpose of the attribute or column.

| Class word | Abbreviation | Definition |
| --- | --- | --- |
| Amount | AMT | A numeric measurement of monetary value. An amount attribute can be specified as an integer, may include decimal positions and may have a positive or negative value. For example, $23,943.00, $99, -$14.00. |
| --- | --- | --- |
| Amount (used in currency view only) | AMTL | A numeric measurement of monetary value expressed in local currency. |
| Amount (used in currency view only) | AMTR | A numeric measurement of monetary value expressed in a reporting currency. |
| Code | CD | A set of one or more user-defined values that represent a more meaningful and descriptive piece of business information. A code usually represents a static set of values. For example, &quot;C01&quot; may be the coded value for the description&quot;Calendar Year 2000 - Period 1&quot;.
**A code may be paired with a description, name or nothing at all (in cases where the code is meaningful).** |
| Count | CNT | An integer number that represents the counted value for some business event, programmatically calculated by a counter. |
| Date | DT | A point in time in terms of day, month, or year in any combination This includes calendar days (MMDDYYYY, YYYYMMDD) and fiscal dates. |
| Description | DESC | A word or phrase that interprets a, code. For example, &quot;Calendar Year 2000 - Period 1&quot; is the description for the coded value &quot;C01&quot;. |
| Duration | DUR | A numeric field that represents the time (greater than hours and minutes) during which something exists or lasts. |
| URL| URL| Standard URL |
| Email | EMAIL | Single email Address |
| Notes | NT | Long text for notes |
| Factor | FCTR | Numeric field expressing a real number other than a percentage value. For example, PRODUCT COST GROSSUP FACTOR might hold the numeric value that is used to calculated a grossed up product cost. |
| Identification / Identifier | ID | A unique label. Identifiers can often be classed as business or surrogate. A business identifier is a commonly used by a business unit. For example, a serial number used to identify a piece of EQUIPMENT. Business identifiers may have some intelligence. Surrogate identifiers usually do not have any meaning or intelligence; they merely provide a unique key. |
| Indicator | FLAG | A code that has only 2 domain values: Y or N. |
| Multiplier | MULT | An integer value that can hold 1 of 3 values: -1, 0, 1. Multipliers are used to derive other values. |
| Name | NAME | Character value used to identify or describe a business object or concept. This is usually a commonly used, descriptive name or title. It is often a proper name for example, SERVICE NAME, CUSTOMER NAME. The classword NAME can be paired with a code if it&#39;s deemed to be more meaningful. |
| Number | NUM | A value which is not for the purpose of measuring a quantity or expressing a percentage or factor, but which is usually a numeric value. Non-numeric characters could be contained in the value, such as in ACTIVITY NUMBER. For this reason, attributes with this class word are not normally subject to arithmetic. |
| Percentage (Percent) | PCT | Numeric field expressing a percentage. For example an attribute DISCOUNTED SALES PERCENTAGE might hold the percentage that is used to discount price of a product. |
| Quantity | QTY | An integer number that represents the counted value for some business event or other object. For example, TOTAL INVENTORY QUANTITY. |
| Rate | RT | A quantity, amount, or degree of something measured per unit of something else. An amount of payment or charge based on another amount; _for example_ the amount of premium per unit of insurance. |
| Ratio | RTO | The indicated quotient of two mathematical expressions. The relationship in quantity, amount, or size between two or more things. |
| Surrogate Key | SID | A unique identifier that does not have any meaning or intelligence. The SID is used for the unique identifiers of mart dimensions. |
| Text | TXT | Free form or unstructured text description. Text, unlike name and description, does not have any specific pre-defined purpose. |
| Indicator | IND | Binary value of 0 or 1 to be used as indicator |
| Time | TIME | A point in time or measurement stated in terms of hour, minute, second or fraction thereof in any combination. (HH:MM:SS, HHMM, HH, etc.) This does not include hours measured as a quantity, such as the number of hours it takes to fulfill a purchase order. |
| Timestamp | TS | A system generated date and time value that is used to record a system event. Often the timestamp is used for audit purposes. |
| Value | VAL | A numeric value that can be used in an arithmetic computation. |

## Other Naming Convention for Azure Resources

Environment Suffix:
- dev = Dev
- tst = Test
- prd = Production


| Azure Resource | Convention (env)| Example |
| --- | --- | --- |
| App Insight for App Service | dh-portal-insight-app-<i>env</i> | dh-portal-insight-app-<i>dev</i>|
| App Service for Portal Application | dh-portal-app-<i>env</i> | dh-portal-app-<i>dev</i> |
| Cosmos DB Account|dh-portal-cosmosdb-<i>env</i>|dh-portal-cosmosdb-<i>dev</i>|
| Function App for PowerShell | dh-portal-function-ps-<i>env</i>|dh-portal-function-ps-<i>dev</i>|
| Key Vault | datahub-key-<i>env</i>|datahub-key-<i>dev</i>|
| Log Analytics Workspace | dh-portal-log-<i>env</i>|dh-portal-log-<i>dev</i>|
| SQL Server | dh-portal-sql-<i>env</i>|dh-portal-sql-<i>dev</i>|
| SQL Database | dh-portal-<i>database_name</i>|dh-portal-<i>projectdb</i>|
| Search | dh-portal-search-<i>env</i>|dh-portal-search-<i>dev</i>|
| SignalR Service | dh-portal-signalr-<i>env</i>|dh-portal-signalr-<i>dev</i>|
| Storage Gen 2 | dhportaldatalake<i>env</i>|dhportaldatalake<i>dev</i>|
| Storage Account | dhportalstorage<i>env</i>|dhportalstorage<i>dev</i>|

# Power BI

## Workspace Names

Power BI Workspaces are expected to be shared between multiple users and/or groups.

> `<branch_name> - <project_acronym> - <project_description`

**Sample:** _HRB_ _-_ _HRWF_ _-_ _Workforce_

**Rules:**
- Avoid adding version numbers in the name
- Avoid adding references to a date (e.g. Year, Month, Day)