using Alba.Security;
using Alba;
using System.IdentityModel.Tokens.Jwt;

namespace Biwen.Settings.Tests.WebTests
{
    public class WebAppFixture<T> : IAsyncLifetime where T : class
    {

        public IAlbaHost AlbaHost { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            // This is a Alba extension that can "stub" out authentication
            var securityStub = new AuthenticationStub()
                .With(type: "foo", "bar")
                .With(JwtRegisteredClaimNames.Email, "guy@company.com")
                .WithName("jeremy");

            AlbaHost = await Alba.AlbaHost.For<T>(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    //configure services
                });

            }, securityStub);
        }

        public async Task DisposeAsync()
        {
            await AlbaHost.DisposeAsync();
        }
    }
}
