using Microsoft.AspNetCore.Identity;

namespace YumYum_Spot_API.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
}
