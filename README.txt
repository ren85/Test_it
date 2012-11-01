This is simple tool for load testing web apps. 
The idea is to issue certain amount of http packets every second and measure response times and successful requests rates.
Use it together with fiddler web debugger.

To use it: 
1) get binaries (.net 4 is required)
2) there are three parameter files:
	-params.txt - contains program parameters. See comments in it.
	-lif.txt - contains rows in the format seconds => requests. Given number of requests will be distributed uniformly over given time interval.
	-requests.txt - here http packets are to be described. See binaries/requests.txt for examples.
3) after parameters are set, just run TestIt.exe (from command line or just by clicking) and collect your statistics
4) This is a tool, but also a weapon. Be good and don't break anything, unless it's yours!

Source code:
1) It's VS 2012 project, .net 4.
2) It is pretty easy to add custom request logic programmatically:
	-add a new class in Tests folder and implement ITest interface (examples are Get.cs and Post.cs)
	-in TestFactory.cs return your object from GetCurrentTest method
	-that's all