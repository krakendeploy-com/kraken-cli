using System.Net.Http.Json;
using Kraken.Cli.Models;

namespace Kraken.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var parameters = ParseArguments(args);

        if (!parameters.TryGetValue("Action", out var action))
        {
            Console.WriteLine("Missing required parameter: --Action");
            return;
        }

        // Common required parameters
        var baseParams = new[] { "OrgId", "WorkspaceID", "ProjectId", "ApiKey" };

        foreach (var param in baseParams)
            if (!parameters.ContainsKey(param))
            {
                Console.WriteLine($"Missing required parameter: --{param}");
                return;
            }

        var orgId = parameters["OrgId"];
        var workspaceId = parameters["WorkspaceID"];
        var projectId = parameters["ProjectId"];
        var apiKey = parameters["ApiKey"];

        switch (action)
        {
            case "CreateRelease":
                if (!parameters.TryGetValue("Version", out var releaseVersion))
                {
                    Console.WriteLine("Missing required parameter: --Version");
                    return;
                }

                if (!parameters.TryGetValue("Packages", out var packagesRaw))
                {
                    Console.WriteLine("Missing required parameter: --Packages");
                    return;
                }

                var packages = ParsePackages(packagesRaw);
                parameters.TryGetValue("Name", out var releaseName); // Optional

                await CreateReleaseAsync(orgId, workspaceId, projectId, apiKey, releaseVersion, packages, releaseName);
                break;

            case "UploadPackage":
                if (!parameters.TryGetValue("File", out var filePath))
                {
                    Console.WriteLine("Missing required parameter: --File");
                    return;
                }

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File does not exist: {filePath}");
                    return;
                }

                if (!parameters.TryGetValue("Version", out var packageVersion))
                {
                    Console.WriteLine("Missing required parameter: --Version");
                    return;
                }

                UploadPackage(orgId, workspaceId, projectId, apiKey, filePath, packageVersion);
                break;

            default:
                Console.WriteLine($"Unknown action: {action}");
                break;
        }
    }

    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in args)
            if (arg.StartsWith("--"))
            {
                var split = arg.Substring(2).Split('=', 2);
                if (split.Length == 2) result[split[0]] = split[1];
            }

        return result;
    }

    private static Dictionary<string, string> ParsePackages(string input)
    {
        var dict = new Dictionary<string, string>();
        var entries = input.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            var parts = entry.Split('=', 2);
            if (parts.Length == 2)
                dict[parts[0]] = parts[1];
            else
                Console.WriteLine($"⚠️ Invalid package entry: '{entry}'");
        }

        return dict;
    }

    private static async Task CreateReleaseAsync(string orgId, string workspaceId, string projectId, string apiKey,
        string version, Dictionary<string, string> packages, string releaseName)
    {
        Console.WriteLine("Creating release...");

        var input = new CreateReleaseInput
        {
            Name = releaseName,
            Version = version,
            Packages = packages
        };

        var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var url = $"/organization/{orgId}/workspaces/{workspaceId}/projects/{projectId}/releases/create";

        try
        {
            var response = await client.PostAsJsonAsync(url, input);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Release created successfully.");
            }
            else
            {
                Console.WriteLine($"❌ Failed to create release: {response.StatusCode}");
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception occurred: {ex.Message}");
        }
    }

    private static void UploadPackage(string orgId, string workspaceId, string projectId, string apiKey,
        string filePath, string version)
    {
        Console.WriteLine("Uploading package...");
        Console.WriteLine($"File: {filePath}");
        Console.WriteLine($"Version: {version}");

        // TODO: Implement actual upload logic
    }
}