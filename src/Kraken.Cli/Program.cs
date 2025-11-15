using System.Net.Http.Headers;
using System.Net.Http.Json;
using Kraken.Cli.Models;

namespace Kraken.Cli;

internal class Program
{
    private const string DefaultBaseUrl = "https://api.krakendeploy.com";

    private static async Task Main(string[] args)
    {
        var parameters = ParseArguments(args);

        if (!parameters.TryGetValue("action", out var action))
        {
            Console.WriteLine("Missing required parameter: --action");
            return;
        }


        var baseParams = new[] { "org-id", "workspace-id", "project-id", "api-key" };

        foreach (var param in baseParams)
            if (!parameters.ContainsKey(param))
            {
                Console.WriteLine($"Missing required parameter: --{param}");
                return;
            }

        var orgId = parameters["org-id"];
        var workspaceId = parameters["workspace-id"];
        var projectId = parameters["project-id"];
        var apiKey = parameters["api-key"];

        var baseUrl = parameters.GetValueOrDefault("base-url", DefaultBaseUrl);

        switch (action)
        {
            case "create-release":
                if (!parameters.TryGetValue("version", out var releaseVersion))
                {
                    Console.WriteLine("Missing required parameter: --version");
                    return;
                }

                var hasPackages = parameters.TryGetValue("packages", out var packagesRaw);
                var hasRegistryImages = parameters.TryGetValue("registry-images", out var registryImagesRaw);

                if (!hasPackages && !hasRegistryImages)
                {
                    Console.WriteLine(
                        "Missing required parameter: At least one of --packages or --registry-images must be provided");
                    return;
                }

                var artifacts = hasPackages ? ParsePackages(packagesRaw!) : new List<ArtifactInfo>();
                var registryImages = hasRegistryImages ? ParsePackages(registryImagesRaw!) : new List<ArtifactInfo>();

                await CreateReleaseAsync(orgId, workspaceId, projectId, apiKey, releaseVersion, artifacts,
                    registryImages, baseUrl);
                break;

            case "create-deployment":
                var hasReleaseId = parameters.TryGetValue("release-id", out var releaseId);
                var hasVersion = parameters.TryGetValue("version", out var deployVersion);

                if (!hasReleaseId && !hasVersion)
                {
                    Console.WriteLine("Missing required parameter: Either --release-id or --version must be provided");
                    return;
                }

                if (hasReleaseId && hasVersion)
                {
                    Console.WriteLine("Cannot specify both --release-id and --version. Use one or the other.");
                    return;
                }

                if (!parameters.TryGetValue("environment-id", out var environmentId))
                {
                    Console.WriteLine("Missing required parameter: --environment-id");
                    return;
                }

                if (hasReleaseId)
                    await CreateDeploymentByReleaseIdAsync(orgId, workspaceId, projectId, apiKey, environmentId,
                        releaseId!, baseUrl);
                else
                    await CreateDeploymentByVersionAsync(orgId, workspaceId, projectId, apiKey, environmentId,
                        deployVersion!, baseUrl);
                break;

            default:
                Console.WriteLine($"Unknown action: {action}");
                break;
        }
    }

    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--"))
                continue;

            var trimmed = arg.Substring(2);
            string key;
            string value;

            var eqIndex = trimmed.IndexOf('=');
            if (eqIndex >= 0)
            {
                key = trimmed.Substring(0, eqIndex);
                value = trimmed.Substring(eqIndex + 1);
            }
            else
            {
                key = trimmed;
                if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    value = args[++i];
                else
                    value = "true";
            }

            result[key] = value;
        }

        return result;
    }

    private static List<ArtifactInfo> ParsePackages(string input)
    {
        var artifacts = new List<ArtifactInfo>();
        var entries = input.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            var parts = entry.Split(':', 3);
            if (parts.Length == 3)
                artifacts.Add(new ArtifactInfo(parts[0], parts[1], parts[2]));
            else
                Console.WriteLine($"⚠️ Invalid package entry format. Expected 'name:version:slug-id', got: '{entry}'");
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
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }

    private static async Task CreateReleaseAsync(string orgId, string workspaceId, string projectId, string apiKey,
        string version, List<ArtifactInfo> artifacts, List<ArtifactInfo> registryImages, string baseUrl)
    {
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

        if (!ValidateApiKey(apiKey)) return;

        Console.WriteLine("Creating release...");
        if (artifacts != null && artifacts.Count > 0)
            Console.WriteLine(
                $"  📦 Artifacts: {string.Join(", ", artifacts.Select(a => $"{a.Name}:{a.Version}:{a.ArtifactSourceSlug}"))}");
        if (registryImages != null && registryImages.Count > 0)
            Console.WriteLine(
                $"  🐳 Registry Images: {string.Join(", ", registryImages.Select(r => $"{r.Name}:{r.Version}:{r.ArtifactSourceSlug}"))}");

        var input = new CreateReleaseInput
        {
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

    private static async Task CreateDeploymentByReleaseIdAsync(string orgId, string workspaceId, string projectId,
        string apiKey,
        string environmentId, string releaseId, string baseUrl)
    {
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

        if (!ValidateApiKey(apiKey)) return;

        Console.WriteLine($"Creating deployment with Release ID: {releaseId}...");

        var client = CreateHttpClient(apiKey, baseUrl);
        var url =
            $"/organization/{orgId}/workspaces/{workspaceId}/projects/{projectId}/environments/{environmentId}/deployments/releases/{releaseId}/create";

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

    private static async Task CreateDeploymentByVersionAsync(string orgId, string workspaceId, string projectId,
        string apiKey,
        string environmentId, string version, string baseUrl)
    {
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

        if (!ValidateApiKey(apiKey)) return;

        Console.WriteLine($"Creating deployment with Release Version: {version}...");

        var client = CreateHttpClient(apiKey, baseUrl);
        var url =
            $"/organization/{orgId}/workspaces/{workspaceId}/projects/{projectId}/environments/{environmentId}/deployments/version/{version}/create";

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
}