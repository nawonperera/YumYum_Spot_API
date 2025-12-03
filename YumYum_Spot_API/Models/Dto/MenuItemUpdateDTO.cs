using System.ComponentModel.DataAnnotations;

namespace YumYum_Spot_API.Models.Dto;

public class MenuItemUpdateDTO
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? SpecialTag { get; set; }
    [Range(0, 1000)]
    public double Price { get; set; }
    public IFormFile? File { get; set; } //IFormFile is a built-in ASP.NET Core interface used to handle uploaded files coming from an HTML form.
}
