using Alba;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AsyncState;
using static Microsoft.AspNetCore.Builder.BiwenSettingApis;
using System.Text.Json.Nodes;

namespace Biwen.Settings.Tests
{
    public class AsyncContextHttpContextOfTTests(ITestOutputHelper testOutput) :
        TestBase(testOutput)
    {
        [Fact]
        public void async_context_http_context_of_t_test1()
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