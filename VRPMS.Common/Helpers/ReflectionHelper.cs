using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using VRPMS.Common.Services;

namespace VRPMS.Common.Helpers;

public static class ReflectionHelper
{
    private const string VrpmsAssemblyPrefix = "VRPMS.";
    private const string LibraryFileNameWildcard = "*.dll";

    private static IEnumerable<Type> GetAllTypesThatImplementInterface<T>(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(type => typeof(T).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false });
    }

    private static Assembly[] GetSolutionAssemblies(string nameContains = VrpmsAssemblyPrefix)
    {
        var fileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, LibraryFileNameWildcard);

        if (!string.IsNullOrEmpty(nameContains))
        {
            fileNames = fileNames.Where(w => Path.GetFileName(w).Contains(nameContains) && !Path.GetFileName(w).Contains(AppDomain.CurrentDomain.FriendlyName)).ToArray();
        }

        return fileNames.Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x))).ToArray();
    }

    public static void RegisterAssemblies(this IServiceCollection services)
    {
        var assemblies = GetSolutionAssemblies()
            .Where(w => w.GetTypes().Any(type => typeof(IRegistrable).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false }))
            .ToList();

        foreach (var assembly in assemblies)
        {
            foreach (var type in GetAllTypesThatImplementInterface<IRegistrable>(assembly))
            {
                var instance = (IRegistrable)Activator.CreateInstance(type)!;
                instance.Register(services);
            }
        }
    }
}