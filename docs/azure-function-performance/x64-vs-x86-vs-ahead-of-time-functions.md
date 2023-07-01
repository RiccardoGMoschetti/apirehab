---
layout: default
title: .NET workloads - more variations
nav_order: 1
parent: Azure Functions
comments: true
---
# Performance of Azure Functions with .NET workloads: the case of 64 bits vs 32 bits, ahead-of-time compilation, and self-contained vs framework-dependant binaries

Building on the findings regarding the comparison of Linux and Windows Azure Functions, we extended our test to answer these common questions:
- is performance better if I use x86 (32 bits) or x64 (64 bits) frameworks?
- is framework-dependant compilation better than portable 
- is Azure's ahead-of-time-flavor compilation (named _ready to run_; [here](https://github.com/Azure/azure-functions-host/issues/5876) is a description) a good choice?

For these tests, we used the same methodology as with the [main tests](./azure-functions.md) which can be summarized as such:
- we deployed simple APIs that retrieve random data from a Redis Cache
- we tested the performance a vegeta-equipped VM in the same virtual network as the rest of the infrastructure

For this particular test, we focused on the P2v3 tier, which seems to be the best option for wo

# Results
## Windows

As you can see from the tables below, these findings are evident:

- the ahead of time compilation option (ready to run) does not improve performance (however, this does not account for boot time, which we didn't measure as we give for granted it must be better)
- 32 bits and 64 bits are very comparable; possibly, x65
- self-contained and framework-dependant compilations are indifferent for performance

<table>
   <th colspan="4"><font size="+1">Maximum requests per second</font></th>
   <tr><td>Tier</td><td><span style="color:darkGreen; font-weight:bold">Good Performance</span></td><td><span style="color:darkOrange; font-weight:bold">Mediocre Performance</span></td><td><span style="color:darkRed; font-weight:bold">Any Performance</span></td></tr>
   <tr><td>&nbsp;</td><td>95th perc. < 100ms</td><td>95th perc. < 1000ms</td><td>(any 95th perc)</td></tr>
   <tr><td>Windows P2v3 <em>Ready to Run</em></td><td>800  </td><td>875</td><td>950</td></tr>
   <tr><td>Windows P2v3 <em>64 bits framework-dependant</em></td><td>825</td><td>1000</td><td>1000</td></tr>
   <tr><td>Windows P2v3 <em>64 bits self-contained</em></td><td>800</td><td>1050</td><td>1050</td></tr>
      <tr><td>Windows P2v3 <em>32 bits framework-dependant</em></td><td>800</td><td>1050</td><td>1050</td></tr>
   <tr><td>Windows P2v3 <em>32 bits self-contained</em></td><td>800</td><td>975</td><td>975</td></tr>
</table>



{% if page.comments %}
<div id="disqus_thread"></div>
<script>
    var disqus_config = function () {
    this.page.url = 'https://www.api.rehab/docs/azure-function-performance/dot-net-APIs.html';
    this.page.identifier = 'dotNetOnAzureFunctions';
    };
    (function() { 
    var d = document, s = d.createElement('script');
    s.src = 'https://www-api-rehab.disqus.com/embed.js';
    s.setAttribute('data-timestamp', +new Date());
    (d.head || d.body).appendChild(s);
    })();
</script>
<noscript>Please enable JavaScript to view the <a href="https://disqus.com/?ref_noscript">comments powered by Disqus.</a></noscript>
{% endif %}