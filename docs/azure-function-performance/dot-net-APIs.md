---
layout: default
title: Azure Functions Performance
nav_order: 2
---
# Test scope
The scope of this API performance test was centered around two key technologies:
- Azure Functions (refer to the developer guide available [here](https://learn.microsoft.com/en-us/azure/azure-functions/){:target="_blank" rel="noopener"})
- Microsoft .NET core APIs (start your exploration [here](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api){:target="_blank" rel="noopener"})

## Key Findings
In our investigation, we discovered that higher-cost Azure Functions generally deliver better performance. It is important to note that Linux functions are less efficient in running .NET workloads compared to Windows functions. However, Linux functions offer cost advantages, particularly in the P\*v3 tier, making them a viable and favorable choice. To gain more insights into the outcomes across different operating systems and tiers, please refer to the relevant section [below](#finalConsiderations).

## Measurement Approach
To evaluate performance, we executed .NET 7 isolated process APIs on various Azure Function tiers. These APIs were intentionally designed to perform the bare minimum, as the objective was to assess infrastructure rather than code. The tested API, named "GetFromCache" in the provided code snippet, followed these steps:
1. Retrieval of a string (from a set of 100) from an Azure Redis Cache located in the same virtual network as the Azure Function.
2. Creation of an in-memory object containing the retrieved string, a GUID, and another random string.
3. Serialization of the object and generation of a response containing the serialized object.

It is worth noting that typical APIs are unlikely to perform fewer operations than this; the question arises as to whether they should actually perform considerably more.

Here is a snippet of the code used for the "GetFromCache" API:
<script src="https://gist.github.com/RiccardoGMoschetti/af07f24520dbe62f1a2abecc4966c4d7.js"></script>

## Implications of the Results
Although your software and dependencies may differ from the ones tested here, this exercise provides an upper limit benchmark that even flawless software cannot surpass. We believe this data can assist you in making informed decisions.

## Load tool used
For generating a substantial number of concurrent calls, we utilized Vegeta, <a href="https://github.com/tsenart/vegeta">Vegeta</a>, a reliable and straightforward tool. We employed version 12.8.3, as the latest version appeared to lack compatibility with ARM64 architecture. The client machine responsible for generating the load was an Ubuntu 22.04 VM with 64 GB RAM and 8 CPUs, situated within the same network as the functions being tested.

## The infrastructure / architecture 
Our testing encompassed all production-ready Azure Function tiers available in the West Europe region, covering both Linux and Windows operating systems. The Azure functions were hosted in the same data center and virtual network as the VMs and the Redis Cache, utilizing private endpoints to ensure minimal network latency. The provided architectural diagram illustrates the solution's structure. 
<img src="https://github.com/RiccardoGMoschetti/apirehab/blob/dd723e412665ea3b43f35d68fc12c2b7089a2063/docs/images/Architecture-API-DotNet-On-Azure-Functions.drawio.png?raw=true"/>.  
You can obtain the original diagrams.net (formerly draw.io) drawing <a href="/docs/drawio/Architecture-API-DotNet-On-Azure-Functions.drawio" download>from this location</a>.

## What is better, Linux or Windows?
They are very similar, except for the P\*v2 tiers, where Windows is definitely better. For the P\*v3 tiers, Linux is more comparable.  
Linux is always *cheaper* (especially in the P\*v3 tiers). If your application is stateless, more Linux resources can give you better performance at lower prices and they can definitely be used rather than Windows.

## The data made available by Vegeta
For every tier of the available Azure Functions, Vegeta gave us the following information: minimum value, mean, 50th percentile (which we consider more revealing than the mean, as the latter is more influenced by outliers, 90th percentile (the data that we considered more important), maximum value.
For our purposes, we focused on the 95th percentile and we stressed the functions for ten minutes straight. 
We then categorized the performance into:

- <span style="color:darkGreen; font-weight:bold">Good</span> performance: at that rate, 95% of API calls take less than 100ms. This means that  only 5% of API calls won't be very fast.
- <span style="color:darkOrange; font-weight:bold">Mediocre</span>: 95% takes less than 1 second. This means that 5% of API calls will be slow.
- <span style="color:darkRed; font-weight:bold">Barely working</span>: the server won't break, but it will take up to 30 seconds per call to do so.

In this test, we did not use B tiers of app functions, as they are not reported as production ready by Microsoft.

<a id="theResults"></a>
# The results
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

At the end of May, 2023, these were the prices for the different tiers and operating systems:

| plan |CPUs|RAM GB|Storage|€/month|€/month|
|      |    |      |       | Linux |Windows|
|------|----|------|-------|-------|-------|
| *S1* |  1 | 1.75 |    50 |    63 |    66 |
| *S2* |  2 | 3.50 |    50 |   126 |   132 |
| *S3* |  4 | 7.50 |    50 |   251 |   264 |
         
From this picture you can gather than Windows workloads, in the S* tier, allow you more requests than Linux, at a slightly increased price. 
This means that, <em>unless you have particular reasons to choose Linux functions with .NET workloads, Windows S* tiers are preferrable to Linux S* tiers.</em>

### "P*V2" tiers

These tiers are the second generation of "premium" app services.  
Compared to "S" tiers, they have more memory and perform much better. They can still be your choice for most workloads where you don't need the P\*v3 tiers (which also give you long-term discount which are not allowed for P\*v2 tiers).

Findings for the Linux P\*v2 functions:

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
 
You can see here that Windows tiers can get very expensive but also _very_ performant. A Windows P3 function will serve more than 900 requests per second.

### "P*v3" tiers

These are the best newest in the Azure app service plans.  
They perform better than the P2 counterparts and they allow for "reservation" (long term discounts) if you commit to use them for one or three years (the longer the commitment, the stronger the discount). This makes them a very good fit when you know you are going to need app services in the next year (even if not for the same purposes as today: nothing prevents you from buying an app service now and use it with different domain names in the future, while still taking advantage of the same discount.)

Findings for the  P\*V*3* functions:

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
| *P1V3*|  2 | 8 GB| 250 GB|      118|     53|      223 |    134 |
| *P2V3*|  4 |16 GB| 250 GB|      235|    106|      447 |    268 |
| *P3V3*|  8 |32 GB| 250 GB|      471|    212|      894 |    537 |


You can see that Linux workloads definitely less expensive than the Windows. The difference is more remarkable than in the other tiers. If you need a lot of requests to support and you can commit for one or three years, the decision to go to a Linux P*V3 app service seems quite obvious. 
<a id="finalConsiderations"></a>
# Final considerations
Even though Windows tiers perform better than Linux tiers, the latter cost less than proportionally.  
Below you can find a "cost efficency" value, that is the ratio between the number of maximum req/s (provided the 95th percentile respond in less than 100 ms) and the cost per month. The higher cost efficiency, the more bang for the buck.  
For instance, if you need 1000 req/s, you are better off with four P1V3 Linux tiers (which serve 250 req/s each and cost 118x4=472 euros monthly) 
rather than with one P3v3 tier (which serves up to 1250 requests for seconds but cost 894 euros per month)  
Also consider that generally it is better to have more small instances than one bigger instance: if the application restarts because it (say) goes out of memory, the single big instance becomes unavailable for everyone; the several smaller instances remain available for n-1 instances until the broken 1 comes back up again. 
<table>
 <th colspan="5">Tier efficiency</th>
 <tr><td>Tier</td><td><span style="color:darkGreen; font-weight:bold">OS</span></td><td><span style="color:darkOrange; font-weight:bold">maxium req/s for Good Performance</span></td><td>Cost per month</td><td>Cost efficiency</td></tr>
   <tr><td>P2V3</td><td>Linux</td><td>750</td><td>235</td><td>3,19</td></tr>
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
