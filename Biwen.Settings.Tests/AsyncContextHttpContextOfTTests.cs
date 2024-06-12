using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AsyncState;

namespace Biwen.Settings.Tests
{
    public class AsyncContextHttpContextOfTTests(ITestOutputHelper testOutput) :
        TestBase(testOutput)
    {
        [Fact]
        public void Test1()
        {
            var services = new ServiceCollection();
            services.AddAsyncStateHttpContext();

            var serviceProvider = services.BuildServiceProvider();

            var asyncContext = serviceProvider.GetRequiredService<IAsyncContext<Thing>>();

            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            httpContextAccessor.HttpContext = new DefaultHttpContext();

            serviceProvider.GetRequiredService<IAsyncState>().Initialize();

            var thing = new Thing("hello async context");

            asyncContext.Set(thing);
            var flag = asyncContext.TryGet(out var thing2);

            Assert.True(flag);
            Assert.Equal(thing, thing2);


            Output.WriteLine("Test1");
        }
    }


    public record Thing(string? Name);
}