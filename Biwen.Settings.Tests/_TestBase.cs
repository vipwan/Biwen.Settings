namespace Biwen.Settings.Tests
{
    public abstract class TestBase(ITestOutputHelper testOutputHelper)
    {
        public ITestOutputHelper Output { get; private set; } = testOutputHelper;
    }
}