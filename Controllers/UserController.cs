using HRMApi.Data;
using HRMApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace HRMApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ======================
        // 🔹 REGISTER (Email + Password)
        // ======================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email và mật khẩu không được để trống" });

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Email đã tồn tại" });

            var newUser = new User
            {
                UserName = request.UserName ?? request.Email.Split('@')[0],
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Role = "User"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công", newUser.Id, newUser.UserName, newUser.Email });
        }

        // ======================
        // 🔹 LOGIN (Email + Password)
        // ======================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized(new { message = "Email không tồn tại" });

            if (user.PasswordHash != HashPassword(request.Password))
                return Unauthorized(new { message = "Sai mật khẩu" });

            var token = Guid.NewGuid().ToString(); // 👉 Sau này có thể thay bằng JWT

            return Ok(new { message = "Đăng nhập thành công", user.Id, user.UserName, user.Email, user.Role, token });
        }

        // ======================
        // 🔹 LOGIN GMAIL
        // ======================
        [HttpPost("login-gmail")]
        public async Task<IActionResult> LoginWithGmail([FromBody] GmailLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { message = "Email không hợp lệ" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = request.Email.Split('@')[0],
                    Email = request.Email,
                    Role = "User",
                    PasswordHash = "" // Gmail login thì để trống
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var token = Guid.NewGuid().ToString();

            return Ok(new { message = "Đăng nhập Gmail thành công", user.Id, user.UserName, user.Email, user.Role, token });
        }

        // ======================
        // 🔹 GET ALL USERS
        // ======================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // ======================
        // 🔹 UPDATE USER
        // ======================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User không tồn tại" });

            user.UserName = request.UserName ?? user.UserName;
            user.Email = request.Email ?? user.Email;
            user.Role = request.Role ?? user.Role;
            if (!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = HashPassword(request.Password);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công", user });
        }

        // ======================
        // 🔹 DELETE USER
        // ======================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "User không tồn tại" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa user thành công" });
        }

        // ======================
        // 🔹 Support: Hash password
        // ======================
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    // ======================
    // 🔹 DTO Models
    // ======================
    public class RegisterRequest
    {
        public string? UserName { get; set; }
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class GmailLoginRequest
    {
        public string Email { get; set; } = "";
    }

    public class UpdateUserRequest
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
    }
}
