using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace TestIt.Tests
{
    public class Post : ITest
    {
        Request request { get; set; }
        public Post(Request request)
        {
            this.request = request;
        }
        public void DoWorkAsync(string nr, Action callback)
        {
            try
            {
                DateTime start = DateTime.Now;
                using (MyWebClient client = new MyWebClient())
                {
                    client.Encoding = Encoding.UTF8;

                    foreach (var header in request.Headers)
                        client.Headers[header.Key] = header.Value;
                    foreach (var header in request.CommonHeaders)
                        client.Headers[header.Key] = header.Value;

                    client.UploadDataCompleted += (s, e) =>
                        {
                            ResponseTimeInMilliseconds = (int)(DateTime.Now - start).TotalMilliseconds;
                            try
                            {
                                if (e.Error != null)
                                {
                                    IsFailure = true;
                                    FailureReason = "General failure: " + (e.Error.InnerException != null ? e.Error.InnerException.Message : e.Error.Message);
                                    FailureTime = DateTime.Now;
                                }
                                else
                                {
                                    if (e.Result != null)
                                        DownloadedBytes = e.Result.Count(); 
                                    if (request.IsResponseBinary)
                                    {
                                        if (e.Result.Count() != request.BinaryResponseSizeInBytesShouldBe)
                                        {
                                            IsFailure = true;
                                            FailureReason = string.Format("Bad response, binary response size was {0} bytes, expected {1} bytes", e.Result.Count(), request.BinaryResponseSizeInBytesShouldBe);
                                            FailureTime = DateTime.Now;
                                        }
                                    }
                                    else
                                    {
                                        var res = Helpers.BytesToString(e.Result);
                                        if (!res.Contains(request.ResponseShouldContain))
                                        {
                                            IsFailure = true;
                                            FailureReason = string.Format("Bad response, should-be-there string not found: ({0})", request.ResponseShouldContain);
                                            FailureTime = DateTime.Now;
                                        }
                                    }
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
                    var uri = new Uri(request.Url);
                    var servicePoint = ServicePointManager.FindServicePoint(uri);
                    servicePoint.Expect100Continue = false;
                    client.UploadDataAsync(uri, Helpers.BytesFromDelimitedString(request.BodyBytes));
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
