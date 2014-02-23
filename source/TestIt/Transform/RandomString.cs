using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestIt.Transform
{
    public class RandomString : ITransform
    {
        static string LoadString { get; set; }

        public Tests.Request Transform(Tests.Request request)
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
            request.Url = request.Url.Replace("{random_string}", Helpers.GetRandomString());
            if (!string.IsNullOrEmpty(request.BodyBytes))
            {
                var body = Helpers.BytesToString(Helpers.BytesFromDelimitedString(request.BodyBytes));
                if (body.Contains("{random_string}"))
                {
                    body = body.Replace("{random_string}", Helpers.GetRandomString() + RandomString.LoadString);
                    request.BodyBytes = body;
                }
            }

            return request;
        }
    }
}
