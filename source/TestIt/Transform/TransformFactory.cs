using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestIt.Tests;

namespace TestIt.Transform
{
    public class TransformFactory
    {
        public static Request PerformTransformations(Request request)
        {
            if (!string.IsNullOrEmpty(request.BodyBytes))
            {
                request.BodyBytes = Helpers.BytesToString(Helpers.BytesFromDelimitedString(request.BodyBytes));
            }

            var list = new List<ITransform>();
            list.Add(new RandomString());
            list.Add(new NextNumber());
            foreach (var t in list)
            {
                request = t.Transform(request);
            }
            return request;
        }
    }
}
