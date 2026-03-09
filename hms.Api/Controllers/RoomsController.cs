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
    public class roomsController : ControllerBase
    {
        private readonly IRoomsService _roomsService;

        public roomsController(IRoomsService roomsService)
        {
            _roomsService = roomsService;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRooms(
            [FromRoute] Guid hotelId,
            [FromQuery] GetAvailableRoomsRequestDTO request)
        {
            var rooms = await _roomsService.GetAvailableRoomsAsync(hotelId, request);

            var response = CommonResponse.Success(
                rooms,
                "Available rooms retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms(
            [FromRoute] Guid hotelId,
            [FromQuery] GetRoomsRequestDTO request)
        {
            var rooms = await _roomsService.GetRoomsAsync(hotelId, request);

            var response = CommonResponse.Success(
                rooms,
                "Rooms retrieved successfully.",
                HttpStatusCode.OK);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpGet("{roomId:guid}")]
        public async Task<IActionResult> GetRoomById(
            [FromRoute] Guid hotelId,
            [FromRoute] Guid roomId
        )
        {
            var room = await _roomsService.GetRoomByIdAsync(hotelId, roomId);

            var response = CommonResponse.Success(
                room,
                "Room retrieved successfully.",
                HttpStatusCode.OK
            );

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateRoom(
            [FromRoute] Guid hotelId,
            [FromBody] CreateRoomRequestDTO request
        )
        {
            var room = await _roomsService.CreateRoomAsync(hotelId, request);

            var response = CommonResponse.Success(
                room,
                "Room created successfully.",
                HttpStatusCode.Created
            );

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPut("{roomId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateRoom(
            [FromRoute] Guid hotelId,
            [FromRoute] Guid roomId,
            [FromBody] UpdateRoomRequestDTO request
        )
        {
            var room = await _roomsService.UpdateRoomAsync(hotelId, roomId, request);

            var response = CommonResponse.Success(
                room,
                "Room updated successfully.",
                HttpStatusCode.OK
            );

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpDelete("{roomId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteRoom(
            [FromRoute] Guid hotelId,
            [FromRoute] Guid roomId
        )
        {
            await _roomsService.DeleteRoomAsync(hotelId, roomId);

            var response = CommonResponse.Success(
                null,
                "Room deleted successfully.",
                HttpStatusCode.OK
            );

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }
    }
}
