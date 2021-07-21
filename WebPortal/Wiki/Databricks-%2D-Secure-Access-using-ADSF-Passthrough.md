# Summary
- Datahub recommends using AD Credential Pass-through feature of Databricks to ensure the access from Databricks to the Storage Accounts is secure 
- AD Credential Passthrough is a feature available in databricks to configure access control using the user’s AD identify and permissions
- AD credential of the Databricks user is passed through to the Azure Datalake Gen 2 Storage and evaluated against the files and folder ACL
- It is recommended to use AD Groups to segregate the responsibilities of group with the same access 
- Any user using the cluster with AD Credential enabled will be evaluated to if that user has appropriate permission to perform the action against the file/folder

# Databricks AD Credential Passthrough

![image.png](/.attachments/image-362c5896-3e0e-469c-80b6-557bb1184ebc.png)

![image.png](/.attachments/image-9ea2d0e1-44ef-4636-abef-0975c6c846b6.png)
