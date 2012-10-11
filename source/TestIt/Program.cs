using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TestIt.Tests;

namespace TestIt
{
    class Program
    {
        static void Main(string[] args)
        {
            List<List<string>> loadIntervals = new List<List<string>>();
            int timeoutInSec = 60;
            bool DoStats = false;
            bool ShowAllOutput = true;

            bool setMinMaxThreads = false;
            int maxSimultameousWorkerThreads = 0;
            int minIdleWorkerThreads = 0;
            
            using (TextReader lif = new StreamReader("lif.txt"))
            {
                string line = "";
                while ((line = lif.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line.Trim()) || line.Contains("#"))
                        continue;

                    string[] parts = line.Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> list = new List<string>();
                    list.Add(parts[0]);
                    list.Add(parts[1]);
                    loadIntervals.Add(list);
                }
            }
            using (TextReader reader = new StreamReader("params.txt"))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line.Trim()) || line.Contains("#"))
                        continue;

                    string[] parts = line.Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts[0].Trim() == "maxSimultameousWorkerThreads")
                        maxSimultameousWorkerThreads = Convert.ToInt32(parts[1]);
                    if (parts[0].Trim() == "minIdleWorkerThreads")
                        minIdleWorkerThreads = Convert.ToInt32(parts[1]);
                    if (parts[0].Trim() == "timeoutInSec")
                        timeoutInSec = Convert.ToInt32(parts[1]);
                    if (parts[0].Trim() == "setMinMaxThreads")
                        setMinMaxThreads = Convert.ToBoolean(parts[1]);
                    if (parts[0].Trim() == "doStats")
                        DoStats = Convert.ToBoolean(parts[1]);
                    if (parts[0].Trim() == "showAllOutput")
                        ShowAllOutput = Convert.ToBoolean(parts[1]);
                }
            }

            using (TextReader reader = new StreamReader("requests.txt"))
            {
                List<Request> requests = new List<Request>();
                List<Header> common_headers = new List<Header>();

                Request current = null;
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line.Trim()) || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts[0].Trim().ToLower() == "type")
                    {
                        if (current != null)
                            requests.Add(current);

                        current = new Request();
                        current.Type = parts[1].Trim().ToLower() == "get" ? RequestType.Get : RequestType.Post;
                    }
                    if (parts[0].Trim().ToLower() == "url")
                    {
                        current.Url = parts[1].Trim();
                    }
                    if (parts[0].Trim().ToLower() == "header")
                    {
                        Header header = new Header();
                        header.Key = parts[1].Trim();
                        header.Value = parts[2].Trim();
                        current.Headers.Add(header);
                    }
                    if (parts[0].Trim().ToLower() == "body_bytes")
                    {
                        current.BodyBytes = parts[1].Trim();
                    }
                    if (parts[0].Trim().ToLower() == "weight")
                    {
                        current.Weight = Convert.ToInt32(parts[1]);
                    }
                    if (parts[0].Trim().ToLower() == "response_should_contain")
                    {
                        current.ResponseShouldContain = parts[1].Trim();
                    }
                    if (parts[0].Trim().ToLower() == "response_is_pdf")
                    {
                        current.IsResponsePdf = Convert.ToBoolean(parts[1]);
                    }
                    if (parts[0].Trim().ToLower() == "common_header")
                    {
                        Header header = new Header();
                        header.Key = parts[1].Trim();
                        header.Value = parts[2].Trim();
                        common_headers.Add(header);
                    }
                }
                if (current != null)
                    requests.Add(current);
                requests.ForEach(f => f.CommonHeaders = common_headers);
                
                TestFactory.Requests = requests;
                TestFactory.Indexes_distribution = new List<int>();
                for (int i = 0; i < requests.Count; i++)
                    for (int j = 0; j < requests[i].Weight; j++)
                        TestFactory.Indexes_distribution.Add(i);
                TestFactory.Indexes_count = TestFactory.Indexes_distribution.Count;
            }

            Engine engine = new Engine(loadIntervals: loadIntervals,
                                       timeoutInSec: timeoutInSec,
                                       DoStats: DoStats,
                                       ShowAllOutput: ShowAllOutput,
                                       setMinMaxThreads: setMinMaxThreads,
                                       maxSimultameousWorkerThreads: maxSimultameousWorkerThreads,
                                       minIdleWorkerThreads: minIdleWorkerThreads);
            engine.DoTesting();

            //Console.ReadLine();
        }
    }
}
