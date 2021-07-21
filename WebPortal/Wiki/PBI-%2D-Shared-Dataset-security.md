
PowerBI shared datasets can be used to serve as a data source for other reports. Multiple reports can be created using the same data source, and the reports can be in different workspaces.
When creating a report based on a dataset in a workspace other that the one where the dataset exists, a link to the original dataset will be created in the report workspace. Although the powerBI portal will make that link look just like a regular dataset, it is in fact only a link that cannot be edited, and is automatically deleted when the reports referencing the dataset are deleted.

In order to be able to view a report that is sourced from a particular dataset a user needs to have both
                1. Read access to the report (which can be in a different workspace than the dataset itself) AND
                2. Read access to the dataset; Read access to a dataset can be granted (and inherited) at workspace level or at the individual report level. A user who has View access at the report level but no access at the data source level will be able to see that the report exists, but not run it.

In order to create new reports based on a dataset a user needs to 
                1. have permissions to create new reports in the workspace where the new report is to be created AND
                2. have permissions to use that dataset as a data source.
                Permission to use a dataset as a datasource for new reports can be granted in 2 ways, either
                a. at dataset level (grant Build permission) OR
                b. inherited from the worspace (user needs to be either a Member, a Contributor or an Admin of the workspace where the dataset resides)
