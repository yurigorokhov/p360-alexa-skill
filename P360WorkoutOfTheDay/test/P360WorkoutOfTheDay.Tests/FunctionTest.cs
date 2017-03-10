using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using P360WorkoutOfTheDay;

namespace P360WorkoutOfTheDay.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestToUpperFunction()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            var result = function.FunctionHandler("hello world", context);
            result.Wait();

            Assert.Equal("HELLO WORLD", result.Result);
        }
    }
}
