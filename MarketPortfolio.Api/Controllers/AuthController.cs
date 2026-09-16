using MarketPortfolio.Application.Interfaces;
using MarketPortfolio.Application.Services;
using MarketPortfolio.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MarketPortfolio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtTokenGenerator _tokenGenerator;

        public AuthController(IUserRepository userRepository, JwtTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            // 1. Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                return BadRequest(new { Error = "User with this email already exists." });
            }

            // 2. Create the user entity (defaulting role to 'Free' if not specified)
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Role = string.IsNullOrEmpty(request.Role) ? "Free" : request.Role,
                // In a production app, use BCrypt or ASP.NET Core Identity for hashing passwords securely
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password) 
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            // 3. Generate and return the JWT token
            var token = _tokenGenerator.GenerateToken(user);

            return Ok(new { Token = token, Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            // 1. Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { Error = "Invalid email or password." });
            }

            // 2. Generate and return the JWT token
            var token = _tokenGenerator.GenerateToken(user);

            return Ok(new { Token = token, Message = "Login successful." });
        }
    }

    // DTO Records for Request payloads
    public record RegisterRequest(string Email, string Password, string Role);
    public record LoginRequest(string Email, string Password);
}