using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CulinaryCore.API.Models;

public class OrderDetail
{
    [Key]
    public int OrderDetailsId { get; set; }
    [Required]
    public int OrderHeaderId { get; set; }
    [Required]
    public int MenuItemId { get; set; }
    [ForeignKey(nameof(MenuItemId))]
    public MenuItem? MenuItem { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public string ItemName { get; set; } = string.Empty;
    [Required]
    public double Price { get; set; }
    public int? Rating { get; set; } = null;
    
}
