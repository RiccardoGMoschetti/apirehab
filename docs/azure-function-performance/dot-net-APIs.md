---
layout: default
title: Azure Functions Performance
nav_order: 2
---
# What we tested
This API performance test focused on two technologies:
- Azure Functions (a developer guide is [here](https://learn.microsoft.com/en-us/azure/azure-functions/){:target="_blank" rel="noopener"})
- Microsoft .NET core APIs (start [here](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api){:target="_blank" rel="noopener"})

## What we found
Intuitively, more expensive Azure Functions guaranteed better performance. Linux functions don't run .NET workloads as efficiently as Windows functions; however, they cost less (see below to see how much).  
Go directly [here](#theresults), for the result for the different OSs and tiers.

## How we measured
We ran .NET 7 isolated process APIs on multiple Azure Funcion tiers. 
The APIs do the bare minimum on purpose: our aim was to test the infrastructure, not the code.
The tested API merely 
- retrieves a random string (from a set of 100) from an Azure Redis Cache located in the same virtual network as the Azure Functions
- creates an object in memory that contains that that string, a GUID and another random string
- serializes the object and sends a response with that serialized object
Typical APIs will hardly do less than that; the question is: should they actually do _much more_?
<script src="https://gist.github.com/RiccardoGMoschetti/af07f24520dbe62f1a2abecc4966c4d7.js"></script>

## What can you do with these results?
Even though your software and dependencies can be _very_ different from those we tested here, this exercise can give you an idea of the upper limit you will not be able to exceed even if your software is perfect. We believe it's already something to help you in your decisions.

## The load tool we used
We used <a href="https://github.com/tsenart/vegeta">Vegeta</a>, a simple yet reliable tool which can easily generate a big amount of concurrent calls. We used the version 12.8.3 as the latest did not seem to have been built for ARM64.

## The infrastructure / architecture we tested
We tested all of the Azure Function production-ready tiers available in West Europe (S\*, P\*V2, P\*V3) in both OSs available (Linux and Windows).
The client machine generating the load was a 64 GB / 8 CPU Ubuntu 22.04 VM.
The Azure functions were hosted in the same data center and virtual network of the VMs and of the Redis Cache, via private endpoints. This proximity made sure that we were testing the workloads rather than the infrastructure.
This is an architectural drawing of the solution:  
<img src="https://github.com/RiccardoGMoschetti/apirehab/blob/dd723e412665ea3b43f35d68fc12c2b7089a2063/docs/images/Architecture-API-DotNet-On-Azure-Functions.drawio.png?raw=true"/>.  
You can download the original diagrams.net (formerly draw.io) drawing <a href="" download="api-rehab.drawio">here</a>.

## What is better, Linux or Windows?
They are very similar, except for the P2 tier, where Windows is definitely better. For the P3 tier, Linux is more comparable.  
Linux is always cheaper (especially in the P3 tier). If your application is stateless, more Linux resources can give you better performance at lower prices and they should be used rather than Windows.

## The data we worked with
For every tier of the available Azure Functions, Vegeta gave us the following information: minimum value, mean, 50th percentile (which we consider more revealing than the mean, as the latter is more influenced by outliers, 90th percentile (the data that we considered more important), maxium value.

For our purposes, we focused on the 95th percentile and we stressed the functions for ten minutes straight. 

We then categorized the performance into:

- <span style="color:darkGreen; font-weight:bold">Good</span> performance: at that rate, 95% of API calls take less than 100ms. This means that  only 5% of API calls won't be very fast.
- <span style="color:darkOrange; font-weight:bold">Mediocre</span>: 95% takes less than 1 second. This means that 5% of API calls will be slow.
- <span style="color:darkRed; font-weight:bold">Barely working</span>: the server won't break, but it will take up to 30 seconds per call to do so.

In this test, we did not use B tiers of app functions, as they are not reported as production ready by Microsoft.

<a id="theresults"></a>
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
         <span style="color:darkOrange; font-weight:bold">Mediocre Perfmance</span>
      </td>
      <td>
         <span style="color:darkRed; font-weight:bold">Bad Perfmance</span>
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
Compared to "S" tiers, they have more memory and perform much better. They can still be your choice for most workloads where you don't need the P3 tier (which also gives you long-term discount which are not allowed for P2 tiers).

Findings for the Linux P\*V2 functions:

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
         <span style="color:darkOrange; font-weight:bold">Mediocre Perfmance</span>
      </td>
      <td>
         <span style="color:darkRed; font-weight:bold">Bad Perfmance</span>
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
         400
      </td>
      <td>
         150
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
         <span style="color:darkOrange; font-weight:bold">Mediocre Perfmance</span>
      </td>
      <td>
         <span style="color:darkRed; font-weight:bold">Bad Perfmance</span>
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

|*plan*  |CPUs|RAM GB|Storage|€/month|€/month|
|        |    |      |       | Linux |Windows|
|--------|----|------|-------|-------|-------|
| *P1v2* |  1 | 3.50 |   250 |    76 |   132 |
| *P2v2* |  2 | 7.50 |   250 |   152 |   264 |
| *P3v2* |  4 |14.00 |   250 |   305 |   529 |
 

You can see here that Windows tiers can get very expensive but also _very_ performant. A Windows P3 function will serve more than 1000 requests per second.

### "P*V3" tiers

These are the best newest in the Azure app service plans.  
They don't necessarily perform better than the P2 counterparts. However, they allow for "reservation" (long term discounts) if you promise to use them for one or three years (the longer the commitment, the stronger the discount). This makes them a very good fit when you know you are going to need app services in the next year (even if not for the same purposes as today: nothing prevents you from buying an app service now and use it with different domain names in the future, while still taking advantage of the same discount.)

Findings for the Linux P\*V*3* functions:

| *Tier*      |CPUs| RAM |Storage|EUR/Month|3 years EUR/month|EUR/Month|
|             |    |     |       | Linux   |     Windows     |         |
|-------------|----|-----|-------|---------|-----------------|---------|
| *Linux P1V3*|  2 | 8 GB| 250 GB|      118|               53|      450|
| *Linux P2V3*|  4 |16 GB| 250 GB|      235|              106|      750|
| *Linux P3V3*|  8 |32 GB| 250 GB|      471|              212|     1050|


Findings for the Windows P\*V*3* Functions:

| *Tier*        |CPUs| RAM |Storage| EUR/Month|3 years EUR/month|*Max requests/s*|   
|---------------|----|-----|-------|----------|-----------------|----------------|
| *Windows P1V3*|  2 | 8 GB| 250 GB|       223|              134|             500|
| *Windows P2V3*|  4 |16 GB| 250 GB|       447|              268|            1000|
| *Windows P3V3*|  8 |32 GB| 250 GB|       894|              537|            1050|


You can see that Linux workloads definitely less expensive than the Windows. The difference is more remarkable than in the other tiers. If you need a lot of requests to support and you can commit for one or three years, the decision to go to a Linux P*V3 app service seems quite obvious. 

----

[^1]: [It can take up to 10 minutes for changes to your site to publish after you push the changes to GitHub](https://docs.github.com/en/pages/setting-up-a-github-pages-site-with-jekyll/creating-a-github-pages-site-with-jekyll#creating-your-site).

[Just the Docs]: https://just-the-docs.github.io/just-the-docs/
[GitHub Pages]: https://docs.github.com/en/pages
[README]: https://github.com/just-the-docs/just-the-docs-template/blob/main/README.md
[Jekyll]: https://jekyllrb.com
[GitHub Pages / Actions workflow]: https://github.blog/changelog/2022-07-27-github-pages-custom-github-actions-workflows-beta/
[use this template]: https://github.com/just-the-docs/just-the-docs-template/generate
