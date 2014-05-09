using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestIt.Tests;

namespace TestIt.Transform
{
    public class NextNumber : ITransform
    {
        static long Number { get; set; }
        static object NumberLock = new object();

        public Request Transform(Request request)
        {
            if (request.Url.Contains("{next_number}"))
            {
                long nr;
                lock (NumberLock)
                {
                    nr = Number;
                    Number++;
                }
                request.Url = request.Url.Replace("{next_number}", nr.ToString());
            }
            if (!string.IsNullOrEmpty(request.BodyBytes))
            {
                if (request.BodyBytes.Contains("{next_number}"))
                {
                    long nr;
                    lock (NumberLock)
                    {
                        nr = Number;
                        Number++;
                    }
                    request.BodyBytes = request.BodyBytes.Replace("{next_number}", nr.ToString());
                }
            }

            return request;
        }
    }
}
