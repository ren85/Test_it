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
        Timer timer = null;
        int[] loadMap = null;
        int currentSecond = 0;
        int currentJob = 0;
        bool thatsIt = false;
        bool thatsReallyIt = false;
        DateTime start;
        Random rg;

        public static object counter_lock = new object();
        public static int counter;
        public static bool Done;
        public TextWriter Log;

        public void DoTesting()
        {
            using (Log = new StreamWriter("log.txt"))
            {
                if (setMinMaxThreads)
                {

                    if (!ThreadPool.SetMaxThreads(maxSimultameousWorkerThreads, maxSimultameousWorkerThreads))
                        throw new Exception(string.Format("Couldn't set max {0} working threads.", maxSimultameousWorkerThreads));
                    if (!ThreadPool.SetMinThreads(minIdleWorkerThreads, minIdleWorkerThreads))
                        throw new Exception(string.Format("Couldn't set min {0} working threads.", minIdleWorkerThreads));
                }

                timer = new Timer(TimeToStartSomeWorkers, null, Timeout.Infinite, Timeout.Infinite);

                foreach (var list in loadIntervals)
                {
                    loadIntervalInSecs = Convert.ToInt32(list[0]);
                    totalJobs = Convert.ToInt32(list[1]);
                    counter = 0;

                    WriteInfo(string.Format("{0}: Starting interval: {1} jobs during {2} seconds", DateTime.Now, totalJobs, loadIntervalInSecs));

                    DoIntervalWork();

                    if (DoStats)
                    {

                    }
                }
            }
            Console.WriteLine("Done. Output is in log.txt");
            Done = true;
        }

        private void DoIntervalWork()
        {
            jobs = new List<ThreadJob>();
            loadMap = null;
            currentSecond = 0;
            currentJob = 0;
            thatsIt = false;
            thatsReallyIt = false;
            
            loadMap = new int[loadIntervalInSecs];
            rg = new Random();
            for (int i = 0; i < totalJobs; i++) //here we uniformly distribute totalJobs over whole interval
                loadMap[rg.Next(0, loadIntervalInSecs)]++;

            start = DateTime.Now;
            timer.Change(0, 1000);
            while (!thatsReallyIt)
            {
                System.Threading.Thread.Sleep(10000);
            }
        }

        void WriteInfo(string info)
        {
            Console.WriteLine(info);
            Log.WriteLine(info);
        }
        private void TimeToStartSomeWorkers(object info)
        {
            if (currentSecond == loadIntervalInSecs) 
                timer.Change(Timeout.Infinite, Timeout.Infinite);
            lock (_lock) //nothing blocks inside this except the last second (or so we hope)
            {
                if (thatsIt)    //timer may start a little more threads than necessary
                    return;
                System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Highest; //we obtained lock, so we have to be cpu-priviliged

                if (currentSecond == loadIntervalInSecs) //last second
                {
                    thatsIt = true;
                    WriteInfo(string.Format("{0} should be equal (more or less) to {1}. If it isn't it means your computer wasn't always able to fire given amount of threads per second.", (DateTime.Now - start).TotalSeconds, loadIntervalInSecs));

                    if (jobs.Count(f => f.Completed) != jobs.Count)
                    {
                        WriteInfo("Waiting...");
                        int waited = 0;
                        while (waited < timeoutInSec+5)
                        {
                            System.Threading.Thread.Sleep(1000);
                            waited++;
                            if (jobs.Count(f => f.Completed) == jobs.Count)
                                break;
                        }
                    }

                    WriteInfo(string.Format("{0}: Total threads created: {1}, actually completed: {2} ({3}%)", DateTime.Now, jobs.Count, jobs.Count(f => f.Completed), Math.Round(jobs.Count(f => f.Completed)*100 / (double)jobs.Count)));

                    //calc and output stats                    
                    int failures = 0;
                    double totalMillisecs = 0;

                    Dictionary<string, int> failureReasons = new Dictionary<string, int>();
                    List<KeyValuePair<DateTime, string>> errorTimes = new List<KeyValuePair<DateTime, string>>();
                    foreach (ThreadJob job in jobs.Where(f => f.Completed))
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
                    WriteInfo(string.Format("Total workers: {0}, failed {1} ({2}%), successful worker's avg time (sec) {3}, downloaded (after decompression): {4} Mb", 
                                            jobs.Count(f => f.Completed), 
                                            failures, 
                                            jobs.Count(f => f.Completed) != 0 ? Math.Round(failures*100 / (double)jobs.Count(f => f.Completed)) : 0,
                                            (jobs.Count(f => f.Completed) - failures) != 0 ? (double)((totalMillisecs / (double)(jobs.Count(f => f.Completed) - failures)) / (double)1000) : -1,
                                            Math.Round(jobs.Where(f => f.Completed).Sum(f => f.DownloadedBytes) / (double)1048576, 2)));

                    Dictionary<string, int> timeDistribution = new Dictionary<string, int>();
                    timeDistribution.Add("[0; 10] sec", 0);
                    timeDistribution.Add("[10; 30] sec", 0);
                    timeDistribution.Add("[30; 60] sec", 0);
                    timeDistribution.Add("[60; ...] sec", 0);
                    foreach (ThreadJob job in jobs.Where(f => f.Completed))
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

                    WriteInfo("Time distribution:");
                    foreach (string key in timeDistribution.Keys)
                        WriteInfo(key + ": " + timeDistribution[key] + " request(s)");

                    if (failures != 0)
                    {
                        WriteInfo("\nBelow are common failure reasons:");
                        foreach (string failure in failureReasons.Keys.OrderByDescending(f => failureReasons[f]))
                            WriteInfo(string.Format("'{0}' happened {1} times", failure, failureReasons[failure]));
                        WriteInfo("Error map:");
                        foreach (var failureTime in errorTimes)
                            WriteInfo(failureTime.Key + ": " + failureTime.Value);
                    }
                    thatsReallyIt = true; //main program can exit
                }
                else
                {
                    if(ShowAllOutput)
                        WriteInfo(string.Format("{0}: second {1}, jobs that second {2}, active requests: {3}", DateTime.Now, currentSecond, loadMap[currentSecond], Engine.counter));
                    for (int i = 0; i < loadMap[currentSecond]; i++) //how much work happened this second
                    {
                        ThreadJob job = new ThreadJob(timeoutInSec, string.Format("{0}.{1}", currentSecond, i), currentJob);
                        jobs.Add(job);
                        ThreadPool.QueueUserWorkItem(job.ThreadProc);
                        currentJob++;
                    }
                    currentSecond++;
                }
            }
            System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Normal;
        }
    }

    class ThreadJob
    {
        public bool TerminatedSuccesfully { get; set; }
        public int ResponseTimeInMilliseconds { get; set; }
        public string FailureReason { get; set; }
        public DateTime FailureTime { get; set; }
        public bool Completed { get; set; }
        public long DownloadedBytes { get; set; }

        int timeoutInSec;
        string nr;
        int id;

        public ThreadJob(int timeoutInSec, string nr, int id)
        {
            this.timeoutInSec = timeoutInSec;
            this.nr = nr;
            this.id = id;
        }
        public void ThreadProc(object state)
        {
            lock (Engine.counter_lock)
            {
                Engine.counter++;
            }

            ITest test = TestFactory.GetCurrentTest(id);
            test.DoWorkAsync(timeoutInSec, nr, () => {

                Completed = true;

                TerminatedSuccesfully = !test.IsFailure;
                ResponseTimeInMilliseconds = test.ResponseTimeInMilliseconds;
                DownloadedBytes = test.DownloadedBytes;

                if (test.IsFailure)
                {
                    FailureReason = test.FailureReason;
                    FailureTime = test.FailureTime;
                }

                lock (Engine.counter_lock)
                {
                    Engine.counter--;
                }
            });
           

            // Yield the rest of the time slice.
            System.Threading.Thread.Yield();
        }
    }

}
