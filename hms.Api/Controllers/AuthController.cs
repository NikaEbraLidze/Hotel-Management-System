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
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register/guest")]
        public async Task<IActionResult> RegisterGuestAsync([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            var userId = await _authService.RegisterGuestAsync(registrationRequestDTO);
            return StatusCode(Convert.ToInt32(HttpStatusCode.Created), new CommonResponse
            {
                IsSuccess = true,
                Message = "Guest registered successfully.",
                StatusCode = HttpStatusCode.Created,
                Data = new { UserId = userId }
            });
        }

        [HttpPost("register/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            var userId = await _authService.RegisterAdminAsync(registrationRequestDTO);
            return StatusCode(Convert.ToInt32(HttpStatusCode.Created), new CommonResponse
            {
                IsSuccess = true,
                Message = "Admin registered successfully.",
                StatusCode = HttpStatusCode.Created,
                Data = new { UserId = userId }
            });
        }

        [HttpPost("register/manager")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RegisterManagerAsync([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            var userId = await _authService.RegisterManagerAsync(registrationRequestDTO);
            return StatusCode(Convert.ToInt32(HttpStatusCode.Created), new CommonResponse
            {
                IsSuccess = true,
                Message = "Manager registered successfully.",
                StatusCode = HttpStatusCode.Created,
                Data = new { UserId = userId }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var loginResponse = await _authService.LoginAsync(loginRequestDTO);
            return StatusCode(Convert.ToInt32(HttpStatusCode.OK), new CommonResponse
            {
                IsSuccess = true,
                Message = "User logged in successfully.",
                StatusCode = HttpStatusCode.OK,
                Data = loginResponse
            });
        }

    }
}