
**What are the tools and services available in Datahub that I can leverage for my data analytics project?**
Datahub Portal – single place to manage your datasets (include proper description)
Databricks – use this service to prepare, explore, and analyze your data as well as build AI models, all with the use of on-demand distributed computing engine (Clusters)
Power BI – use this service to explore, analyze and build advanced visualization with your data
Data Factory – use this service to orchestrate and schedule the data pipelines
Azure Blob Storage – cost effectively store structured, semi-structure and unstructured data in the cloud

**What is Databricks?**
Databricks is a cloud based analytics platform that enables you to work with data using the Apache Spark Framework. In another words, it provides an easy to use interface for your data related tasks to be seamlessly executed on multiple virtual machines (clusters) in parallel. 
It is also integrated with other cloud services such as Azure Blob Storage and databases, which makes it easy to securely store and access your data within Databricks.

**What are the programming languages supported in Databricks?**
The following languages are supported: Spark SQL, Java, Scala, Python, R and standard SQL.
It offers the opportunity for you to select the language that your developers are comfortable with for your project. Furthermore, Databricks provides the capability for developer to use multiple languages in single notebook.

**My  team is currently using basic Python/R for data analysis, how easy is it to transition the skills to use Python for Spark (PySpark) and R for Spark (SparkR)?**
Syntax and the methodologies used by PySpark and SparkR are geared towards idea of multiple computers executing the code that you have written, so their syntax differs to some extend from regular Python and R. Therefore, it may require some training to transition the skills to use the Spark version of the languages. Generally speaking, the transition to use PySpark and SparkR are not that complicated.

**My data project is not a complex and does not require the use of Spark or parallel computing power, can I still use the Databricks for this project?**
Absolutely, yes. Databricks provides the capability to write your code using regular Python or R language and use pandas, and other popiular libraries. It also provides capabilities to execute your code on single virtual machine (single cluster), with just minimal computing resource required for your project.

**How can I upload my own dataset and use in Databricks?**
You can upload and manage your dataset using the Datahub Portal. The uploaded data will be stored in in the cloud using the Azure Blog Storage. To access the uploaded file programmatically using Databricks, you will need to obtain the path where the file is stored. The path of your uploaded file can be obtained by going to the portal and…….…
Here is a sample code that shows how to access a data that is stored in Blob Storage using python in Databricks:


**Can I create my own Cluster in Databricks?**
No. For the time being, the Clusters that are  required for your project will created and configured by the Datahub team based on your requirement. You will be provided with the controls to start and shut down the clusters. 

**Is the Databricks environment certified to handle Protected B information?**
No, currently, the Databricks environment supported by NRCAN Datahub only supports up to Protected A level.

**Can I access the Databricks environment using locally running Jupyter notebook or IDEs such as PyCharm,VS Code, and Spyder?**
Yes, you can. However, it requires some configuration to be done on the Databricks. Please contact us with your requirements and we will be happy assist you with this.

**How is my data and the source code protected from unauthorized access?**
The Datahub Analytics environments follow the security principle of “Least Privilege”, which means only the minimum set of privileges required to perform the tasks will be granted to the users. 
Access controls and procedures will be setup to ensure that any source code that is created within your Databricks project can be viewed and executed by only those users that have been authorized to do so. 
Similarly, any data stored in the Azure Blog Storage account for your project will be managed by using folder level access control to ensure that reading and writing to the files and folders can be done by only those users that have been explicitly granted access to do so.

**Who will be responsible for setting up the Databricks resources (folders, clusters, access groups and users)Blob Storage Account and the Access Controls pertaining to these components?**
Configuration of the Databricks resources and the storage account will be managed by the Datahub team. 

**Other than configuring the resources, what other services can I expect from the Datahub team in support of my project?**
We provide technical as well as architectural guidance for any data that needs to be stored and shared at the department level. 

**We have completed analysing and exploring the data in Databricks, and now we would like to have a data pipeline built to regularly update and store this data in persistent storage so it can be shared across the department, what is the next step?**
If you have the resources and would like to implement a production ready solution to update the data regularly, Datahub team will be happy to support you by configuring the environment as well as provide guidance and technical advice. 
In the event that you do not have the resources to implement such a solution, we will be happy to work with you to gather the requirements and implement the solution on your behalf.
For storing and sharing data that requires frequent access for production use, we recommend the use of Delta Lake architecture. 

**I have an AI/Machine Learning project that I would like to implement using Databricks, how do I get started?**
At the moment, we will not be taking on the responsibility of building the ML Models, however, we will be happy to assist you with providing the technical tools and services necessary to achieve your project objective.
Link to the project intake form here….

**What are the type of costs that my analytics project may incur?**
At high level, the costs for your project will depend on the following 3 factors:
1.	Compute – The type of virtual machines (Databricks Clusters) that you require for the project. 

2.	Storage – The amount of data stored using the Blob Storage service

3.	Cost of any additional services that you may require for your project such as license cost for Power BI, Tableau, SQL Sever Database etc.


**Where can I see the costing model for the services?**
How can I track the cost that is being incurred for my project?
Reports and alerts will be configured to easily enable you to track the ongoing usage and cost of the services for your project.

Will the full or part of the cost associated with my project be covered by Datahub?
In some cases Datahub maybe able to cover part of the cost for your project. Please contact us for more details.








