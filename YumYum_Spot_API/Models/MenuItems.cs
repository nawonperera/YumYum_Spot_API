using System.ComponentModel.DataAnnotations;

namespace YumYum_Spot_API.Models;

public class MenuItems
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SpecialTag { get; set; } = string.Empty;
    [Range(0, 1000)]
    public double Price { get; set; }
    [Required]
    public string Image { get; set; } = string.Empty;
}
