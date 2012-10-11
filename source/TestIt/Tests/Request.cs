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
        public bool IsResponsePdf { get; set; }
        public List<Header> CommonHeaders = new List<Header>();

        public Request()
        {
            Weight = 1;
            Type = RequestType.Get;
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
