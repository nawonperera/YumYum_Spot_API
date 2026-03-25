using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CulinaryCore.API.Models;

public class MenuItem
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
    [Required]
    public string Image { get; set; } = string.Empty;
    [NotMapped] // when this used, following property does not add to the database.
    public double Rating { get; set; }
}
