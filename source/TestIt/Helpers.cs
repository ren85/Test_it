using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using System.Threading;

namespace TestIt
{
    [System.ComponentModel.DesignerCategory("")]
    public class MyWebClient : WebClient
    {
        private int _timeout;
        /// <summary>
        /// Time in seconds
        /// </summary>
        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        public MyWebClient()
        {
            this._timeout = 60;
        }
        /// <summary>
        /// Time in seconds
        /// </summary>
        //public MyWebClient(int timeout)
        //{
        //    this._timeout = timeout;
        //}

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //request.Timeout = this._timeout * 1000;
            request.ServicePoint.ConnectionLimit = Int32.MaxValue;
            return request;
        }

    }


    public class Helpers
    {
        public static byte[] BytesFromDelimitedString(string delimited)
        {
            List<byte> bytes = new List<byte>();
            string[] parts = delimited.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
                bytes.Add(Convert.ToByte(p));
            return bytes.ToArray<byte>();
        }

        public static string ToDelimitedString(string s)
        {
            var res = new StringBuilder();
            var bytes = System.Text.Encoding.UTF8.GetBytes(s);
            foreach (var b in bytes)
                res.Append(b.ToString() + "|");
            return res.ToString();
        }

        public static string BytesToString(byte[] bytes)
        { 
            return Encoding.UTF8.GetString(bytes);
        }

        public static string GetRandomString()
        {
            return new String("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Shuffle(MyRandom.NewRandom()).Take(20).ToArray<char>()).ToString();
        }
    }
    
    public static class Ext
    {
        public static string Shorten(this string s, int len = 80)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            if (s.Length <= len)
                return s;
            return s.Substring(len) + " ...";
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }
    }

    public static class MyRandom
    {
        private static readonly Random globalRandom = new Random();
        private static readonly object globalLock = new object();

        public static Random NewRandom()
        {
            lock (globalLock)
            {
                return new Random(globalRandom.Next());
            }
        }
    } 
}
