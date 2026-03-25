using CulinaryCore.API.Data;
using CulinaryCore.API.Models;
using CulinaryCore.API.Models.Dto;
using CulinaryCore.API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CulinaryCore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderHeaderController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ApiResponse _response;

    public OrderHeaderController(ApplicationDbContext db)
    {
        _db = db;
        _response = new ApiResponse();
    }

    [HttpGet]
    public ActionResult<ApiResponse> GetOrders(string userId = "")
    {
        IEnumerable<OrderHeader> orderHeaderList = _db.OrderHeaders.Include(u => u.OrderDetails).ThenInclude(u => u.MenuItem).OrderByDescending(u => u.OrderHeaderId);

        if (!string.IsNullOrEmpty(userId))
        {
            orderHeaderList = orderHeaderList.Where(u => u.ApplicationUserId == userId);
        }
        _response.Result = orderHeaderList;
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        return Ok(_response);
    }


    [HttpGet("{orderId:int}")]
    public ActionResult<ApiResponse> GetOrder(int orderId)
    {
        if (orderId == 0)
        {
            _response.IsSuccess = false;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            _response.ErrorMessages.Add("Invalid order Id");
            return BadRequest(_response);
        }

        OrderHeader? orderHeader = _db.OrderHeaders.Include(u => u.OrderDetails).ThenInclude(u => u.MenuItem).FirstOrDefault(u => u.OrderHeaderId == orderId);

        if (orderHeader == null)
        {
            _response.IsSuccess = false;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            _response.ErrorMessages.Add("Order Not Found");
            return BadRequest(_response);
        }


        _response.Result = orderHeader;
        _response.StatusCode = System.Net.HttpStatusCode.OK;
        return Ok(_response);
    }

    [HttpPost]
    public ActionResult<ApiResponse> CreateOrder([FromBody] OrderHeaderCreateDTO orderHeaderDTO)
    {
        try
        {
            if (ModelState.IsValid)
            {
                OrderHeader orderHeader = new()
                {
                    PickUpName = orderHeaderDTO.PickUpName,
                    PickUpPhoneNumber = orderHeaderDTO.PickUpPhoneNumber,
                    PickUpEmail = orderHeaderDTO.PickUpEmail,
                    OrderDate = DateTime.Now,
                    OrderTotal = orderHeaderDTO.OrderTotal,
                    Status = SD.status_confirmed,
                    TotalItem = orderHeaderDTO.TotalItem,
                    ApplicationUserId = orderHeaderDTO.ApplicationUserId,
                };
                _db.OrderHeaders.Add(orderHeader);
                _db.SaveChanges();

                foreach (var orderDetailDto in orderHeaderDTO.OrderDetailsDTO)
                {
                    OrderDetail orderDetails = new()
                    {
                        OrderHeaderId = orderHeader.OrderHeaderId,
                        MenuItemId = orderDetailDto.MenuItemId,
                        Quantity = orderDetailDto.Quantity,
                        ItemName = orderDetailDto.ItemName,
                        Price = orderDetailDto.Price,
                    };
                    _db.OrderDetails.Add(orderDetails);

                }
                _db.SaveChanges();
                // We do not call _db.SaveChanges() inside the foreach loop because it would send a database request for each item. For example, if there are 10 order details, it would hit the database 10 times. Instead, we call _db.SaveChanges() once after the loop to save everything in one go (bulk save).
                _response.Result = orderHeader;
                orderHeader.OrderDetails = [];
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtAction(nameof(GetOrder), new { orderId = orderHeader.OrderHeaderId }, _response);
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


    [HttpPut("{orderId:int}")]
    public ActionResult<ApiResponse> UpdateOrder(int orderId, [FromBody] OrderHeaderUpdateDTO orderHeaderUpdate)
    {
        try
        {
            if (ModelState.IsValid)
            {
                if (orderId != orderHeaderUpdate.OrderHeaderId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Invalid Id");
                    return NotFound(_response);
                }


                OrderHeader? orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.OrderHeaderId == orderId);

                if (orderHeaderFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErrorMessages.Add("Order not found");
                    return NotFound(_response);
                }

                if (!string.IsNullOrEmpty(orderHeaderUpdate.PickUpName))
                {
                    orderHeaderFromDb.PickUpName = orderHeaderUpdate.PickUpName;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdate.PickUpPhoneNumber))
                {
                    orderHeaderFromDb.PickUpName = orderHeaderUpdate.PickUpPhoneNumber;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdate.PickUpEmail))
                {
                    orderHeaderFromDb.PickUpName = orderHeaderUpdate.PickUpEmail;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdate.Status))
                {

                    if (orderHeaderFromDb.Status.Equals(SD.status_confirmed, StringComparison.InvariantCultureIgnoreCase) && orderHeaderUpdate.Status.Equals(SD.status_readyForPickUp, StringComparison.InvariantCultureIgnoreCase))
                    {
                        orderHeaderFromDb.Status = SD.status_readyForPickUp;
                    }
                    if (orderHeaderFromDb.Status.Equals(SD.status_readyForPickUp, StringComparison.InvariantCultureIgnoreCase) && orderHeaderUpdate.Status.Equals(SD.status_completed, StringComparison.InvariantCultureIgnoreCase))
                    {
                        orderHeaderFromDb.Status = SD.status_completed;
                    }
                    if (orderHeaderUpdate.Status.Equals(SD.status_cancelled, StringComparison.InvariantCultureIgnoreCase))
                    {
                        orderHeaderFromDb.Status = SD.status_cancelled;
                    }
                }
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
