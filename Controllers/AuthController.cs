using DotnetMongoStarter.DTOs;
using DotnetMongoStarter.Models;
using DotnetMongoStarter.Services;
using DotnetMongoStarter.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DotnetMongoStarter.Utils.ApiException;

namespace DotnetMongoStarter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto body)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                     .Select(e => e.ErrorMessage)
                                     .ToList();
                throw new ValidationException("Validation failed.", errors);
            }
            
            var user = await _userService.GetUserByEmail(body.Email);
            if(user != null)
            {
                throw new ApiException("User already exists.", 400, new List<string> { "User already exists." });
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(body.Password);
            var newUser = await _userService.CreateUser(new User
            {
                Name = body.Name,
                Email = body.Email,
                Password = hashedPassword,
                Role = body.Role
            });

            var (accessToken, refreshToken) = _tokenService.GenerateTokens(newUser);
            await _userService.SaveUserToken(newUser.Id!, refreshToken);

            var response = new RegisterResponseDto
            {
                Id = newUser.Id!,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return Ok(ApiResponse<RegisterResponseDto>.Success(response, "User registered successfully."));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto body)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                     .Select(e => e.ErrorMessage)
                                     .ToList();
                throw new ValidationException("Validation failed.", errors);
            }

            var user = await _userService.GetUserByEmail(body.Email);
            if(user == null)
            {
                throw new ApiException("User not found.", 404, new List<string> { "User not found." });
            }

            if (!BCrypt.Net.BCrypt.Verify(body.Password, user.Password))
            {
                throw new ValidationException("Invalid password.", new List<string> { "The password provided is incorrect." });
            }

            var (accessToken, refreshToken) = _tokenService.GenerateTokens(user);
            await _userService.SaveUserToken(user.Id!, refreshToken);

            var response = new LoginResponseDto
            {
                Id = user.Id!,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return Ok(ApiResponse<LoginResponseDto>.Success(response, "User logged in successfully."));
        }

        [HttpPost("refresh/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshAccessToken(string token)
        {
            var principal = _tokenService.GetPrincipalFromToken(token);
            if (principal == null)
            {
                throw new ValidationException("Invalid refresh token.", new List<string> { "The provided token is invalid." });
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserById(userId!);

            var (accessToken, refreshToken) = _tokenService.GenerateTokens(user);
            await _userService.SaveUserToken(user.Id!, refreshToken);

            var response = new RefreshTokenResponseDto
            {
                Id = user.Id!,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return Ok(ApiResponse<RefreshTokenResponseDto>.Success(response, "Token refreshed successfully."));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> UserLogout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new ValidationException("Invalid user.", new List<string> { "User ID not found in token." });
            }

            await _userService.DeleteToken(userId);
            return Ok(ApiResponse<object>.Success(null, "User logged out successfully."));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("Invalid user.", new List<string> { "User ID not found in token." });
            }

            var user = await _userService.GetUserById(userId);

            user.Password = null;
            user.RefreshToken = null;
            return Ok(ApiResponse<User>.Success(user, "User details retrieved successfully."));
        }
    }
}