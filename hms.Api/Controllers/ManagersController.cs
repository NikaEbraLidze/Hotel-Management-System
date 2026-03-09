using System;
using System.Net;
using System.Threading.Tasks;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hms.Api.Controllers
{
    [ApiController]
    [Route("api/{hotelId:guid}/[controller]")]
    public class managersController : ControllerBase
    {
        private readonly IHotelManagersService _hotelManagersService;

        public managersController(IHotelManagersService hotelManagersService)
        {
            _hotelManagersService = hotelManagersService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AddHotelManagerAsync(
            [FromRoute] Guid hotelId,
            [FromBody] AddHotelManagerRequestDTO request)
        {
            var hotelManager = await _hotelManagersService.AddHotelManagerAsync(hotelId, request);

            var response = CommonResponse.Success(
                hotelManager,
                "Manager assigned to hotel successfully.",
                HttpStatusCode.Created);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetHotelManagersAsync([FromRoute] Guid hotelId)
        {
            var hotelManagers = await _hotelManagersService.GetHotelManagersAsync(hotelId);

            var response = CommonResponse.Success(
                hotelManagers,
                "Hotel managers retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpGet("{managerId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetHotelManagerByIdAsync(
            [FromRoute] Guid hotelId,
            [FromRoute] Guid managerId)
        {
            var hotelManager = await _hotelManagersService.GetHotelManagerByIdAsync(hotelId, managerId);

            var response = CommonResponse.Success(
                hotelManager,
                "Hotel manager retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPut("{managerId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateHotelManagerAsync(
            [FromRoute] Guid hotelId,
            [FromRoute] Guid managerId,
            [FromBody] UpdateHotelManagerRequestDTO request)
        {
            var hotelManager = await _hotelManagersService.UpdateHotelManagerAsync(hotelId, managerId, request);

            var response = CommonResponse.Success(
                hotelManager,
                "Hotel manager updated successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpDelete("{managerId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteHotelManagerAsync(
            [FromRoute] Guid hotelId,
            [FromRoute] Guid managerId)
        {
            await _hotelManagersService.DeleteHotelManagerAsync(hotelId, managerId);

            var response = CommonResponse.Success(
                null,
                "Hotel manager deleted successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }
    }
}
