In the portal we have added three custom metrics to track file uploads to the datalake storage: file size, upload time and bytes per millisecond. They are saved in app insights under the metric names "FIleUploadSize, FIleUploadTime and FIleUploadBPMS".

I have create a custom dashboard names "Portal Uploads", in the dev azure instance, shared to everyone. I have added three charts as a sample of how to monitor file uploads in the portal. It is important to note, that the data is logged in app insights programmatically, this exercise is a manual process to setup up display of uploads information.

![image.png](/.attachments/image-d9e10877-6d85-428e-92d3-b5fe0f5b6e91.png)

The above displays the 3 charts, which is any area chart of the average file upload size, time and bpms. The following is a step by step instruction on how to add a new chart.


**STEP 1: Got to app insights resource, select Metrics tab**



![image.png](/.attachments/image-b2ff3d29-06b4-43e9-aba0-0332c9af5720.png)


**Step 2: Select the CUSTOM namespace "azure.appinsights" metric namespace**



![image.png](/.attachments/image-80c58803-b9cd-4335-97d0-4ec715e61585.png)


**STEP 3: Select the Metric to show**


![image.png](/.attachments/image-07af8b3b-36d3-49ef-82e4-cb433d01e4b2.png)


**STEP 4: Select the aggregation type**


![image.png](/.attachments/image-f501ee08-133a-48a4-a788-ec363a454a70.png)


**STEP 5: Select the chart type**


![image.png](/.attachments/image-6e9ab73c-a75a-483d-b43d-f3d9bd5645cc.png)


**STEP 6: Select Pin to dashboard**


![image.png](/.attachments/image-ae9b0c89-54e3-4019-9022-f5f904858930.png)



Select whether private or shared, and the specific dashboard to share to. You can optionally create a new dashboard as well.

**STEP 7: Select the dashboard to view**

![image.png](/.attachments/image-5ea63338-d46a-42a0-80d3-7ea29903676d.png)

**STEP 8: Select the "Portal Upload" dashboard, or the one you added to**

![image.png](/.attachments/image-fb043b5c-d331-4ebe-95d4-8e843d5e9ea7.png)
