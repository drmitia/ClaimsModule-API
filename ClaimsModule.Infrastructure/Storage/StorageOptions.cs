namespace ClaimsModule.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";
    public string LocalBasePath { get; set; } = "uploads";
    public string? AzureConnectionString { get; set; }
    public string? AzureContainerName { get; set; }

    public string GetResolvedLocalPath()
    {
        if (Path.IsPathRooted(LocalBasePath))
            return LocalBasePath;

        // Resolve relative to project root not bin folder
        var basePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..",
            LocalBasePath);

        return Path.GetFullPath(basePath);
    }
}