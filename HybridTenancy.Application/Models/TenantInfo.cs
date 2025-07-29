using HybridTenancy.Shared.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

public class TenantInfo
{
    [SwaggerSchema(ReadOnly = true)]
    public Guid? TenantId { get; set; }
    public string? Identifier { get; set; }
    [SwaggerSchema(ReadOnly = true)]
    public string? ConnectionString { get; set; }
    public TenantMode? Mode { get; set; }
    [SwaggerSchema(ReadOnly = true)]
    public DateTime? ValidTill { get; set; }
    [SwaggerSchema(ReadOnly = true)]
    public DateTime? CreatedOn { get; set; }

    [JsonPropertyName("duration")]
    public string? Duration { get; set; }
}
