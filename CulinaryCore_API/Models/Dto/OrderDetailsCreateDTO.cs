using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CulinaryCore.API.Models.Dto;

public class OrderDetailsCreateDTO
{
    [Required]
    public int MenuItemId { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public string ItemName { get; set; } = string.Empty;
    [Required]
    public double Price { get; set; }
}
