using Leadway_RSA_API.Data;
using Leadway_RSA_API.DTOs;
using Leadway_RSA_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Leadway_RSA_API.Services
{
    public class LoginService : ILoginService
    {
        private readonly ApplicationDbContext _context;

        public LoginService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Login?> Login(CreateLoginDto loginDto)
        {


            
        }

    }
}
