namespace Kraken.Cli.Models;

public class CreateReleaseInput
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public List<ArtifactInfo>? Artifacts { get; set; }
    public List<ArtifactInfo>? RegistryImages { get; set; }
}

public record ArtifactInfo(string Name, string Version);