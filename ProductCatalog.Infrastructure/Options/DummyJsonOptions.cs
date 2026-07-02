using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Infrastructure.Options;

public class DummyJsonOptions
{
    public const string SectionName = "DummyJson";

    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    [Range(1, 60)]
    public int TimeoutSeconds { get; set; } = 10;

    [Range(1, 60)]
    public int AuthValidationCacheMinutes { get; set; } = 5;
}