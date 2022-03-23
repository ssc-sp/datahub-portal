/****** Script for SelectTopNRows command from SSMS  ******/
INSERT into Project_Storage
SELECT newid(),[Param1]
	  ,1 as Storage_Type
      ,r.[Project_ID]	  
  FROM [dbo].[Project_Resources] r,[dbo].[Projects] p
  where ResourceType = 'storage' and r.Project_ID = p.Project_ID
  --and Project_ID <> null

update r set
  r.[Project_StorageId] = s.id
  from [Project_Requests] r, Project_Storage s
  where ServiceType = 'storage' and r.Project_ID = s.Datahub_ProjectProject_ID

