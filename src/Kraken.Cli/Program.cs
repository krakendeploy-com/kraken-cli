using System.Net.Http.Json;
using Kraken.Cli.Models;

namespace Kraken.Cli;

internal class Program
{
    private const string DefaultBaseUrl = "https://api.krakendeploy.com";

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
        
        // Optional: Allow base URL override via parameter or environment variable
        var baseUrl = parameters.TryGetValue("BaseUrl", out var url) 
            ? url 
            : Environment.GetEnvironmentVariable("KRAKEN_API_URL") ?? DefaultBaseUrl;

        switch (action)
        {
            case "CreateRelease":
                if (!parameters.TryGetValue("Version", out var releaseVersion))
                {
                    Console.WriteLine("Missing required parameter: --Version");
                    return;
                }

                // Support both --Packages (artifacts) and --RegistryImages
                var hasPackages = parameters.TryGetValue("Packages", out var packagesRaw);
                var hasRegistryImages = parameters.TryGetValue("RegistryImages", out var registryImagesRaw);

                if (!hasPackages && !hasRegistryImages)
                {
                    Console.WriteLine("Missing required parameter: At least one of --Packages or --RegistryImages must be provided");
                    return;
                }

                var artifacts = hasPackages ? ParsePackages(packagesRaw!) : new List<ArtifactInfo>();
                var registryImages = hasRegistryImages ? ParsePackages(registryImagesRaw!) : new List<ArtifactInfo>();
                parameters.TryGetValue("Name", out var releaseName); // Optional

                await CreateReleaseAsync(orgId, workspaceId, projectId, apiKey, releaseVersion, artifacts, registryImages, releaseName, baseUrl);
                break;

            case "CreateDeployment":
                // Support either ReleaseId or Version parameter
                var hasReleaseId = parameters.TryGetValue("ReleaseId", out var releaseId);
                var hasVersion = parameters.TryGetValue("Version", out var deployVersion);

                if (!hasReleaseId && !hasVersion)
                {
                    Console.WriteLine("Missing required parameter: Either --ReleaseId or --Version must be provided");
                    return;
                }

                if (hasReleaseId && hasVersion)
                {
                    Console.WriteLine("Cannot specify both --ReleaseId and --Version. Use one or the other.");
                    return;
                }

                if (!parameters.TryGetValue("EnvironmentId", out var environmentId))
                {
                    Console.WriteLine("Missing required parameter: --EnvironmentId");
                    return;
                }

                if (hasReleaseId)
                {
                    await CreateDeploymentByReleaseIdAsync(orgId, workspaceId, projectId, apiKey, environmentId, releaseId!, baseUrl);
                }
                else
                {
                    await CreateDeploymentByVersionAsync(orgId, workspaceId, projectId, apiKey, environmentId, deployVersion!, baseUrl);
                }
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

    private static List<ArtifactInfo> ParsePackages(string input)
    {
        var artifacts = new List<ArtifactInfo>();
        var entries = input.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            var parts = entry.Split('=', 2);
            if (parts.Length == 2)
                artifacts.Add(new ArtifactInfo(parts[0], parts[1]));
            else
                Console.WriteLine($"⚠️ Invalid package entry: '{entry}'");
        }

        return artifacts;
    }

    private static bool ValidateApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || !apiKey.StartsWith("krk_"))
        {
            Console.WriteLine("❌ Invalid API key format. API key must start with 'krk_'.");
            return false;
        }
        return true;
    }

    private static HttpClient CreateHttpClient(string apiKey, string? baseUrl = null)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl ?? DefaultBaseUrl)
        };
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }

    private static async Task CreateReleaseAsync(string orgId, string workspaceId, string projectId, string apiKey,
        string version, List<ArtifactInfo> artifacts, List<ArtifactInfo> registryImages, string? releaseName, string baseUrl)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(version))
        {
            Console.WriteLine("❌ Version cannot be empty.");
            return;
        }

        if ((artifacts == null || artifacts.Count == 0) && (registryImages == null || registryImages.Count == 0))
        {
            Console.WriteLine("❌ At least one artifact or registry image must be specified.");
            return;
        }

        if (!ValidateApiKey(apiKey))
        {
            return;
        }

        Console.WriteLine("Creating release...");
        if (artifacts != null && artifacts.Count > 0)
        {
            Console.WriteLine($"  📦 Artifacts: {string.Join(", ", artifacts.Select(a => $"{a.Name}={a.Version}"))}");
        }
        if (registryImages != null && registryImages.Count > 0)
        {
            Console.WriteLine($"  🐳 Registry Images: {string.Join(", ", registryImages.Select(r => $"{r.Name}={r.Version}"))}");
        }

        var input = new CreateReleaseInput
        {
            Name = releaseName,
            Version = version,
            Artifacts = artifacts.Count > 0 ? artifacts : null,
            RegistryImages = registryImages.Count > 0 ? registryImages : null
        };

        var client = CreateHttpClient(apiKey, baseUrl);
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

    private static async Task CreateDeploymentByReleaseIdAsync(string orgId, string workspaceId, string projectId, string apiKey,
        string environmentId, string releaseId, string baseUrl)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(environmentId))
        {
            Console.WriteLine("❌ EnvironmentId cannot be empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(releaseId))
        {
            Console.WriteLine("❌ ReleaseId cannot be empty.");
            return;
        }

        if (!ValidateApiKey(apiKey))
        {
            return;
        }

        Console.WriteLine($"Creating deployment with Release ID: {releaseId}...");

        var client = CreateHttpClient(apiKey, baseUrl);
        var url = $"/organization/{orgId}/workspaces/{workspaceId}/projects/{projectId}/environments/{environmentId}/deployments/releases/{releaseId}/create";

        try
        {
            var response = await client.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Deployment created successfully.");
            }
            else
            {
                Console.WriteLine($"❌ Failed to create deployment: {response.StatusCode}");
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception occurred: {ex.Message}");
        }
    }

    private static async Task CreateDeploymentByVersionAsync(string orgId, string workspaceId, string projectId, string apiKey,
        string environmentId, string version, string baseUrl)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(environmentId))
        {
            Console.WriteLine("❌ EnvironmentId cannot be empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            Console.WriteLine("❌ Version cannot be empty.");
            return;
        }

        if (!ValidateApiKey(apiKey))
        {
            return;
        }

        Console.WriteLine($"Creating deployment with Release Version: {version}...");

        var client = CreateHttpClient(apiKey, baseUrl);
        var url = $"/organization/{orgId}/workspaces/{workspaceId}/projects/{projectId}/environments/{environmentId}/deployments/version/{version}/create";

        try
        {
            var response = await client.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Deployment created successfully.");
            }
            else
            {
                Console.WriteLine($"❌ Failed to create deployment: {response.StatusCode}");
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