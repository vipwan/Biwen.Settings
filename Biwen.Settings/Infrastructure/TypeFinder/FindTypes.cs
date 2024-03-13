namespace Biwen.Settings.Infrastructure.TypeFinder
{
    public static class FindTypes
    {
        public static IInAssemblyFinder InAssembly(Assembly assembly) => new InAssemblyFinder([assembly]);

        public static IInAssemblyFinder InAssemblies(params Assembly[] assemblies) => new InAssemblyFinder(assemblies);

        public static IInAssemblyFinder InCurrentAssembly => InAssembly(Assembly.GetCallingAssembly());

        public static IInAssemblyFinder InAllAssemblies => InAssemblies(AppDomain.CurrentDomain.GetAssemblies());
    }
}
