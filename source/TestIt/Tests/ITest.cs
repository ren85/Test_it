using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestIt.Tests
{
    interface ITest
    {
        void DoWork(int TimeoutInSeconds, string nr);

        bool IsFailure
        {
            get;
        }
        string FailureReason
        {
            get;
        }
        DateTime FailureTime
        {
            get;
        }
        int ResponseTimeInMilliseconds
        {
            get;
        }
    }
}
