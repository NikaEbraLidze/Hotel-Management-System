using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hms.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class reservationsController : ControllerBase
    {
        private readonly IReservationsService _reservationsService;

        public reservationsController(IReservationsService reservationsService)
        {
            _reservationsService = reservationsService;
        }

        [HttpPost("hotels/{hotelId:guid}/reservations")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> CreateReservationAsync(
            [FromRoute] Guid hotelId,
            [FromBody] CreateReservationRequestDTO request)
        {
            var guestId = GetCurrentUserId();

            var reservation = await _reservationsService.CreateReservationAsync(hotelId, guestId, request);

            var response = CommonResponse.Success(
                reservation,
                "Reservation created successfully.",
                HttpStatusCode.Created);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpGet("reservations/{reservationId:guid}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> GetReservationByIdAsync([FromRoute] Guid reservationId)
        {
            var guestId = GetCurrentUserId();

            var reservation = await _reservationsService.GetReservationByIdAsync(reservationId, guestId);

            var response = CommonResponse.Success(
                reservation,
                "Reservation retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpGet("reservations")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> GetMyReservationsAsync()
        {
            var guestId = GetCurrentUserId();

            var reservations = await _reservationsService.GetMyReservationsAsync(guestId);

            var response = CommonResponse.Success(
                reservations,
                "Your reservations retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPut("reservations/{reservationId:guid}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> UpdateReservationAsync(
            [FromRoute] Guid reservationId,
            [FromBody] UpdateReservationRequestDTO request)
        {
            var guestId = GetCurrentUserId();

            var reservation = await _reservationsService.UpdateReservationAsync(reservationId, guestId, request);

            var response = CommonResponse.Success(
                reservation,
                "Reservation updated successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpDelete("reservations/{reservationId:guid}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> DeleteReservationAsync([FromRoute] Guid reservationId)
        {
            var guestId = GetCurrentUserId();

            await _reservationsService.DeleteReservationAsync(reservationId, guestId);

            var response = CommonResponse.Success(
                null,
                "Reservation cancelled successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpGet("hotels/{hotelId:guid}/reservations")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetHotelReservationsAsync(
            [FromRoute] Guid hotelId,
            [FromQuery] GetHotelReservationsRequestDTO request)
        {
            var currentUserId = GetCurrentUserId();

            var reservations = await _reservationsService.GetHotelReservationsAsync(
                hotelId,
                currentUserId,
                User.IsInRole("Admin"),
                request);

            var response = CommonResponse.Success(
                reservations,
                "Hotel reservations retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpGet("hotels/{hotelId:guid}/reservations/available-rooms")]
        public async Task<IActionResult> GetAvailableReservationRoomsAsync(
            [FromRoute] Guid hotelId,
            [FromQuery] GetAvailableReservationRoomsRequestDTO request)
        {
            var rooms = await _reservationsService.GetAvailableRoomsAsync(hotelId, request);

            var response = CommonResponse.Success(
                rooms,
                "Available rooms retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(userId, out var parsedUserId))
                throw new UnauthorizedAccessException("Authenticated user ID is missing or invalid.");

            return parsedUserId;
        }
    }
}
