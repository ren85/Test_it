using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestIt.Tests
{
    public class Request
    {
        public RequestType Type { get; set; }
        public string Url { get; set; }
        public List<Header> Headers = new List<Header>();
        public string BodyBytes { get; set; }
        public int Weight { get; set; }
        public string ResponseShouldContain { get; set; }
        public bool IsResponseBinary { get; set; }
        public int BinaryResponseSizeInBytesShouldBe { get; set; }
        public List<Header> CommonHeaders = new List<Header>();

        public Request()
        {
            Weight = 1;
            Type = RequestType.Get;
        }
    
        public static Request CopyRequest(Request req)
        {
            Request request = new Request()
            {
                BinaryResponseSizeInBytesShouldBe = req.BinaryResponseSizeInBytesShouldBe,
                BodyBytes = req.BodyBytes,
                CommonHeaders = Request.CopyHeaders(req.CommonHeaders),
                Headers = Request.CopyHeaders(req.Headers),
                IsResponseBinary = req.IsResponseBinary,
                ResponseShouldContain = req.ResponseShouldContain,
                Type = req.Type,
                Url = req.Url,
                Weight = req.Weight
            };
            return request;
        }
        public static List<Header> CopyHeaders(List<Header> source)
        {
            var res = new List<Header>();
            for (int i = 0; i < source.Count; i++)
            {
                res.Add(new Header()
                    {
                        Key = source[i].Key,
                        Value = source[i].Value
                    });
            }
            return res;
        }
    }
 
    public class Header
    {
        public string Key {get; set;}
        public string Value {get; set;}
    }

    public enum RequestType
    { 
        Get,
        Post
    }
}
