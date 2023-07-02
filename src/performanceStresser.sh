#!/bin/bash
# usage: bash ./performanceStresser 2>&1 | tee -a "peformanceResults.txt"

# To be able to automatically scale your function withthis script, you should create in your Azure Active Directory tenant a service principal that has access to that function.
# The service principal has an app_id, a client_secret, and a tenant_id

app_id="{your app id}"
client_secret="{your app secret}"
tenant_id="{your tenant id}"

resource_group="{your resource group}"
app_plan="{your app plan}"
function="{name of your function}"
api_to_call="GetFromCache"

output_file_prefix="{output file prefix}"
duration="600s"

ulimit -m 1000000
ulimit -n 1000000

for plan in S1 S2 S3 P1V2 P2V2 P3V2 P1V3 P2V3 P3V3 B1
do
    echo "Time is: $(date)"
	echo "logging in and restarting function"
	az login --service-principal -u ${app_id} -p ${password} --tenant ${tenant_id}
    az functionapp restart -n ${function} -g ${resource_group}
    echo "sleeping 30s"
    sleep 30s

    echo  "moving to plan: ${plan}"
	az appservice plan update -n ${app_plan} -g ${resource_group} --sku $plan
	echo "sleeping 30s"
    sleep 30s

    echo "restarting function again"
    az functionapp restart -n ${function} -g ${resource_group}
	echo "sleeping 30s"
    sleep 30s

	echo "curl-ing"
	curl https://${function}.azurewebsites.net/api/${api_to_call}
	echo "sleeping 30s"
	sleep 30s

	for i in 10 20 30 40 50 75 100 125 150 175 200 250 300 350 400 450 500 550 600 650 700 750 800 850 900 950 1000 1050 1100 1150 1200 1250 1300 1350 1400
	do
		echo "start: $(date)"
		echo "testing rate $i with plan $plan"
		output=$(echo "GET https://${function}.azurewebsites.net/api/SimpleJson" | vegeta attack -duration=${duration}  -rate $i -timeout 30s | tee ${output_file_prefix}_${plan}_rate_${i}.bin | vegeta report)
		echo "this was the output of the test:"
		echo $output
		
		cat ${output_file_prefix}_${plan}_rate_${i}.bin  | vegeta plot > ${output_file_prefix}_${plan}_rate_${i}.html
		success_ratio=$(echo $output | grep -oP 'Success \[ratio\] \K\d+\.\d+')
		echo "success ratio: $success_ratio"
		
		if (( $(echo "$success_ratio < 98" | bc -l) )); then
			echo "success ratio is lower than 98%. Exiting the loop for this plan"
			break
		else 
			echo "sleeping 60s and going on"
			sleep 60s
		fi

		echo "finish: $(date)"
		echo "sleeping 30s"
		sleep 30s
	done
done
