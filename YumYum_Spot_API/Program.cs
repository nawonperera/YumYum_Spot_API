using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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

/*========================================
 =      Athuentication Configuration     =
 =========================================*/

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


builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

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




// This class customizes the OpenAPI documentation to include Bearer (JWT) authentication details.
internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
// internal -> this class can be used only inside the same project
// sealed   -> no other class can inherit from this class
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Get all registered authentication schemes in the application.
        // "authentication schemes" means all the auth methods your app supports
        // Example: Bearer, Cookies, OAuth, Microsoft, Google etc.
        var authnticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        // Check if the "Bearer" authentication is configured in the app.
        // JwtBearerDefaults.AuthenticationScheme = "Bearer"
        if (authnticationSchemes.Any(authScheme => authScheme.Name == JwtBearerDefaults.AuthenticationScheme))
        {
            // Create the security scheme requirement for Bearer (JWT) authentication.
            var requirement = new Dictionary<string, OpenApiSecurityScheme>
            {
                // Key: scheme name ("Bearer")
                // Value: configuration of the Bearer security type in Swagger
                [JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,   // using HTTP-based security
                    Scheme = "bearer",                // type is Bearer token
                    In = ParameterLocation.Header,    // token should be sent in the HTTP header
                    BearerFormat = "JWT",             // token format is JWT
                }
            };

            // Initialize Components if it is null.
            // ??= means: "if document.Components is null, assign a new OpenApiComponents()"
            document.Components ??= new OpenApiComponents();
            // Add the Bearer security scheme to Swagger.
            document.Components.SecuritySchemes = requirement;
        }

        // Set the general Swagger document information.
        document.Info = new()
        {
            Title = "YumYum Spot API",                      // Swagger title
            Version = "v1",                                 // API version
            Description = "API for YumYum Spot Restaurant Application", // simple API description
        };
    }
}
