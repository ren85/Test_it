using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestIt.Transform
{
    public class RandomString : ITransform
    {
        static string LoadString { get; set; }
        static object string_lock = new object();

        public Tests.Request Transform(Tests.Request request)
        {
            if (string.IsNullOrEmpty(RandomString.LoadString))
            {
                lock (string_lock)
                {
                    if (string.IsNullOrEmpty(RandomString.LoadString))
                    {
                        Console.WriteLine("Building string...");
                        StringBuilder sb = new StringBuilder();
                        var part = Helpers.GetRandomString();
                        for (int i = 0; i < 20000 / part.Length; i++)
                        {
                            sb.Append(part);
                        }
                        RandomString.LoadString = sb.ToString();
                    }
                }
            }
            request.Url = request.Url.Replace("{random_string}", Helpers.GetRandomString());
            if (!string.IsNullOrEmpty(request.BodyBytes))
            {
                if (request.BodyBytes.Contains("{random_string}"))
                {
                    request.BodyBytes = request.BodyBytes.Replace("{random_string}", Helpers.GetRandomString() + RandomString.LoadString);
                }
            }

            return request;
        }
    }
}
