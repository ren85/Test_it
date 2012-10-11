using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using TestIt.Tests;

namespace TestIt
{
    class Engine
    {   
        //PARAMS  
        List<List<string>> loadIntervals;
        int timeoutInSec = 0;
        bool DoStats = false;
        bool ShowAllOutput = true;
        bool setMinMaxThreads = false;
        int maxSimultameousWorkerThreads = 0;
        int minIdleWorkerThreads = 0;
        //PARAMS

        public Engine(List<List<string>> loadIntervals, int timeoutInSec, bool DoStats, bool ShowAllOutput, bool setMinMaxThreads, int maxSimultameousWorkerThreads, int minIdleWorkerThreads)
        {
            this.loadIntervals = loadIntervals;
            this.timeoutInSec = timeoutInSec;
            this.DoStats = DoStats;
            this.ShowAllOutput = ShowAllOutput;
            this.setMinMaxThreads = setMinMaxThreads;
            this.maxSimultameousWorkerThreads = maxSimultameousWorkerThreads;
            this.minIdleWorkerThreads = minIdleWorkerThreads;
        }

        int totalJobs = 0;
        int loadIntervalInSecs = 0;
        static object _lock = new object();
        List<ThreadJob> jobs = new List<ThreadJob>();
        ManualResetEvent[] doneEvents = null;
        Timer timer = null;
        int[] loadMap = null;
        int currentSecond = 0;
        int currentJob = 0;
        bool thatsIt = false;
        bool thatsReallyIt = false;
        DateTime start;
        Random rg;

        public void DoTesting()
        {
            if (setMinMaxThreads)
            {
                int defMinWorkers, defMaxWorkers, defMinIO, defMaxIO;
                ThreadPool.GetMinThreads(out defMinWorkers, out defMinIO);
                ThreadPool.GetMaxThreads(out defMaxWorkers, out defMaxIO);

                if (!ThreadPool.SetMaxThreads(maxSimultameousWorkerThreads, maxSimultameousWorkerThreads/*defMaxIO*/))
                    throw new Exception(string.Format("Couldn't set max {0} working threads.", maxSimultameousWorkerThreads));
                if (!ThreadPool.SetMinThreads(minIdleWorkerThreads, minIdleWorkerThreads/*defMinIO*/))
                    throw new Exception(string.Format("Couldn't set min {0} working threads.", minIdleWorkerThreads));
            }

            timer = new Timer(TimeToStartSomeWorkers, null, Timeout.Infinite, Timeout.Infinite);
            
            foreach (var list in loadIntervals)
            {                
                loadIntervalInSecs = Convert.ToInt32(list[0]);
                totalJobs = Convert.ToInt32(list[1]);

                Console.WriteLine(string.Format("{0}: Starting interval: {1} jobs during {2} seconds", DateTime.Now, totalJobs, loadIntervalInSecs));
                DoIntervalWork();

                System.Threading.Thread.Sleep(60000);

                if (DoStats)
                {

                }                
            }
        }

        private void DoIntervalWork()
        {
            jobs = new List<ThreadJob>();
            doneEvents = null;
            loadMap = null;
            currentSecond = 0;
            currentJob = 0;
            thatsIt = false;
            thatsReallyIt = false;

            doneEvents = new ManualResetEvent[totalJobs];
            
            loadMap = new int[loadIntervalInSecs];
            rg = new Random();
            for (int i = 0; i < totalJobs; i++) //here we uniformly distribute totalJobs over whole interval
                loadMap[rg.Next(0, loadIntervalInSecs)]++;

            start = DateTime.Now;
            timer.Change(0, 1000);
            while (!thatsReallyIt)
                System.Threading.Thread.Sleep(10000);
        }


