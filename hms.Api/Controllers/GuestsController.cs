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
    [Route("api/[controller]")]
    public class guestsController : ControllerBase
    {
        private readonly IGuestsService _guestsService;

        public guestsController(IGuestsService guestsService)
        {
            _guestsService = guestsService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetGuestsAsync()
        {
            var guests = await _guestsService.GetGuestsAsync();

            var response = CommonResponse.Success(
                guests,
                "Guests retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpGet("{guestId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetGuestByIdAsync([FromRoute] Guid guestId)
        {
            var guest = await _guestsService.GetGuestByIdAsync(guestId);

            var response = CommonResponse.Success(
                guest,
                "Guest retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPut("{guestId:guid}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> UpdateGuestAsync(
            [FromRoute] Guid guestId,
            [FromBody] UpdateGuestRequestDTO request)
        {
            var guest = await _guestsService.UpdateGuestAsync(guestId, request);

            var response = CommonResponse.Success(
                guest,
                "Guest updated successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpDelete("{guestId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGuestAsync([FromRoute] Guid guestId)
        {
            await _guestsService.DeleteGuestAsync(guestId);

            var response = CommonResponse.Success(
                null,
                "Guest deleted successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }
    }
}
