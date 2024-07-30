using Alba;
using Biwen.Settings.Tests.WebTests;
using System.Text.Json.Nodes;

namespace Biwen.Settings.Tests
{
    using static Microsoft.AspNetCore.Builder.BiwenSettingApis;

    public class WebTestCases(WebAppFixture<TestWebUI.Program> app, ITestOutputHelper testOutput) :
        IClassFixture<WebAppFixture<TestWebUI.Program>>
    {
        private readonly IAlbaHost _host = app.AlbaHost;
        private readonly ITestOutputHelper _testOutput = testOutput;

        //http://localhost:5150/biwensetting/api/all

        [Fact]
        public async Task get_all_settings_test()
        {

            var result = await _host.Scenario(x =>
            {
                x.Get.Url("/biwensetting/api/all");
                x.StatusCodeShouldBeOk();
            });

            var settings = await result.ReadAsJsonAsync<IEnumerable<SettingDto>>();
            Assert.NotNull(settings);

            _testOutput.WriteLine(await result.ReadAsTextAsync());
        }

        [Fact]
        public async Task get_setting_test()
        {

            var type = "Biwen.Settings.TestWebUI.GithubSetting";
            var host = await AlbaHost.For<TestWebUI.Program>();

            var result = await _host.Scenario(x =>
            {
                x.Get.Url($"/biwensetting/api/get/{type}");
                x.StatusCodeShouldBeOk();
            });

            var setting = await result.ReadAsJsonAsync<SettingDto>();
            Assert.NotNull(setting);
            Assert.Equal(type, setting.SettingType);

            _testOutput.WriteLine(await result.ReadAsTextAsync());
        }

        [Fact]
        public async Task set_setting_test()
        {
            var type = "Biwen.Settings.TestWebUI.GithubSetting";

            var result = await _host.Scenario(x =>
               {
                   x.Post.Json(new
                   {
                       Token = "1111111134567",
                   })
                   .ContentType("application/json-patch+json")
                   .Accepts("application/json")
                   .ToUrl($"/biwensetting/api/set/{type}");
               });

            var json = result.ReadAsJson<JsonObject>();

            Assert.NotNull(json);
            Assert.Equal("1111111134567", json["token"]?.ToString());
        }
    }

}