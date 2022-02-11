
# What is the SSC DataHub?

The SSC DataHub is a **Proof of Concept for the Federal Science DataHub enterprise capabilities.**

The SSC DataHub is an enterprise platform for storing, working with and collaborating on data initiatives across the GC and with external partners. It is a central location for users to store any kind of data, perform collaborative analysis, manipulate data using advanced analytics tools, and conducting data science experiments.

![image](https://user-images.githubusercontent.com/82101285/129956914-9ebe7b07-25dd-4c2c-9da0-d8e8a2499c78.png)


![image](https://user-images.githubusercontent.com/82101285/122599381-48a01d00-d03c-11eb-9bb8-a20d76646258.png)

## Data Projects

The DataHub makes it easy for multiple teams, labs or users to get access to ETL, Data Science or Analytical tools.

The web interface lets users browse data project, request access and work with the following tools:
- **PowerBI Workspaces:** Each data project can be associated with its own workspace.
- **Storage Explorer & Databricks:** For data science projects, a separate storage account and a databricks workspace are created. The portal includes a user friendly drag and drop user interface to browse the account, upload and download files.
- **Form Builder**: DataHub include a data model manager that lets user design data models which can be converted into Entity Framework Models and connected to Blazor Forms.
- **Data Entry**: Once deployed into a web application, the forms designed with the form builder can be accessed for each data project and convert complex legacy spreadsheets into user friendly web applications.

## System Architecture

The diagram below shows the key components of the platform

![image](https://user-images.githubusercontent.com/82101285/122604469-fd8a0800-d043-11eb-8e51-e1a3b3325ee2.png)

# Github Structure

This project includes multiple repositories
- **DataHub Web Portal:** This repository contains the code for all the portal and Azure Functions used to automate PowerBI & Databricks tasks
- **DataHub Terraform:** The terraform infrastructure for the project is stored in this repository and elements of the terraform script are dynamically generated from the Data Project database. Please contact us for details on the terraform infrastructure.
- **DataHub Databricks:** Databricks is used in this project for ETL, Data Science and other data transformations. Examples from this repository can be used as template for setting up new tasks.



