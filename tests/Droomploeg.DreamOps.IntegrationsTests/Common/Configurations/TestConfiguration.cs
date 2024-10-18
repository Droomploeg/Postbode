using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Droomploeg.DreamOps.IntegrationsTests.Common.Configurations;

public static class TestConfiguration
{
    public static IConfiguration Configuration()
    {
        var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
        var path = assemblyFile.Directory!.FullName;
        return new ConfigurationBuilder()
            .SetBasePath(path)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }


}
