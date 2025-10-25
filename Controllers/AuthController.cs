using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HRMApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // ====================== ĐĂNG KÝ ======================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return BadRequest("User already exists!");

            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = string.IsNullOrEmpty(model.Email) ? $"{model.Username}@example.com" : model.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // tạo role nếu chưa có
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            // gán role cho user
            await _userManager.AddToRoleAsync(user, "User");

            return Ok("User created successfully!");
        }

        // ====================== ĐĂNG NHẬP (ĐÃ SỬA) ======================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName), // Claim này chứa Username
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""), // ✨ THÊM CLAIM EMAIL
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // thêm role claim
                foreach (var userRole in userRoles)
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(3), // Có thể tăng thời gian hết hạn
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                // ✨ TRẢ VỀ THÊM USERNAME VÀ EMAIL ✨
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    // ✨ Thêm thông tin người dùng vào response ✨
                    user = new
                    {
                        username = user.UserName, // Key là 'username'
                        email = user.Email       // Key là 'email'
                    }
                });
            }

            return Unauthorized("Sai tên đăng nhập hoặc mật khẩu");
        }

        // ====================== LẤY THÔNG TIN CÁ NHÂN (HÀM MỚI) ======================
        [Authorize] // Yêu cầu phải có token hợp lệ
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Lấy user hiện tại từ token (dựa vào ClaimTypes.NameIdentifier hoặc ClaimTypes.Name)
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized(new { message = "Không tìm thấy người dùng hoặc token không hợp lệ" });
            }

            // Trả về thông tin cần thiết
            return Ok(new
            {
                username = user.UserName, // Key là 'username'
                email = user.Email       // Key là 'email'
                // Bạn có thể trả về thêm thông tin khác nếu cần
            });
        }


        // ====================== ĐỔI MẬT KHẨU ======================
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            if (model == null ||
                string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return BadRequest(new { message = "Thiếu mật khẩu cũ hoặc mật khẩu mới" });
            }

            // Lấy user hiện tại từ token
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "Không tìm thấy người dùng" });

            // Đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = $"Đổi mật khẩu thất bại: {errors}" });
            }

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }
    }

    // ====================== MODEL ======================
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

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}
