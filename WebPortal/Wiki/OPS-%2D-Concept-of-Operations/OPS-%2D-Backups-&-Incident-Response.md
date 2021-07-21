## Backups

### Database Backup for MS SQL
- Handled by Azure services
- PITR (Point In Time Restore): Last 7 Days
- Weekly Backup: Last 12 Weeks
- Monthly backups: Last 12 Months
- Yearly backups: Keep week 26 for 2 Years
### Azure Cosmos DB
- Backed up every 8 hours
- Kept for 10 days

### Search Index
- There is currently no built-in index extraction, snapshot, or backup-restore feature in the Azure.

### File Storage Backups
- Datahub user storage manual backup. 
- Retention of 90 days.

### Azure App Services
- Daily backup kept for 30 days
- Function App is manually configured for daily backup kept for 30 days

### Storage for Data Projects
- 

## Information System Recovery and Reconstitution

### Datahub Infrastructure

Datahub Infrastructure can be deployed and reconstituted from the deployment scripts. The deployment includes the key data elements required to start the services.

### User Storage

User storage uses a custom backup solution.

### Data Project Storage

## Incident Response

### Definition

Communication Plan

The communication plan ensures NRCan is prepared to detect, respond to, and recover from outages in the DataHub system. The goal is to make sure the system is recovered as quickly as possible to limit impact to users and partners, and to reduce data loss.

The goal of this communication plan is to:

● Provide a clear roadmap for consistently handling incidents;

● Define the roles and responsibility in incident handling;

● Promote the same understanding and eliminate ambiguity during incident
handling;

● Make incident handling more smooth and effective.

### Roles and Responsibilities

● Azure is responsible for providing the cloud infrastructure to meet its expected SLA obligation. During expected and unexpected downtime, Azure is expected to make an announcement and notify NRCan of the downtime whenever possible. Azure is also expected to answer support calls from NRCan.

● DataHub operational support team is responsible for DataHub system support of existing applications. This team is the primary group for keeping the DataHub application up and running. It is also responsible for performing troubleshooting and recovery as needed.

● NRCan Cloud team has elevated privileges in NRCan\'s Azure cloud infrastructure. The DataHub operational support team may have to escalate to the NRCan cloud team if required.

● DataHub business clients are responsible for communicating to their clients and partners of the DataHub system.

● NRCan Help Desk intakes user inquiries and error reporting as well as sending broadcast messages for planning and unplanned system downtime.

### Contact Information

● Every team and team members must provide contact information and
availability.

Reporting Requirement

● All DataHub incidents are to be reported and consolidated to a central
record management system. The information must include time, scale of
impact, symptom, notification history and action taken.

Sources of Reporting

● From DataHub users;

● From automated monitoring mechanism;

● From 3rd party;

Incident Handling Flow

1\. Help desk receives incident reporting and evaluates the nature of
the issue to make sure it is not the user computing device. Otherwise,
the help desk provides assistance to the user or redirects to another IT
group.

2\. Help desk creates incidents and records the time of occurence, scale
of impact, symptom and forward to DataHub Operational Support team for
further troubleshooting.

3\. The DataHub Operational Support team receives incident reporting
from

a\. Help desk

b\. Automated monitoring mechanism

c\. DataHub business client

d\. Azure

e\. NRCan Cloud team

4\. The DataHub Operational Support team takes actions as necessary

a\. Perform user administration and change user settings

b\. Explore log entries

c\. Perform data fix as per hot fix procedure

d\. Perform service restoration from backups

5\. The DataHub Operational Support team may escalate to various NRCan
groups for further assistance

6\. The DataHub Operational Support team may notify the Help Desk of
known issues with the DataHub system. \<Some text here\>
