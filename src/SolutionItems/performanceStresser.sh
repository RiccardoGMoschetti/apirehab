#!/bin/bash
# usage: bash ./performanceStresser 2>&1 | tee -a "peformanceResults.txt"

# To be able to automatically scale your function with this script, you should create in your Azure Active Directory tenant a service principal that has access to that function.
# The service principal has an app_id, a password (or client_secret), and a tenant_id

app_id="{your app id}"
password="{your app secret}"
tenant_id="{your tenant id}"

resource_group="{your resource group}"
app_plan="{your app plan}"
function="{name of your function, withour the .azurewebsites.net suffix}"
api_to_call="GetFromCache"

output_file_prefix="{output file prefix}"
duration="600s"

ulimit -m 1000000
ulimit -n 1000000

declare plans=("s1" "s2" "s3" "p1v2" "p2v2" "p3v2" "p0v3" "p1v3" "p2v3" "p3v3")
declare -A plan_ranges

declare range_s1=(10 20 30 40 50 60 70 80 90 100)
declare range_s2=(25 50 75 100 125 150 175 200)
declare range_s3=(150 175 200 225 250 275 300 325 350 375 400 425 450 475 500)

declare range_p0v3=(100 125 150 175 200 225 250 275 300 325 350 375)
declare range_p1v3=(150 175 200 225 250 275 300 325 350 375 400 425 450 475 500)
declare range_p2v3=(400 425 450 475 500 525 550 575 600 625 650 675 700 725 750 775 800 825 850 875 900 925 950 975 1000 1025 1050 1075 1100)

declare range_p3v3=(500 525 550 575 600 625 650 675 700 725 750 775 800 825 850 875 900 925 950 975 1000 1025 1050 1075 1100)

declare range_p1v2=(25 50 75 100 125 150 175 200 225 250 275 300 325 350 375)
declare range_p2v2=(150 175 200 225 250 275 300 325 350 375 400 425 450 475 500)
declare range_p3v2=(500 525 550 575 600 625 650 675 700 725 750 775 800 825 850 875 900 925 950 975 1000 1025 1050 1075 1100)


plan_ranges["s1"]=range_s1[@]
plan_ranges["s2"]=range_s2[@]
plan_ranges["s3"]=range_s3[@]

plan_ranges["p0v3"]=range_p0v3[@]
plan_ranges["p1v3"]=range_p1v3[@]
plan_ranges["p2v3"]=range_p2v3[@]
plan_ranges["p3v3"]=range_p3v3[@]

plan_ranges["p1v2"]=range_p1v2[@]
plan_ranges["p2v2"]=range_p2v2[@]
plan_ranges["p3v2"]=range_p3v2[@]

az login --service-principal -u ${app_id} -p ${password} --tenant ${tenant_id}

for plan in "${plans[@]}"  
do
    echo "Time is: $(date)"
    echo "moving to plan: ${plan}"
	az appservice plan update -n ${app_plan} -g ${resource_group} --sku $plan
	echo "sleeping 30s"
    	sleep 30s

	echo "curl-ing"
	echo https://${function}.azurewebsites.net/api/${api_to_call}
	curl https://${function}.azurewebsites.net/api/${api_to_call}
	for rate in  "${!plan_ranges[$plan]}" 
	do
		echo "start: $(date)"
		echo "testing rate $rate with plan $plan"
		output=$(echo "GET https://${function}.azurewebsites.net/api/${api_to_call}" | vegeta attack -duration=${duration}  -rate $rate -timeout 30s -header "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlJpY2NhcmRvIEcgTW9zY2hldHRpIiwiYXVkIjoiQVBJUmVoYWIhIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.G2FVWwYt7-dJ31Gac5sA9GAjht0FtLfw2GedaI6PcE7Pykv9aBxv57yc4JPE1_6RWykaqjIEHZ6wZsaEHWr5j49wUlWLCbCY6aBabP_7uiI_6uR4aul2U9isquJ9ugKKJuWaSwxZZHMDcw-9GHyY7sv8V8lj30V0YY5A9xDW4yv-ShGFwMKGdke43KHqHUaviyMCi3fhGl_bTJHlxp6d4OsZTRKQIXkuDGaW-LfaTbH6iBmM7JHFdwK_r9aTmvUvdEX3kFdXn3Yh-8NjwF7K2eyF_vxjxRjnPwb9qUXIYocnWeE0bz_BY0FZsjmtBTnNjAl4OQfj1TqWslC61tHCng" | tee ${output_file_prefix}_${plan}_rate_${rate}.bin | vegeta report)
		echo "this was the output of the test:"
		echo $output
		
		cat ${output_file_prefix}_${plan}_rate_${rate}.bin  | vegeta plot > ${output_file_prefix}_${plan}_rate_${rate}.html
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
	echo  "moving to plan B1 to reduce costs now"
	az appservice plan update -n ${app_plan} -g ${resource_group} --sku B1
	echo "sleeping 30s"
	sleep 30s
