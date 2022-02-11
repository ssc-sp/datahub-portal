
# What is the SSC DataHub?

The SSC DataHub is a **Proof of Concept for the Federal Science DataHub enterprise capabilities.**

The SSC DataHub is an enterprise platform for storing, working with and collaborating on data initiatives across the GC and with external partners. It is a central location for users to store any kind of data, perform collaborative analysis, manipulate data using advanced analytics tools, and conducting data science experiments.


![image](https://user-images.githubusercontent.com/99416857/153608163-8debe2cf-cc72-4c40-bb2a-456065ff3783.png)


## Data Projects

The DataHub makes it easy for multiple teams, labs or users to get access to ETL, Data Science or Analytical tools.

The web interface lets users browse data project, request access and work with the following tools:
- **Storage Explorer and Databricks:** separate storage account and a databricks workspace are created for each project. The portal includes a user friendly drag and drop user interface to browse the account, upload and download files. _(Note - Databricks to be enabled at later point in the PoC)._

## System Architecture

The diagram below shows the key components of the platform

![image](https://user-images.githubusercontent.com/99416857/153606851-5cf7d092-8f62-46fb-8a02-26d032ed6f2e.png)


# Github Structure

This project includes multiple repositories
- **DataHub Web Portal:** This repository contains the code for all the portal and Azure Functions used to automate Databricks tasks.
- **DataHub Terraform:** The terraform infrastructure for the project is stored in this repository and elements of the terraform script are dynamically generated from the Data Project database. Please contact us for details on the terraform infrastructure.
- **DataHub Databricks:** Databricks is used in this project for ETL, Data Science and other data transformations. Examples from this repository can be used as template for setting up new tasks.



