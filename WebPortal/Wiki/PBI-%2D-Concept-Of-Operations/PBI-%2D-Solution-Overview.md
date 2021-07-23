# PowerBI Solution overview

## Solution Architecture with Desktop Data Source
![image.png](/.attachments/image-7e5d0f59-bc21-442c-b7f7-0e0568ed8f6c.png)
In this solution, the data source is a file (e.g. Excel, MS Access database, CSV) supported by PowerBI Desktop that will be used as a single point in time snapshot of the data used in the report.

## Solution Architecture with DataHub Data Source
![image.png](/.attachments/image-b049ef4d-e2e0-4511-9a9e-3274d6277afa.png)

This alternative architecture, relies on databases or blob storage that is stored in the DataHub Azure resource group. 