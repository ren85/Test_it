using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace TestIt.Tests
{
    public class Get : ITest
    {
        Request request { get; set; }
        public Get(Request request)
        {
            this.request = request;
        }
        public void DoWorkAsync(int TimeoutInSeconds, string nr, Action callback)
        {
            try
            {
                DateTime start = DateTime.Now;
                using (MyWebClient client = new MyWebClient(TimeoutInSeconds))
                {
                    foreach (var header in request.Headers)
                        client.Headers[header.Key] = header.Value;
                    foreach (var header in request.CommonHeaders)
                        client.Headers[header.Key] = header.Value;

                    if (request.IsResponseBinary)
                    {
                        client.DownloadDataCompleted += (s, e) =>
                            {
                                try
                                {
                                    ResponseTimeInMilliseconds = (int)(DateTime.Now - start).TotalMilliseconds;

                                    if (e.Error != null)
                                    {
                                        IsFailure = true;
                                        FailureReason = "General failure: " + (e.Error.InnerException != null ? e.Error.InnerException.Message : e.Error.Message);
                                        FailureTime = DateTime.Now;
                                    }
                                    else if (e.Result.Count() != request.BinaryResponseSizeInBytesShouldBe)
                                    {
                                        IsFailure = true;
                                        FailureReason = string.Format("Bad response, binary response size was {0} bytes, expected {1} bytes", e.Result.Count(), request.BinaryResponseSizeInBytesShouldBe);
                                        FailureTime = DateTime.Now;
                                        if (e.Result != null)
                                            DownloadedBytes = e.Result.Count();
                                    }
                                    else
                                    {
                                        if (e.Result != null)
                                            DownloadedBytes = e.Result.Count();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    IsFailure = true;
                                    FailureReason = "General failure: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                                    FailureTime = DateTime.Now;
                                }
                                finally
                                {
                                    callback();
                                }
                            };
                        client.DownloadDataAsync(new Uri(request.Url));
                    }
                    else
                    {
                        client.Encoding = Encoding.UTF8;
                        client.DownloadStringCompleted += (s, e) =>
                            {
                                try
                                {
                                    ResponseTimeInMilliseconds = (int)(DateTime.Now - start).TotalMilliseconds;
                                    if (e.Result != null)
                                        DownloadedBytes = System.Text.Encoding.UTF8.GetBytes(e.Result).Count(); 

                                    if (e.Error != null)
                                    {
                                        IsFailure = true;
                                        FailureReason = "General failure: " + (e.Error.InnerException != null ? e.Error.InnerException.Message : e.Error.Message);
                                        FailureTime = DateTime.Now;
                                    }
                                    else if (!e.Result.Contains(request.ResponseShouldContain))
                                    {
                                        IsFailure = true;
                                        FailureReason = string.Format("Bad response, should-be-there string not found: ({0})", request.ResponseShouldContain);
                                        FailureTime = DateTime.Now;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    IsFailure = true;
                                    FailureReason = "General failure: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                                    FailureTime = DateTime.Now;
                                }
                                finally
                                {
                                    callback();
                                }
                            };
                        client.DownloadStringAsync(new Uri(request.Url));
                    }
                }
            }
            catch (Exception e)
            {
                IsFailure = true;
                FailureReason = "General failure: " + (e.InnerException != null ? e.InnerException.Message : e.Message);
                FailureTime = DateTime.Now;
                callback();
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

        public long DownloadedBytes
        {
            get;
            set;
        }
    }
}
