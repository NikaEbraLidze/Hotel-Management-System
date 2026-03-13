using System;
using System.Net;
using System.Threading.Tasks;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hms.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class hotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        public hotelsController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHotelsAsync([FromQuery] GetHotelsRequestDTO request)
        {
            var hotels = await _hotelService.GetHotelsAsync(request);

            var response = CommonResponse.Success(
                hotels,
                "Hotels retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpGet("{Id:guid}")]
        public async Task<IActionResult> GetHotelByIdAsync([FromRoute] Guid Id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(Id);

            var response = CommonResponse.Success(
                hotel,
                "Hotel retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterHotelAsync(
            [FromForm] RegisterHotelRequestDTO request,
            IFormFile image)
        {
            var hotel = await _hotelService.RegisterHotelAsync(request, image);

            var response = CommonResponse.Success(
                hotel,
                "Hotel registered successfully.",
                HttpStatusCode.Created);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPut("{Id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateHotelAsync(
            [FromRoute] Guid Id,
            [FromForm] UpdateHotelRequestDTO request,
            IFormFile image)
        {
            var hotel = await _hotelService.UpdateHotelAsync(Id, request, image);

            var response = CommonResponse.Success(
                hotel,
                "Hotel updated successfully",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpDelete("{Id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHotelAsync([FromRoute] Guid Id)
        {
            await _hotelService.DeleteHotelAsync(Id);

            var response = CommonResponse.Success(
                null,
                "Hotel deleted successfully",
                HttpStatusCode.OK
            );

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }
    }
}
