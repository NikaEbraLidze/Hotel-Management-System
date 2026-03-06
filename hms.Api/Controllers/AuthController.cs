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
            var response = CommonResponse.Success(
                new { UserId = userId },
                "Guest registered successfully.",
                HttpStatusCode.Created);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPost("register/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            var userId = await _authService.RegisterAdminAsync(registrationRequestDTO);
            var response = CommonResponse.Success(
                new { UserId = userId },
                "Admin registered successfully.",
                HttpStatusCode.Created);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPost("register/manager")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RegisterManagerAsync([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            var userId = await _authService.RegisterManagerAsync(registrationRequestDTO);
            var response = CommonResponse.Success(
                new { UserId = userId },
                "Manager registered successfully.",
                HttpStatusCode.Created);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var loginResponse = await _authService.LoginAsync(loginRequestDTO);
            var response = CommonResponse.Success(
                loginResponse,
                "User logged in successfully.");

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

    }
}
