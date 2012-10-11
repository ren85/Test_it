This is simple tool for load testing web apps. The idea is to issue certain amount of http packets every second and measure
response times and successful requests rates.
To use it: 
1) get binaries (.net 4 is required)
2) there are three parameter files:
	-params.txt - you'll probably won't have to change it
	-lif.txt - contains rows in the format seconds => requests. Given number of requests will be distributed uniformly over given time interval.
	-requests.txt - here http packets need to be described. See binaries/requests.txt for examples
3) after parameters are set, just run TestIt.exe and collect your statistics