        private void TimeToStartSomeWorkers(object info)
        {
            lock (_lock) //nothing blocks inside this except the last second (or so we hope)
            {
                if (thatsIt)    //timer may start a little more threads than necessary
                    return;
                System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Highest; //we obtained lock, so we have to be cpu-priviliged

                if (currentSecond == loadIntervalInSecs) //last second
                {
                    thatsIt = true;
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    Console.WriteLine("{0} should be equal (more or less) to {1}.", (DateTime.Now - start).TotalSeconds, loadIntervalInSecs);

                    for (int i = 0; i < jobs.Count; i++)
                        WaitHandle.WaitAll(new ManualResetEvent[1] { doneEvents[i] });

                    Console.WriteLine("{0}: all workers are done.", DateTime.Now);

                    //calc and output stats                    
                    int failures = 0;
                    double totalMillisecs = 0;

                    Dictionary<string, int> failureReasons = new Dictionary<string, int>();
                    List<KeyValuePair<DateTime, string>> errorTimes = new List<KeyValuePair<DateTime, string>>();
                    foreach (ThreadJob job in jobs)
                    {
                        if (!job.TerminatedSuccesfully)
                        {
                            failures++;
                            if (!failureReasons.Keys.Contains(job.FailureReason))
                                failureReasons.Add(job.FailureReason, 1);
                            else
                                failureReasons[job.FailureReason]++;
                            errorTimes.Add(new KeyValuePair<DateTime, string>(job.FailureTime, job.FailureReason));
                        }
                        else
                        {
                            totalMillisecs += job.ResponseTimeInMilliseconds;
                        }
                    }
                    Console.WriteLine("Total workers: {0}, failed {1}, successful worker's avg time (sec) {2}", jobs.Count, failures,
                        (jobs.Count - failures) != 0 ? (double)((totalMillisecs / (double)(jobs.Count - failures)) / (double)1000) : -1);

                    Dictionary<string, int> timeDistribution = new Dictionary<string, int>();
                    timeDistribution.Add("[0; 10] sec", 0);
                    timeDistribution.Add("[10; 30] sec", 0);
                    timeDistribution.Add("[30; 60] sec", 0);
                    timeDistribution.Add("[60; ...] sec", 0);
                    foreach (ThreadJob job in jobs)
                    {
                        if (job.TerminatedSuccesfully)
                        {
                            int time = job.ResponseTimeInMilliseconds/1000;
                            if (time <= 10)
                                timeDistribution["[0; 10] sec"]++;
                            if (time > 10 && time <= 30)
                                timeDistribution["[10; 30] sec"]++;
                            if (time > 30 && time <= 60)
                                timeDistribution["[30; 60] sec"]++;
                            if (time > 60)
                                timeDistribution["[60; ...] sec"]++;
                        }
                    }

                    Console.WriteLine("Time distribution:");
                    foreach (string key in timeDistribution.Keys)
                        Console.WriteLine(key + ": " + timeDistribution[key] + " request(s)");

                    if (failures != 0)
                    {
                        Console.WriteLine("\nBelow are common failure reasons:");
                        foreach (string failure in failureReasons.Keys)
                            Console.WriteLine(string.Format("'{0}' happened {1} times", failure, failureReasons[failure]));
                        Console.WriteLine("Error map:");
                        foreach (var failureTime in errorTimes)
                            Console.WriteLine(failureTime.Key + ": " + failureTime.Value);
                    }
                    thatsReallyIt = true; //main program can exit
                }
                else
                {
                    if(ShowAllOutput)
                        Console.WriteLine("{0}: second {1}, jobs that second {2}", DateTime.Now, currentSecond, loadMap[currentSecond]);
                    for (int i = 0; i < loadMap[currentSecond]; i++) //how much work happened this second
                    {
                        doneEvents[currentJob] = new ManualResetEvent(false);
                        ThreadJob job = new ThreadJob(doneEvents[currentJob], timeoutInSec, string.Format("{0}.{1}", currentSecond, i), currentJob);
                        jobs.Add(job);
                        ThreadPool.QueueUserWorkItem(job.ThreadProc);
                        currentJob++;
                    }
                    currentSecond++;
                }
            }
        }
    }

    class ThreadJob
    {
        public bool TerminatedSuccesfully { get; set; }
        public int ResponseTimeInMilliseconds { get; set; }
        public string FailureReason { get; set; }
        public DateTime FailureTime { get; set; }

        ManualResetEvent _doneEvent;
        int timeoutInSec;
        string nr;
        int id;

        public ThreadJob(ManualResetEvent doneEvent, int timeoutInSec, string nr, int id)
        {
            this._doneEvent = doneEvent;
            this.timeoutInSec = timeoutInSec;
            this.nr = nr;
            this.id = id;
        }
        public void ThreadProc(object state)
        {
            ITest test = TestFactory.GetCurrentTest(id);
            test.DoWork(timeoutInSec, nr);
            TerminatedSuccesfully = !test.IsFailure;
            ResponseTimeInMilliseconds = test.ResponseTimeInMilliseconds;
            if (test.IsFailure)
            {
                FailureReason = test.FailureReason;
                FailureTime = test.FailureTime;
            }

            _doneEvent.Set();

            // Yield the rest of the time slice.
            System.Threading.Thread.Yield();
        }
    }

}
