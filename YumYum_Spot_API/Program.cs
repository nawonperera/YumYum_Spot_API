using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using YumYum_Spot_API.Data;
using YumYum_Spot_API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllers();

// Get the JWT secret key from appsettings.json (ApiSettings:Secret). This is the key used to sign and validate JWT tokens.
var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");
builder.Services.AddAuthentication(options =>
{
    // This tells ASP.NET Core to use JWT Bearer tokens as the main authentication method.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    // If a user is not authenticated, ASP.NET Core will challenge them using JWT rules.
    // If the user is not logged in (no valid JWT), ASP.NET Core will return a 401 error and tell the client that it needs to send a valid JWT token.
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(u =>
{
    u.RequireHttpsMetadata = false; // Allow HTTP (not required to be HTTPS). Only use this during development.
    u.SaveToken = true; // Store the JWT token in the authentication handler after it's validated.
                        // This allows the token to be accessed later inside the request pipeline if needed.
    u.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters // Settings used to validate incoming JWT tokens.
    {
        ValidateIssuerSigningKey = true, // Ensure the token has a valid signing key. Without this, anyone could create fake tokens.
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)), // The server will use THIS key to check if the token is valid.
        ValidateIssuer = false, // We are not checking who created the token.
                                // Normally a token has an 'issuer' field (example: "myapi.com").
                                // Since ValidateIssuer = false, we are skipping that check for simplicity.
        ValidateAudience = false, // We are not checking who the token is meant for.
                                  // Normally a token has an 'audience' value (like the name of the app it belongs to).
                                  // Since ValidateAudience = false, we are ignoring that check.
        ValidateLifetime = true // Check the token's expiration time.
                                // If the token is expired, it will be rejected.
    };
});


builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Enable serving default files (like index.html) and static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
