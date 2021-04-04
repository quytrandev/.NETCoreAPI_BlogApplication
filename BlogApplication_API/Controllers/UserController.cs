using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Services;
using DTO;
using BCrypt;
using System.Security.Claims;
using BlogApplication_API.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BlogApplication_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly IJwtAuthManager _jwtAuthManager;

        public UserController(ILogger<UserController> logger, IUserService userService, IJwtAuthManager jwtAuthManager)
        {
            _logger = logger;
            _userService = userService;
            _jwtAuthManager = jwtAuthManager;
        }

        [HttpPost]
        public IActionResult Register([FromBody] UserDTO userDTO)
        {
            userDTO.UserId = new Guid();
            userDTO.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(userDTO.Password, BCrypt.Net.HashType.SHA256);

            ServiceResponse<UserDTO> response = _userService.Register(userDTO);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login([FromBody] LoginRequest request)
        {
            bool isValid = _userService.IsValidUserCredentials(request);
            if (isValid)
            {
                var userId = _userService.GetUserId(request.Email);
                var claims = new[]
                {
                 new Claim(ClaimTypes.Name,request.Email),
                 new Claim(ClaimTypes.NameIdentifier, userId)
                };

                var jwtResult = _jwtAuthManager.GenerateTokens(request.Email, claims, DateTime.Now);
                LoginResult loginResult = new LoginResult
                {
                    UserId = Guid.Parse(userId),
                    Email = request.Email,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString,
                    ExpiredAt = jwtResult.RefreshToken.ExpireAt
                };

                bool isTokenAdded = _userService.IsUserTokenAdded(loginResult);
                if (isTokenAdded)
                {
                    _logger.LogInformation($"User [{request.Email}] logged in the system.");
                    return Ok(loginResult);
                }
                else
                {
                    return Ok(new
                    {
                        message = "Invalid token"
                    });
                }

            }
            else
            {
                return Ok(new
                {
                    message = "Username or password was incorrect"
                });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public string Ping()
        {
            return "pong";
        }

        [HttpGet("{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetProfile(Guid userId)
        {
            ServiceResponse<UserDTO> response = _userService.GetProfile(userId);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateUsername([FromForm] UserDTO userDTO)
        {
            ServiceResponse<UserDTO> response = _userService.UpdateUsername(userDTO.UserId, userDTO.Username);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ChangePassword([FromForm] UserDTO userDTO)
        {
            ServiceResponse<UserDTO> response = _userService.ChangePassword(userDTO.UserId, userDTO.Password, userDTO.NewPassword);

            if (!response.isSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Logout()
        {
            var userName = User.Identity?.Name;
            _jwtAuthManager.RemoveRefreshTokenByUserName(userName);
            _logger.LogInformation($"User [{userName}] logged out the system.");
            return Ok();
        }
    }
}
