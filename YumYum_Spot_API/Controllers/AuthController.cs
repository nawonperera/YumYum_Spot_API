using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using YumYum_Spot_API.Models;
using YumYum_Spot_API.Models.Dto;
using YumYum_Spot_API.Utility;

namespace YumYum_Spot_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly ApiResponse _response;
    private readonly UserManager<ApplicationUser> _userManager; // This is a service provided by Identity.
                                                                // It is used to create, update, delete, and manage users.
                                                                // Example: creating a user, checking password, adding roles, etc.
    private readonly RoleManager<IdentityRole> _roleManager; // Provides methods like: Create role, Delete role, Assign user to role, etc.
    private readonly string secretKey;

    public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        secretKey = configuration.GetValue<string>("ApiSettings:Secret") ?? "";
        _response = new ApiResponse();
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO model)
    {
        if (ModelState.IsValid)
        {
            ApplicationUser newUser = new()
            {
                Email = model.Email,
                UserName = model.Email,
                Name = model.Name,
                NormalizedEmail = model.Email.ToUpper()
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                // Check if the role specified in the model exists
                // _roleManager.RoleExistsAsync returns a Task<bool>, true if role exists
                // GetAwaiter().GetResult() is used here to synchronously wait for the result
                if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                    await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                }

                // Compares the role string in the model with SD.Role_Admin
                // StringComparison.CurrentCultureIgnoreCase → ignores case differences(e.g., "admin" = "Admin").
                if (model.Role.Equals(SD.Role_Admin, StringComparison.CurrentCultureIgnoreCase))
                {
                    await _userManager.AddToRoleAsync(newUser, SD.Role_Admin);
                }
                else
                {
                    await _userManager.AddToRoleAsync(newUser, SD.Role_Customer);
                }

                _response.StatusCode = System.Net.HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    _response.ErrorMessages.Add(error.Description);
                }
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
        }
        else
        {
            _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            foreach (var error in ModelState.Values)
            {
                foreach (var item in error.Errors)
                {
                    _response.ErrorMessages.Add(item.ErrorMessage);
                }
            }
            return BadRequest(_response);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
    {
        if (ModelState.IsValid)
        {

            var userFromDb = await _userManager.FindByEmailAsync(model.Email);

            if (userFromDb != null)
            {
                bool isValid = await _userManager.CheckPasswordAsync(userFromDb, model.Password);
                if (!isValid)
                {
                    _response.Result = new LoginResponseDTO();
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Invalid Password");
                    return BadRequest(_response);
                }

                // Generate JWT Token
                JwtSecurityTokenHandler tokenHandler = new(); // JwtSecurityTokenHandler is responsible for creating and reading JWT tokens

                byte[] key = System.Text.Encoding.ASCII.GetBytes(secretKey); // Convert the secret key (string) into a byte array
                                                                             // This key is used to sign the JWT token
                                                                             // Describe the details of the JWT token we want to create
                SecurityTokenDescriptor tokenDescriptor = new()
                {
                    // The "Subject" represents the identity of the user (claims about the user)
                    Subject = new ClaimsIdentity(
                        [
                        new Claim("fullname", userFromDb.Name), // Custom claim: full name of the user
                        new Claim("id", userFromDb.Id),  // Custom claim: user ID
                        new Claim(ClaimTypes.Email, userFromDb.Email!.ToString()), // Standard claim: user's email address
                                                                             // !. means, we are sure Email is not null
                        new Claim(ClaimTypes.Role, _userManager.GetRolesAsync(userFromDb).Result.FirstOrDefault()!) // Add the user's role as a claim in the JWT token
                                         // ClaimTypes.Role → standard claim name for user roles
                                         // _userManager.GetRolesAsync(userFromDb) → gets all roles assigned to the user
                                         // .Result → waits for the result synchronously (not recommended, but works)
                                         // .FirstOrDefault() → takes the first role from the list (e.g., "Admin" or "Customer")
                                         // ! → tells the compiler that the value will not be null
                        ]),

                    Expires = DateTime.UtcNow.AddDays(7),

                    //When you create a JWT token you sign it(Combine it) using a secret key and an algorithm(like HMAC-SHA256).
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key), // The secret key used to sign the JWT token
                        SecurityAlgorithms.HmacSha256Signature) // The algorithm used to sign the token (HMAC-SHA256)
                };

                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor); // Create the JWT token based on the descriptor

                LoginResponseDTO loginResponse = new()
                {
                    Email = userFromDb.Email,
                    Token = tokenHandler.WriteToken(token), // Convert the JWT token into a string format
                    Role = _userManager.GetRolesAsync(userFromDb).Result.FirstOrDefault()!
                };
                _response.Result = loginResponse;
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);

            }
            _response.Result = new LoginResponseDTO();
            _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Invalid Password");
            return BadRequest(_response);
        }
        else
        {
            _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            foreach (var error in ModelState.Values)
            {
                foreach (var item in error.Errors)
                {
                    _response.ErrorMessages.Add(item.ErrorMessage);
                }
            }
            return BadRequest(_response);
        }
    }

}
