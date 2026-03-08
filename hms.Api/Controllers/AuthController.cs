using System;
using System.Net;
using System.Threading.Tasks;
using hms.Application.Contracts.Service;
using hms.Application.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace hms.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class authController : ControllerBase
    {
        private readonly IAuthService _authService;

        public authController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register/guest")]
        public async Task<IActionResult> RegisterGuestAsync([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            var registrationResponse = await _authService.RegisterGuestAsync(registrationRequestDTO);
            var response = CommonResponse.Success(
                registrationResponse,
                "Guest registered successfully.",
                HttpStatusCode.Created);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPost("register/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            var registrationResponse = await _authService.RegisterAdminAsync(registrationRequestDTO);
            var response = CommonResponse.Success(
                registrationResponse,
                "Admin registered successfully.",
                HttpStatusCode.Created);

            return StatusCode(Convert.ToInt32(response.StatusCode), response);
        }

        [HttpPost("register/manager")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RegisterManagerAsync([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            var registrationResponse = await _authService.RegisterManagerAsync(registrationRequestDTO);
            var response = CommonResponse.Success(
                registrationResponse,
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
