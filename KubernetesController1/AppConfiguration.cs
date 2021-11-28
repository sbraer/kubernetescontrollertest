using Microsoft.Extensions.Configuration;

namespace KubernetesController1;
public class AppConfiguration
{
    public AppConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", false) // get values from appsettings.json
            .AddEnvironmentVariables() // replace value from {value_env_variable} if exist
            .AddEnvironmentVariablesFromFile() // replace value in {value_env_variable}_FILE is exist
            .Build();

        Group = configuration.GetSection("Group")?.Value ?? throw new ArgumentNullException("group");
        Version = configuration.GetSection("Version")?.Value ?? throw new ArgumentNullException("Version");
        NamespaceParameter = configuration.GetSection("NamespaceParameter")?.Value ?? throw new ArgumentNullException("NamespaceParameter");
        Plural = configuration.GetSection("Plural")?.Value ?? throw new ArgumentNullException("Plural");
        FieldSelector = configuration.GetSection("FieldSelector")?.Value ?? throw new ArgumentNullException("FieldSelector");
    }

    public string Group { get; }
    public string Version { get; }
    public string NamespaceParameter { get; }
    public string Plural { get; }
    public string FieldSelector { get; }
}

