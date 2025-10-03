using HRMApi.Data;
using HRMApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using System.Security.Cryptography;
using System.Text;

namespace HRMApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========== REGISTER ==========
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest(new { message = "Email đã tồn tại!" });

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đăng ký thành công",
                user.Id,
                user.Email,
                user.UserName,
                user.Role
            });
        }

        // ========== LOGIN ==========
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.Username);
            if (user == null || user.PasswordHash != HashPassword(model.Password))
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });

            return Ok(new
            {
                message = "Đăng nhập thành công",
                user.Id,
                user.Email,
                user.UserName,
                user.Role
            });
        }

        // ========== GOOGLE LOGIN ==========
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginModel model)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = payload.Email.Split('@')[0],
                        Email = payload.Email,
                        AvatarUrl = payload.Picture,
                        Role = "User"
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    message = "Đăng nhập Google thành công",
                    user.Id,
                    user.Email,
                    user.UserName,
                    user.Role
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Google token không hợp lệ", error = ex.Message });
            }
        }

        // ========== SUPPORT ==========
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    // DTOs
    public class RegisterModel
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class GoogleLoginModel
    {
        public string IdToken { get; set; } = "";
    }
}
