using CulinaryCore.API.Data;
using CulinaryCore.API.Models;
using CulinaryCore.API.Models.Dto;
using CulinaryCore.API.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CulinaryCore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderDetailsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ApiResponse _response;

    public OrderDetailsController(ApplicationDbContext db)
    {
        _db = db;
        _response = new ApiResponse();
    }

    [HttpPut("{orderDetailsId:int}")]
    public ActionResult<ApiResponse> UpdateOrder(int orderDetailsId, [FromBody] OrderDetailsUpdateDTO orderDetailsUpdate)
    {
        try
        {
            if (ModelState.IsValid)
            {
                if (orderDetailsId != orderDetailsUpdate.OrderDetailId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Invalid Id");
                    return NotFound(_response);
                }


                OrderDetail? orderDetailFromDb = _db.OrderDetails.FirstOrDefault(u => u.OrderDetailsId == orderDetailsId);

                if (orderDetailFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Order not found");
                    return NotFound(_response);
                }

                orderDetailFromDb.Rating = orderDetailsUpdate.Rating;

                
                _db.SaveChanges();


                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMessages = ModelState.Values.SelectMany(u => u.Errors).Select(u => u.ErrorMessage).ToList();
                return BadRequest(_response);
            }
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            _response.ErrorMessages.Add(ex.Message);
            return StatusCode((int)HttpStatusCode.InternalServerError, _response);
        }
    }
}
