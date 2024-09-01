using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DockerDashboard.Shared.Data;

public class ImageModel
{
    [Key]
    public required string ImageId { get; set; }

    public string ShortId => ImageId.Substring(0, Math.Min(12, ImageId.Length));

    [Key]
    public required string ImageName { get; set; }

    [Key]
    public string? ImageTag { get; set; }

    public DateTime Created { get; set; }

    public long Size { get; set; }

    public long Countainers { get; set; }
}