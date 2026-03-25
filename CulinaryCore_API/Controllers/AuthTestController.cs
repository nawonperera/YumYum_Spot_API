using CulinaryCore.API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthTestController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public ActionResult<string> GetSomething()
    {
        return "You are authorized User";
    }

    [HttpGet("{someValue:int}")]
    [Authorize(Roles = SD.Role_Admin)]
    public ActionResult<string> GetSomething(int someValue)
    {
        return "You are authorized User, with Role of Admin";
    }
}
