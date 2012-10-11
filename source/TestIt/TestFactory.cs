using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestIt.Tests;

namespace TestIt
{
    class TestFactory
    {
        public static List<Request> Requests { get; set; } //This is going to be accessed from many threads simultaneously.
                                                           //Without locking the only safe thing to do is read, like Requests[i].
                                                           //Enumerating is not safe.

        public static int Indexes_count { get; set; }
        public static List<int> Indexes_distribution { get; set; }

        public static ITest GetCurrentTest(int id)
        {
            var random = new Random(id);
            int index = Indexes_distribution[random.Next(0, Indexes_count)];
            Request request = Requests[index];
            if (request.Type == RequestType.Get)
                return new Get(request);
            else
                return new Post(request);
        }
    }
}
