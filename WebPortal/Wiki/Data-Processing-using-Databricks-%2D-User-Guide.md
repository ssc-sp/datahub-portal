# Data Processing Using Spark and Databricks - User Guide

Please note the programming language used for this user guide is provided mostly Python, however, you may visit the website provided under each section for examples using other languages such as Scala, and R.

## Access files from Data Lake storage and load it into a spark dataframe 

df=spark.read.format('csv').option("header","true").load('abfss://databricks-demo@datahubdatalakedev.dfs.core.windows.net/demo/export-2.csv')

<databricks-demo> is the container
<datahubdatalakedev> is the blob storage account

https://docs.databricks.com/data/data-sources/azure/azure-storage.html

## Read data from excel file
projectsDf = spark.read.format("com.crealytics.spark.excel").option("sheetName", fiscal_year + " Projects").option("dataAddress", "A5").option("Header", False).load("dbfs:/mnt" + vMountDirectory + vFileName)

*Requires crealytics library to be installed on the cluster



## Frequently used Spark SQL Commands

#### Select rows from a dataframe by filtering: 
df.select("GEO","Value","Date").where("value >6000").show()

#### Filter rows with not null values:
dfND = df.where(df["VALUE"].isNotNull())

####Group by a column and get the count of rows:
dfCountByLF=dfND.groupBy("Labour_force_characteristics").count()

####Rename columns:
renamedDf = df.withColumnRenamed("LName","Last_Name")

https://spark.apache.org/docs/latest/sql-programming-guide.html


## Use of different programming languages within the same notebook (Python, sql, scala, R)

#### Convert Spark Dataframe to regular Pandas(Python) dataframe: 
import pandas
pandasDf = df.select("*").toPandas()

#### Create a temporary table (view) from a dataframe and query the view using sql
dfFullTimeEmp.createOrReplaceTempView("vw_dfFullTimeEmp")

SELECT sum(value) Employment, NAICS FROM vw_dfFullTimeEmp 
WHERE NAICS <> 'Total, all industries'
GROUP BY NAICS 
ORDER BY Employment desc

#### Convert the dataset to a SparkR dataframe 
%r
library(SparkR)
sdrf2<-sql("SELECT * FROM vw_dfFullTimeEmp ")


## Store data 

#### Create persistent table in the Data Lake 
CREATE TABLE IF NOT EXISTS DEMO_LABOUR_FORCE 
USING DELTA
LOCATION 'abfss://databricks-demo@datahubdatalakedev.dfs.core.windows.net/demo/tbl/'
AS SELECT * FROM vw_dfFullTimeEmp

#### Visit previous version of the delta lake table using sql 
SELECT * FROM DEMO_LABOUR_FORCE TIMESTAMP AS OF '2021-02-22T22:32:33.000+0000'

#### Write data to SQL Server datbase
server_name = "jdbc:sqlserver://sqlserver-ciosb-datahub.database.windows.net"
database_name = "spi-drf-db"
url = server_name + ";" + "databaseName=" + database_name + ";"

username = <db username>
password = dbutils.secrets.get(scope = "datalake-key-dev", key = "sqladmin-pswd-ciosb-datahub")

table_name_project_tracker = "EA_PROJECT_TRACKER"
projectsDf_with_uniqueID.write.format("jdbc").mode("overwrite").option("url", url).option("dbtable", table_name_project_tracker).option("user", username) .option("password", password).save()





