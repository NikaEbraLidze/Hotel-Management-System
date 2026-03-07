using System;
using System.Threading.Tasks;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace hms.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        public HotelController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpGet("get-hotels")]
        public async Task<IActionResult> GetHotelsAsync([FromQuery] GetHotelsRequestDTO request)
        {
            var hotels = await _hotelService.GetHotelsAsync(request);

            var response = CommonResponse.Success(
                hotels,
                "Hotels retrieved successfully.",
                System.Net.HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPost("register-hotel")]
        public async Task<IActionResult> RegisterHotelAsync([FromBody] RegisterHotelRequestDTO request)
        {
            var hotel = await _hotelService.RegisterHotelAsync(request);

            var response = CommonResponse.Success(
                hotel,
                "Hotel registered successfully.",
                System.Net.HttpStatusCode.Created);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }
    }
}