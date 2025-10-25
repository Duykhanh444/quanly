using HRMApi.Data;
using HRMApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 Bảo vệ các API chính (trừ QuickAdd)
    public class KhoHangController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KhoHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Lấy danh sách tất cả kho của user đăng nhập
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KhoHang>>> GetAll()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "test-user";

                var list = await _context.KhoHang
                    .Where(k => k.UserId == userId)
                    .OrderByDescending(k => k.NgayNhap)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi GetAll: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Lấy chi tiết 1 kho
        [HttpGet("{id}")]
        public async Task<ActionResult<KhoHang>> GetById(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "test-user";

                var kho = await _context.KhoHang
                    .FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId);

                if (kho == null) return NotFound("Không tìm thấy kho hàng.");

                return Ok(kho);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi GetById: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Thêm kho mới
        [HttpPost]
        public async Task<ActionResult<KhoHang>> Create([FromBody] KhoHang model)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "test-user";

                model.UserId = userId;
                model.NgayNhap = DateTime.Now;
                model.TrangThai = model.TrangThai ?? "Hoạt động";

                _context.KhoHang.Add(model);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi Create: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Cập nhật kho
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] KhoHang model)
        {
            if (id != model.Id)
                return BadRequest("ID không khớp.");

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "test-user";
                var existing = await _context.KhoHang.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId);

                if (existing == null)
                    return NotFound("Không tìm thấy kho hàng.");

                _context.Entry(existing).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi Update: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Xóa kho
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "test-user";
                var kho = await _context.KhoHang.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId);

                if (kho == null)
                    return NotFound("Không tìm thấy kho hàng.");

                _context.KhoHang.Remove(kho);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi Delete: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Xuất kho
        [HttpPut("Xuat/{id}")]
        public async Task<IActionResult> XuatKho(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "test-user";
                var kho = await _context.KhoHang.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId);

                if (kho == null)
                    return NotFound("Không tìm thấy kho hàng.");
                if (kho.TrangThai == "Đã xuất")
                    return BadRequest("Kho này đã xuất rồi.");

                kho.TrangThai = "Đã xuất";
                kho.NgayXuat = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok(kho);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi XuatKho: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Thêm hoặc sửa kho hàng (tự động xác định theo Id)
        [HttpPost("ThemHoacSua")]
        public async Task<IActionResult> ThemHoacSua([FromBody] KhoHang model)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "test-user";
                model.UserId = userId;

                if (model.Id == 0)
                {
                    model.NgayNhap = DateTime.Now;
                    model.TrangThai = model.TrangThai ?? "Hoạt động";
                    _context.KhoHang.Add(model);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Đã thêm mới kho hàng", data = model });
                }
                else
                {
                    var existing = await _context.KhoHang.FirstOrDefaultAsync(k => k.Id == model.Id && k.UserId == userId);
                    if (existing == null)
                        return NotFound("Không tìm thấy kho hàng.");

                    _context.Entry(existing).CurrentValues.SetValues(model);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Đã cập nhật kho hàng", data = existing });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi ThemHoacSua: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Nhập nhanh qua QR / JSON — ai quét cũng thêm được
        [HttpPost("QuickAdd")]
        [AllowAnonymous]
        public async Task<IActionResult> QuickAdd([FromBody] KhoHang model)
        {
            try
            {
                model.UserId = model.UserId ?? "guest";
                model.NgayNhap = DateTime.Now;
                model.TrangThai = model.TrangThai ?? "Hoạt động";

                _context.KhoHang.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Tạo kho thành công qua QR!",
                    data = model
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi QuickAdd: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Kiểm tra kho theo Id (thay vì MaKho)
        [HttpGet("TimTheoMa/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> TimTheoMa(int id)
        {
            try
            {
                var kho = await _context.KhoHang.FirstOrDefaultAsync(k => k.Id == id);
                if (kho == null)
                    return NotFound("Kho chưa tồn tại.");
                return Ok(kho);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi TimTheoMa: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}