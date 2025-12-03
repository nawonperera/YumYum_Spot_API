using Microsoft.AspNetCore.Mvc;
using YumYum_Spot_API.Data;
using YumYum_Spot_API.Models;
using YumYum_Spot_API.Models.Dto;

namespace YumYum_Spot_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MenuItemController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ApiResponse _response;
    private readonly IWebHostEnvironment _env;

    public MenuItemController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _response = new ApiResponse();
        _env = env;
    }

    [HttpGet]
    public IActionResult GetMenuItems()
    {
        _response.Result = _db.MenuItems.ToList();
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        return Ok(_response);
    }
    [HttpGet("{id:int}", Name = "GetMenuItem")]
    public IActionResult GetMenuItem(int id)
    {
        if (id <= 0)
        {
            _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            return BadRequest(_response);
        }
        MenuItem? menuItem = _db.MenuItems.FirstOrDefault(u => u.Id == id);
        _response.Result = menuItem;
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        return Ok(_response);
    }

    [HttpPost]
    // [FromForm] MenuItemCreateDTO menuItemCreateDTO → The data comes from a form (because you likely upload an image using IFormFile).
    public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO menuItemCreateDTO)
    {
        try
        {
            if (ModelState.IsValid)
            {
                if (menuItemCreateDTO.File == null || menuItemCreateDTO.File.Length == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.ErrorMessages = ["Image file is required."];
                    return BadRequest(_response);
                }

                // Save the uploaded file to wwwroot/images
                var ImagePath = Path.Combine(_env.WebRootPath, "images");  // Builds the full physical path to wwwroot/images -> Combine wwwroot folder path and images folder name.
                if (!Directory.Exists(ImagePath))
                {
                    Directory.CreateDirectory(ImagePath); // Create the images directory if it doesn't exist.
                }

                var filePath = Path.Combine(ImagePath, menuItemCreateDTO.File.FileName);

                if (System.IO.File.Exists(filePath))
                // Checks if a file with the same name already exists in the images folder
                // Path.Combine(ImagePath, menuItemCreateDTO.File.FileName);->Creates the full physical file path (e.g., C:/project/wwwroot/images/myphoto.jpg)
                {
                    System.IO.File.Delete(filePath); // Deletes the existing file to avoid conflicts.
                }

                // Uploading the image file
                using (var stream = new FileStream(filePath, FileMode.Create)) // Opens/creates a file so we can write data into it
                {
                    await menuItemCreateDTO.File.CopyToAsync(stream);
                    // CopyToAsync(stream) -> menuItemCreateDTO.File is an IFormFile (an abstraction representing the uploaded file).
                    // CopyToAsync(stream) starts copying bytes from the uploaded file into your FileStream.
                    // This method copies in small chunks(buffers), not the whole file in one giant piece.
                }

                MenuItem menuItem = new()
                {
                    Name = menuItemCreateDTO.Name,
                    Description = menuItemCreateDTO.Description,
                    Category = menuItemCreateDTO.Category,
                    SpecialTag = menuItemCreateDTO.SpecialTag,
                    Price = menuItemCreateDTO.Price,
                    Image = "images/" + menuItemCreateDTO.File.FileName // Storing the relative path to the image
                };
                _db.MenuItems.Add(menuItem);
                await _db.SaveChangesAsync();

                _response.Result = menuItemCreateDTO;
                _response.StatusCode = System.Net.HttpStatusCode.Created;

                // Returns a 201 Created response.
                // "GetMenuItem" is the name of the route used to fetch a single menu item.
                // new { id = menuItem.Id } fills the route parameter so the URL becomes /api/MenuItem/{id}.
                // The Location header in the response will point to the new item's URL.
                // _response is the body returned to the client.
                return CreatedAtRoute("GetMenuItem", new { id = menuItem.Id }, _response);
            }
            else
            {
                _response.IsSuccess = false;
            }
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = [ex.ToString()];
        }

        return BadRequest(_response);
    }
}
