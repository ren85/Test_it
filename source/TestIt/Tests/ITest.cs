using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestIt.Tests
{
    interface ITest
    {
        void DoWorkAsync(int TimeoutInSeconds, string nr, Action callback);

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
        long DownloadedBytes
        {
            get;
        }
    }
}
