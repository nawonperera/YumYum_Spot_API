using System.ComponentModel.DataAnnotations;

namespace YumYum_Spot_API.Models.Dto;

public class MenuItemCreateDTO
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? SpecialTag { get; set; }
    [Range(0, 1000)]
    public double Price { get; set; }
    public IFormFile File { get; set; } = null!;
}
