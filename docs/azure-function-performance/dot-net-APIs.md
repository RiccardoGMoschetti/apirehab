---
layout: default
title: .NET Core Azure Functions
nav_order: 2
---
# What we tested
This API performance test focused on two technologies:
- Azure Functions (a developer guide is [here](https://learn.microsoft.com/en-us/azure/azure-functions/){:target="_blank" rel="noopener"})
- Microsoft .NET core APIs (start [here](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api){:target="_blank" rel="noopener"})

## What we found
Intuitively, more expensive Azure Functions guaranteed better performance. This proved true not only for price tiers, but also for the OS: Linux functions don't run .NET workloads as efficiently as Windows functions; however, they cost less (see below to see how much).

## How we measured
We ran .NET 7 isolated process APIs on multiple Azure Funcion tiers. 
The APIs do the bare minimum on purpose: our aim was to test the infrastructure, not the code. The API merely 
- retrieves a random string (from a set of 100) from an Azure Redis Cache located in the same virtual network as the Azure Functions
- creates an object in memory that contains that that string, a GUID and another random string
- serializes the object and sends a response with that serialized object
Typical APIs will hardly do less than that; the question is: should they actually do _much more_?
{% gist af07f24520dbe62f1a2abecc4966c4d7 %}

## What can you do with these results?
Even though your software and dependencies can be _very_ different from those we tested here, this exercise can give you an idea of the upper limit you will not be able to exceed even if your software is perfect. We believe it's already something to help you in your decisions.

## The load tool we used
We used <a href="https://github.com/tsenart/vegeta">Vegeta</a>, a simple yet reliable tool which can easily generate a big amount of concurrent calls. We used the version 12.8.3 as the latest did not seem to have been built for ARM64.

## The infrastructure / architecture we tested
We tested all of the Azure Function production-ready tiers available in West Europe (S\*, P\*V2, P\*V3) in both OSs available (Linux and Windows).
The client machine generating the load was a 64 GB / 8 CPU Ubuntu 22.04 VM.
The Azure functions were hosted in the same data center and virtual network of the VMs and of the Redis Cache, via private endpoints. This proximity made sure that we were testing the workloads rather than the infrastructure.
This is an architectural drawing of the solution:  
<img src="https://github.com/RiccardoGMoschetti/apirehab/blob/dd723e412665ea3b43f35d68fc12c2b7089a2063/docs/images/Architecture-API-DotNet-On-Azure-Functions.drawio.png"/>.  
You can download the original diagrams.net (formerly draw.io) drawing <a href="" download="api-rehab.drawio">here</a>.

## What is better, Linux or Windows?
They are very similar, except for the P2 tier, where Windows is definitely better. For the P3 tier, Linux is more comparable.  
Linux is always cheaper (especially in the P3 tier). If your application is stateless, more Linux resources can give you better performance at lower prices and they should be used rather than Windows.

## The data we worked with
For every tier of the available Azure Functions, Vegeta gave us the following information:
- minimum value (which we ignored)
- mean (which we ignored)
- 50th percentile (which we consider more revealing than the mean, as the latter is more influenced by outliers)
- 90th percentile
- 95th percentile
- 99th percentile
- maximum value (which we ignored as well)

For our purposes, a test is _passed_ at a certain rate when, for ten minutes straight, more than 98% of the requests are fullfilled and the 95th percentile for that rate takes less or equal to 999 milliseconds to reply. 999 milliseconds can be considered a lax paramater; take into consideration that this test is supposed to give you the _higher_ limit of the infrastructure, which your users won't hopefully encounter continuously.
In this test, we did not use B tiers of app functions, as they are not reported as production ready by Microsoft.

Upper limits were tested by deploying to the Functions a very basic API (/api/SimpleJson) that does the follow:
- it creates an object in memory with a GUID and a randorm string
- it serializes the object and gives it back to the client.

We believe any API must do _at least_ this (and a lot more). So, this can be considered the absolute upper limit for your needs.

### "S" tiers

These were the results for Linux Functions (prices in April 2023) when we requested the "simpleJson" API:

| *Tier*      | CPUs   | RAM    | Storage| Price/Month |*Max requests/s*  |   
|-------------|--------|--------|--------|-------------|------------------|
| *Linux S1*  |      1 | 1.75 GB|   50 GB|          64 |           **75** |
| *Linux S2*  |      2 | 3.50 GB|   50 GB|         128 |          **150** | 
| *Linux S3*  |      4 | 7.50 GB|   50 GB|         256 |          **300** |

These were the results for Windows Functions:

| *Tier*         | CPUs   | RAM    | Storage| Price/Month |*Max requests/s*|   
|----------------|--------|--------|--------|-------------|----------------|
| *Windows  S1*  |      1 | 1.75 GB|   50 GB|          67 |          *100* |
| *Windows  S2*  |      2 | 3.50 GB|   50 GB|         135 |          *200* | 
| *Windows  S3*  |      4 | 7.50 GB|   50 GB|         270 |          *500* |

From this picture you can gather than Windows workloads, in the S* tier, allow you more requests than Linux, at a slightly increased price. 

### "P*V2" tiers

These tiers are the second generation of "premium" app services.  
Compared to "S" tiers, they have more memory and perform much, much better. They can still be your choice for most workloads where you don't need the P3 tier (which also gives you long-term discount which are not allowed for P2 tiers).

Findings for the Linux P\*V2 functions:

| *Tier*        | CPUs   | RAM     | Storage| Price/Month |*Max requests/s*|   
|---------------|--------|---------|--------|-------------|----------------|
| *Linux P1V2*  |      1 |  3.50 GB|  250 GB|          78 |          *150* |
| *Linux P2V2*  |      2 |  7.50 GB|  250 GB|         156 |          *400* | 
| *Linux P3V2*  |      4 | 14.00 GB|  250 GB|         311 |          *750* |

Findings for the Windows P\*V2 Functions:

| *Tier*          | CPUs   | RAM     | Storage| Price/Month |*Max requests/s*|   
|-----------------|--------|---------|--------|-------------|----------------|
| *Windows P1V2*  |      1 |  3.50 GB|  250 GB|         135 |          *250* |
| *Windows P2V2*  |      2 |  7.50 GB|  250 GB|         270 |          *600* | 
| *Windows P3V2*  |      4 | 14.00 GB|  250 GB|         539 |         *1050* |

You can see here that Windows tiers can get very expensive but also _very_ performant. A Windows P3 function will serve more than 1000 requests per second.

### "P*V3" tiers

These are the best newest in the Azure app service plans.  
They don't necessarily perform better than the P2 counterparts. However, they allow for "reservation" (long term discounts) if you promise to use them for one or three years (the longer the commitment, the stronger the discount). This makes them a very good fit when you know you are going to need app services in the next year (even if not for the same purposes as today: nothing prevents you from buying an app service now and use it with different domain names in the future, taking advantage of the same discount.)

Findings for the Linux P\*V*3* functions:

| *Tier*      |CPUs| RAM |Storage| EUR/Month|3 years EUR/month|*Max requests/s*|   
|-------------|----|-----|-------|----------|-----------------|----------------|
| *Linux P1V3*|  2 | 8 GB| 250 GB|       120|               54|             450|
| *Linux P2V3*|  4 |16 GB| 250 GB|       240|              108|             750|
| *Linux P3V3*|  8 |32 GB| 250 GB|       480|              216|            1050|


Findings for the Windows P\*V*3* Functions:

| *Tier*        |CPUs| RAM |Storage| EUR/Month|3 years EUR/month|*Max requests/s*|   
|---------------|----|-----|-------|----------|-----------------|----------------|
| *Windows P1V3*|  2 | 8 GB| 250 GB|       227|              136|             500|
| *Windows P2V3*|  4 |16 GB| 250 GB|       456|              274|            1000|
| *Windows P3V3*|  8 |32 GB| 250 GB|       911|              547|            1050|


You can see that Linux workloads definitely less expensive than the Windows. The difference is more remarkable than in the other tiers. If you need a lot of requests to support and you can commit for one or three years, the decision to go to a Linux P3 app service seems quite obvious. 

----

[^1]: [It can take up to 10 minutes for changes to your site to publish after you push the changes to GitHub](https://docs.github.com/en/pages/setting-up-a-github-pages-site-with-jekyll/creating-a-github-pages-site-with-jekyll#creating-your-site).

[Just the Docs]: https://just-the-docs.github.io/just-the-docs/
[GitHub Pages]: https://docs.github.com/en/pages
[README]: https://github.com/just-the-docs/just-the-docs-template/blob/main/README.md
[Jekyll]: https://jekyllrb.com
[GitHub Pages / Actions workflow]: https://github.blog/changelog/2022-07-27-github-pages-custom-github-actions-workflows-beta/
[use this template]: https://github.com/just-the-docs/just-the-docs-template/generate
