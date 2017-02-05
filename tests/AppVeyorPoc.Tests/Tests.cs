using AppVeyorPoc.Core;
using System;
using Xunit;

namespace Tests
{
    public class Tests
    {
        [Fact]
        public void Test1()
        {
            var c = new Core();

            var bar = c.Foo(1);

            Assert.Equal(bar, "1");
        }
    }
}
