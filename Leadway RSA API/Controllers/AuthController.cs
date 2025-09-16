using Leadway_RSA_API.Data;
using Leadway_RSA_API.DTOs;
using Leadway_RSA_API.Models;
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
    [Route("api/admin/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        // Hardcoded credentials for a single admin user.
        // WARNING: In a production environment, these should be stored in a secure
        // configuration provider or environment variables, NOT hardcoded.
        private const string AdminEmail = "admin";
        private const string AdminPassword = "password123"; // This should be a strong, hashed password


        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Authenticates a single hardcoded administrator and returns a JWT with an 'Admin' role.
        /// </summary>
        [HttpPost("login")]
        public IActionResult AdminLogin([FromBody] AdminLoginDto loginDto)
        {
            // 1. Validate the hardcoded credentials.
            // In a real app, the password would be hashed and compared securely.
            if (loginDto.EmailAddress != AdminEmail || loginDto.Password != AdminPassword)
            {
                return Unauthorized("Invalid credentials.");
            }

            // 2. Create the JWT with an 'Admin' role claim.
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, AdminEmail),
            new Claim(ClaimTypes.Role, "Admin") // The crucial claim for role-based authorization
        };
            var jwtToken = GenerateJwtToken(claims);

            // 3. Return the JWT.
            return Ok(new { jwtToken });
        }


        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
