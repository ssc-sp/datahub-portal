## DataHub Portal File Upload Performance Issue

Bug 2086

### Definition

<center>

::: mermaid
 graph LR;
 A[Web<br>Browser] --> B
 A --> V["VPN<br>(Scenario #1)"]
 V --> B
 B["Azure WAF<br>(Scenario #2)"] --> C["Azure App Service"]
 A --> D["Gen 2 Storage Account<br>(Scenario #4)"]
 A --> E["VPN<br>(Scenario #3)"]
 E --> D
 T["Telework<br>(Scenario #5)"] --> B
 Q["Telework<br>(Scenario #6)"] --> D
 A --> T
 A --> Q
:::

</center>


### Definition

We upload a 100MB file over home internet connection with 50Mbps upload speed. Each scenario is performed 3 times and the average is taken.

### Results

| Scenario # | Descrition | Shortest Time | CPU Usage | Bandwidth Usage |
| --- | ----------- | --- | --- | --------- |
| 1 | Upload to App with VPN | 13'45" | 20% | 1.5 Mbps |
| 2 | Upload to App without VPN | 3'45" | 30% | 5 Mbps |
| 3 | Upload to Storage Account with VPN | 3'15" | 30% | 4.5 Mbps |
| 4 | Upload to Storage Account without VPN | 1'10" | 25% | 14 Mbps |
| 5 | Telework upload to App | 1'49" | 32% | 10 Mbps |
| 6 | Telework upload to Storage Account | 8" | 50% | 100 Mbps |

### Tools

\<Some text here\>

### Process and Procedures

\<Some text here\>

