namespace Kraken.Cli.Models;

public class CreateReleaseInput
{
    public string? Name { get; set; }
    public string? Version { get; set; }
    public Dictionary<string, string>? Packages { get; set; }
}