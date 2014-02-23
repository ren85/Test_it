using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestIt.Tests;

namespace TestIt.Transform
{
    interface ITransform
    {
        Request Transform(Request request);
    }
}
