using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using iTextSharp.text.pdf;

namespace TestIt.Tests
{
    public class Get : ITest
    {
        Request request { get; set; }
        public Get(Request request)
        {
            this.request = request;
        }
        public void DoWork(int TimeoutInSeconds, string nr)
        {
            DateTime start = DateTime.Now;
            try
            {
                using (MyWebClient client = new MyWebClient(TimeoutInSeconds))
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers[HttpRequestHeader.Accept] = "*/*";
                    client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                    client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; InfoPath.2; .NET4.0E)";
                    
                    foreach (var header in request.Headers)
                        client.Headers[header.Key] = header.Value;
                    foreach (var header in request.CommonHeaders)
                        client.Headers[header.Key] = header.Value;

                    if (request.IsResponsePdf)
                    {
                        var data = client.DownloadData(request.Url);
                        try
                        {
                            PdfReader r = new PdfReader(data);
                        }
                        catch (Exception e)
                        {
                            IsFailure = true;
                            FailureReason = "Bad pdf: " + (e.InnerException != null ? e.InnerException.Message : e.Message);
                            FailureTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        var data = client.DownloadString(request.Url);
                        if (!data.Contains(request.ResponseShouldContain))
                        {
                            IsFailure = true;
                            FailureReason = string.Format("Bad response, should-be-there string not found: ({0})", request.ResponseShouldContain);
                            FailureTime = DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                IsFailure = true;
                FailureReason = "General failure: " + (e.InnerException != null ? e.InnerException.Message : e.Message);
                FailureTime = DateTime.Now;
            }
            finally
            {
                ResponseTimeInMilliseconds = (int)(DateTime.Now - start).TotalMilliseconds;
            }    
        }

        public bool IsFailure
        {
            get;
            set;
        }

        public string FailureReason
        {
            get;
            set;
        }

        public DateTime FailureTime
        {
            get;
            set;
        }

        public int ResponseTimeInMilliseconds
        {
            get;
            set;
        }
    }
}
