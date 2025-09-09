using Leadway_RSA_API.Data;
using Leadway_RSA_API.DTOs;
using Leadway_RSA_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Leadway_RSA_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public AuthController(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("login")]
        public async IActionResult Login([FromBody] CreateLoginDto loginDto)
        {
            // You will replace this with your actual user validation logic.
            // This is just a placeholder example. You would typically query
            // a database for a user with the matching email and password.
            // DO NOT store passwords in plain text! Use a hashing algorithm like BCrypt.
            bool isValidUser = await IsValidUserAsync(loginDto.Email, loginDto.Password);
            if (!isValidUser)
            {
                return Unauthorized("Invalid credentials."); // Return 401 Unauthorized if login fails
            }

            // If the user is valid, generate a token.
            var tokenString = GenerateJwtToken(loginDto.Email);

            // Return the token in a new DTO
            return Ok(new { token = tokenString });
        }

        private async Task<bool> IsValidUserAsync(string email, string password)
        {
            // 1. Find the applicant by their email
            // We use AsNoTracking() because we're only reading data, not updating it.
            var applicant = await _context.Applicants
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(a => a.EmailAddress == email);

            if (applicant == null)
            {
                // Applicant not found
                return false;
            }

            // 2. Validate the password
            // IMPORTANT: For a real-world app, you MUST use password hashing.
            // This example assumes a plain-text password for demonstration.
            // A secure implementation would use a library like BCrypt.NET.
            return true;
        }

        private string GenerateJwtToken(string email)
        {
            // Get the JWT configuration from appsettings.json
            var jwtIssuer = _config["Jwt:Issuer"];
            var jwtAudience = _config["Jwt:Audience"];
            var jwtKey = _config["Jwt:Key"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Add claims to the token (e.g., user's email, roles, etc.)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // You can add more claims here, like user roles
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "Applicant")
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // Token will expire in 30 minutes
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
