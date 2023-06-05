---
layout: default
title: Azure Functions Performance
nav_order: 2
---
# Performance of .NET workloads on Azure Functions
The scope of this API performance test was centered around two key technologies:
- Azure Functions (refer to the developer guide available [here](https://learn.microsoft.com/en-us/azure/azure-functions/){:target="_blank" rel="noopener"})
- Microsoft .NET core APIs (start your exploration [here](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api){:target="_blank" rel="noopener"})

## Key Findings
In our investigation, we discovered that 
- Higher-cost Azure Functions offer performance that exceeds the proportional increase in their price. For example, an S1 app function (63 euros per month at the end of May 2023) can accommodate 10 requests per second with good performance (this means that the 95% percentile responds in <100 ms), while a P3v3 Windows function (894 euros per month) has the capability to handle up to 1250 requests per second. With this tier, you get 125 times the performance for 14 times the price. In the [Final Considerations](#finalConsiderations) paragraph, we offer a "cost efficiency" analysis as a summary for this reasoning.
- Linux functions are less efficient in running .NET workloads compared to Windows functions. 
- However, Linux functions offer cost advantages, particularly in the P\*v3 tier, making them a viable and favorable choice in many cases when you can split your workload in more smaller app function instances. 

To gain more insights into the outcomes across different operating systems and tiers, please refer to the relevant section [below](#finalConsiderations).

## Measurement Approach
To evaluate performance, we deployed a .NET 7 isolated process API on various Azure Function tiers. This API was intentionally designed to perform the bare minimum, as the objective was to assess infrastructure rather than code. The tested API, named "GetFromCache" in the code snippet below, followed these steps:
1. Retrieval of a random string (from a set of 100) from an Azure Redis Cache located in the same virtual network as the Azure Function.
2. Creation of an in-memory object containing the retrieved string, a GUID, and another random string.
3. Serialization of the object and generation of a response containing the serialized object.

<script src="https://gist.github.com/RiccardoGMoschetti/af07f24520dbe62f1a2abecc4966c4d7.js"></script>

It is worth noting that typical APIs are unlikely to perform fewer operations than this; the question arises as to whether they should actually perform considerably more.

## Implications of the Results
Although your software and dependencies may differ from the ones tested here, this exercise provides an upper limit benchmark that even flawless software cannot surpass. We believe this data can assist you in making informed decisions.

## Load tool used
For generating a substantial number of concurrent calls, we utilized <a href="https://github.com/tsenart/vegeta">Vegeta</a>, a reliable and straightforward multi-platform tool, in version 12.8.3. The client machine responsible for generating the load was an Ubuntu 22.04 VM with 64 GB RAM and 8 CPUs, situated within the same network as the functions being tested.

## The infrastructure / architecture 
Our testing encompassed all production-ready Azure Function tiers available in the West Europe region, covering both Linux and Windows operating systems. The Azure functions were hosted in the same data center and virtual network as the VMs and the Redis Cache, utilizing private endpoints to ensure minimal network latency. The provided architectural diagram illustrates the solution's structure. 
<img src="https://github.com/RiccardoGMoschetti/apirehab/blob/dd723e412665ea3b43f35d68fc12c2b7089a2063/docs/images/Architecture-API-DotNet-On-Azure-Functions.drawio.png?raw=true"/>.  
You can obtain the original diagrams.net (formerly draw.io) drawing <a href="/docs/drawio/Architecture-API-DotNet-On-Azure-Functions.drawio" download>from this location</a>.

## Data from Vegeta
Vegeta provides you with the following information for each performance test: minimum value, mean, 50th percentile (considered more informative than the mean due to its lower susceptibility to outliers), 90th percentile, 95th, 99th, and maximum value. For our analysis, we subjected the functions to continuous stress for ten minutes.  
We categorized the performance as follows:

- <span style="color:darkGreen; font-weight:bold">Good</span> performance: In this category, 95% of API calls are completed in less than 100ms, indicating that only a small percentage of API calls may experience difficult response times.
- <span style="color:darkOrange; font-weight:bold">Mediocre</span>: Here, 95% of API calls are completed in less than 1 second, indicating that a small portion of API calls may be slow.
- <span style="color:darkRed; font-weight:bold">Barely working</span>: Although the server remains functional, each call may take up to 30 seconds to complete.

It is important to note that we did not include the B-tier app functions in this test, as Microsoft does not categorize them as production-ready.

<a id="theResults"></a>
# Results
## "S" tiers
### Linux tiers

<table>
   <th colspan="4">
      Maximum requests per second
   </th>
   <tr>
      <td>
         Tier
      </td>
      <td>
         <span style="color:darkGreen;font-weight:bold">Good Performance</span>
      </td>
      <td>
         <span style="color:darkOrange;font-weight:bold">Mediocre Performance</span>
      </td>
      <td>
         <span style="color:darkRed;font-weight:bold">Barely working</span>
      </td>
   </tr>
   <tr>
      <td>
       &nbsp;
      </td>
      <td>
         95th perc. < 100ms
      </td>
      <td>
         95th perc. < 1000ms
      </td>
      <td>
         (any 95th perc)
      </td>
   </tr>
   <tr>
      <td>
         Linux <em>S1</em>
      </td>
      <td>
         10
      </td>
      <td>
         30
      </td>
      <td>
         50
      </td>
   </tr>
   <tr>
      <td>
         Linux <em>S2</em>
      </td>
      <td>
         50
      </td>
      <td>
         100
      </td>
      <td>
         150
      </td>
   </tr>
   <tr>
      <td>
         Linux <em>S3</em>
      </td>
      <td>
         200
      </td>
      <td>
         300
      </td>
      <td>
         400
      </td>
   </tr>
</table>

### Windows tiers

<table>
   <th colspan="4">
      Maximum requests per second
   </th>
   <tr>
      <td>
         Tier
      </td>
      <td>
         <span style="color:darkGreen; font-weight:bold">Good Performance</span> 
      </td>
      <td>
         <span style="color:darkOrange; font-weight:bold">Mediocre Performance</span>
      </td>
      <td>
         <span style="color:darkRed; font-weight:bold">Bad Performance</span>
      </td>
   </tr>
   <tr>
      <td>
       &nbsp;
      </td>
      <td>
           95th perc. &lt; 100ms
      </td>
      <td>
            95th perc. < 1000ms
      </td>
      <td>
         (any 95th perc)
      </td>
   </tr>
   <tr>
      <td>
         Windows <em>S1</em>
      </td>
      <td>
         10
      </td>
      <td>
         50
      </td>
      <td>
         70
      </td>
   </tr>
   <tr>
      <td>
         Windows <em>S2</em>
      </td>
      <td>
        100
      </td>
      <td>
         150
      </td>
      <td>
         175
      </td>
   </tr>
   <tr>
      <td>
         Windows <em>S3</em>
      </td>
      <td>
         325
      </td>
      <td>
         350
      </td>
      <td>
         400
      </td>
   </tr>
</table>

### Prices

As of May 2023, the following are the monthly prices for the different tiers and operating systems:

| plan |CPUs|RAM GB|Storage|€/month|€/month|
|      |    |      |       | Linux |Windows|
|------|----|------|-------|-------|-------|
| *S1* |  1 | 1.75 |    50 |    63 |    66 |
| *S2* |  2 | 3.50 |    50 |   126 |   132 |
| *S3* |  4 | 7.50 |    50 |   251 |   264 |
         
From the above information, it can be inferred that Windows workloads in the S* tier provide higher request capacities than Linux, albeit at a slightly higher price. Therefore, unless there are specific reasons to opt for Linux functions with .NET workloads, Windows S* tiers seem to be preferable to Linux S* tiers.

### "P*V2" tiers

These tiers represent the second generation of "premium" app services. They offer increased memory and significantly improved performance compared to the "S" tiers. For most workloads not requiring the P\*v3 tiers (which also provide long-term discounts not available for P\*v2 tiers), the P\*v2 tiers are a suitable choice.

Linux P\*v2 functions:

### Linux tiers

<table>
   <th colspan="4">
      Maximum requests per second
   </th>
    <tr>
      <td>
         Tier
      </td>
      <td>
         <span style="color:darkGreen; font-weight:bold">Good Performance</span> 
      </td>
      <td>
         <span style="color:darkOrange; font-weight:bold">Mediocre Performance</span>
      </td>
      <td>
         <span style="color:darkRed; font-weight:bold">Bad Performance</span>
      </td>
   </tr>
   <tr>
      <td>
       &nbsp;
      </td>
      <td>
         95th perc. < 100ms
      </td>
      <td>
         95th perc. < 1000ms
      </td>
      <td>
         (any 95th perc)
      </td>
   </tr>
   <tr>
      <td>
         Linux <em>P1v2</em>
      </td>
      <td>
         50
      </td>
      <td>
         75
      </td>
      <td>
        150
      </td>
   </tr>
   <tr>
      <td>
         Linux <em>P2v2</em>
      </td>
      <td>
         250
      </td>
      <td>
         350
      </td>
      <td>
         400
      </td>
   </tr>
   <tr>
      <td>
         Linux <em>P3v2</em>
      </td>
      <td>
         600
      </td>
      <td>
         750
      </td>
      <td>
         850
      </td>
   </tr>
</table>

### Windows tiers

<table>
   <th colspan="4">
      Maximum requests per second
   </th>
 <tr>
      <td>
         Tier
      </td>
      <td>
         <span style="color:darkGreen; font-weight:bold">Good Performance</span> 
      </td>
      <td>
         <span style="color:darkOrange; font-weight:bold">Mediocre Performance</span>
      </td>
      <td>
         <span style="color:darkRed; font-weight:bold">Bad Performance</span>
      </td>
   </tr>
   <tr>
      <td>
       &nbsp;
      </td>
      <td>
           95th perc. &lt; 100ms
      </td>
      <td>
            95th perc. < 1000ms
      </td>
      <td>
         (any 95th perc)
      </td>
   </tr>
   <tr>
      <td>
         Windows <em>P1v2</em>
      </td>
      <td>
         70
      </td>
      <td>
         175
      </td>
      <td>
         200
      </td>
   </tr>
   <tr>
      <td>
         Windows <em>P2v2</em>
      </td>
      <td>
        325
      </td>
      <td>
         475
      </td>
      <td>
         475
      </td>
   </tr>
   <tr>
      <td>
         Windows <em>P3v2</em>
      </td>
      <td>
         800
      </td>
      <td>
         850
      </td>
      <td>
         900
      </td>
   </tr>
</table>

### Prices

|*plan*  |CPUs|RAM GB|Storage|€/month|€/month|
|        |    |      |       | Linux |Windows|
|--------|----|------|-------|-------|-------|
| *P1v2* |  1 | 3.50 |   250 |    76 |   132 |
| *P2v2* |  2 | 7.50 |   250 |   152 |   264 |
| *P3v2* |  4 |14.00 |   250 |   305 |   529 |
 
You can see here that Windows tiers can get very expensive but also very performant. A Windows P3v2 function will serve more than 900 requests per second!

### "P*v3" tiers

The P\*v3 tiers represent the latest and best Azure app service plans.  
They offer superior performance compared to the v2 counterparts and allow for long-term reservation discounts if committed for one or three years. This makes them an excellent choice when future app service requirements are expected, even if not necessarily for the same purposes as the present.

Findings for the  P\*v*3* functions:

### Linux tiers

<table>
   <th colspan="4">
      Maximum requests per second
   </th>
    <tr>
      <td>
         Tier
      </td>
      <td>
         <span style="color:darkGreen; font-weight:bold">Good Performance</span> 
      </td>
      <td>
         <span style="color:darkOrange; font-weight:bold">Mediocre Performance</span>
      </td>
      <td>
         <span style="color:darkRed; font-weight:bold">Bad Performance</span>
      </td>
   </tr>
   <tr>
      <td>
       &nbsp;
      </td>
      <td>
         95th perc. < 100ms
      </td>
      <td>
         95th perc. < 1000ms
      </td>
      <td>
         (any 95th perc)
      </td>
   </tr>
   <tr>
      <td>
         Linux <em>P1v3</em>
      </td>
      <td>
         250
      </td>
      <td>
         350
      </td>
      <td>
        300
      </td>
   </tr>
   <tr>
      <td>
         Linux <em>P2v3</em>
      </td>
      <td>
         250
      </td>
      <td>
         350
      </td>
      <td>
         400
      </td>
   </tr>
   <tr>
      <td>
         Linux <em>P3v3</em>
      </td>
      <td>
         750
      </td>
      <td>
         850      
      </td>
      <td>
         1000
      </td>
   </tr>
</table>

### Windows tiers

<table>
   <th colspan="4">
      Maximum requests per second
   </th>
 <tr>
      <td>
         Tier
      </td>
      <td>
         <span style="color:darkGreen; font-weight:bold">Good Performance</span> 
      </td>
      <td>
         <span style="color:darkOrange; font-weight:bold">Mediocre Performance</span>
      </td>
      <td>
         <span style="color:darkRed; font-weight:bold">Bad Performance</span>
      </td>
   </tr>
   <tr>
      <td>
       &nbsp;
      </td>
      <td>
           95th perc. &lt; 100ms
      </td>
      <td>
            95th perc. < 1000ms
      </td>
      <td>
         (any 95th perc)
      </td>
   </tr>
   <tr>
      <td>
         Windows <em>P1v3</em>
      </td>
      <td>
        325
      </td>
      <td>
         425
      </td>
      <td>
         450
      </td>
   </tr>
   <tr>
      <td>
         Windows <em>P2v3</em>
      </td>
      <td>
        900
      </td>
      <td>
         1000
      </td>
      <td>
         1050
      </td>
   </tr>
   <tr>
      <td>
         Windows <em>P3v2</em>
      </td>
      <td>
         1250
      </td>
      <td>
         1250
      </td>
      <td>
         1250
      </td>
   </tr>
</table>

### Prices

|*Tier* |CPUs| RAM |Storage|As you go|3 years|As you go |3 years |
|       |    |     |       | Linux   | Linux |Windows   |Windows |
|-------|----|-----|-------|---------|-------|----------|--------|
| *P1v3*|  2 | 8 GB| 250 GB|      118|     53|      223 |    134 |
| *P2v3*|  4 |16 GB| 250 GB|      235|    106|      447 |    268 |
| *P3v3*|  8 |32 GB| 250 GB|      471|    212|      894 |    537 |


You can see that Linux workloads definitely less expensive than the Windows. The difference is more remarkable than in the other tiers.  
If there is a need to accommodate a substantial volume of requests and there is a willingness to commit for a duration of one or three years, opting for a Linux P*V3 app service emerges as a highly evident decision.

<a id="finalConsiderations"></a>
# Final consideration
Although Windows tiers demonstrate superior performance compared to Linux tiers, the latter exhibit a more favorable cost-effectiveness ratio.  
This cost efficiency value represents the relationship between the maximum number of requests per second (ensuring a response time within the 95th percentile of less than 100 ms) and the monthly cost. A higher cost efficiency implies greater value for the investment. For example, if there is a requirement for 1000 requests per second, it is more advantageous to opt for four P1v3 Linux tiers, each capable of serving 250 requests per second and resulting in a monthly cost of 472 euros (118 euros per tier x 4), instead of selecting a single P3v3 tier, which can handle up to 1250 requests per second but incurs a monthly cost of 894 euros.  
Additionally, it is generally advisable to utilize multiple smaller instances rather than a single larger instance. This approach mitigates the impact of instance failures, as a single large instance would render the entire application unavailable until it becomes operational again, whereas several smaller instances ensure the availability of the other instances during the downtime of a faulty instance.
<table>
 <th colspan="5">Tier efficiency</th>
 <tr><td>Tier</td><td><span style="color:darkGreen; font-weight:bold">OS</span></td><td><span style="color:darkOrange; font-weight:bold">maxium req/s for Good Performance</span></td><td>Cost per month</td><td>Cost efficiency</td></tr>
   <tr><td>P2V3</td><td>Linux</td><td>750</td><td>235</td><td><em>3,19 (best)</em></td></tr>
   <tr><td>P1V3</td><td>Linux</td><td>250</td><td>118</td><td>2,12</td></tr>
   <tr><td>P2V3</td><td>Windows</td><td>900</td><td>447</td><td>2,01</td></tr>
   <tr><td>P3V2</td><td>Linux</td><td>600</td><td>305</td><td>1,97</td></tr>
   <tr><td>P3V3</td><td>Linux</td><td>900</td><td>471</td><td>1,91</td></tr>
   <tr><td>P2V2</td><td>Linux</td><td>250</td><td>152</td><td>1,64</td></tr>
   <tr><td>P3V2</td><td>Windows</td><td>800</td><td>529</td><td>1,51</td></tr>
   <tr><td>P1V3</td><td>Windows</td><td>325</td><td>223</td><td>1,46</td></tr>
   <tr><td>P3V3</td><td>Windows</td><td>1250</td><td>894</td><td>1,4</td></tr>
   <tr><td>P2V2</td><td>Windows</td><td>325</td><td>264</td><td>1,23</td></tr>
   <tr><td>S3</td><td>Windows</td><td>325</td><td>264</td><td>1,23</td></tr>
   <tr><td>S3</td><td>Linux</td><td>200</td><td>251</td><td>0,8</td></tr>
   <tr><td>S2</td><td>Windows</td><td>100</td><td>132</td><td>0,76</td></tr>
   <tr><td>P1V2</td><td>Linux</td><td>50</td><td>76</td><td>0,66</td></tr>
   <tr><td>P1V2</td><td>Windows</td><td>70</td><td>132</td><td>0,53</td></tr>
   <tr><td>S2</td><td>Linux</td><td>50</td><td>126</td><td>0,4</td></tr>
   <tr><td>S1</td><td>Linux</td><td>10</td><td>63</td><td>0,16</td></tr>
   <tr><td>S1</td><td>Windows</td><td>10</td><td>66</td><td>0,15</td></tr>
</table>
