using HRMApi.Data;
using HRMApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhoHangController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KhoHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Lấy tất cả kho hàng của user đăng nhập
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KhoHang>>> GetAll()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var list = await _context.KhoHang
                    .Where(k => k.UserId == userId)
                    .ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAll KhoHang Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Lấy 1 kho theo Id của user
        [HttpGet("{id}")]
        public async Task<ActionResult<KhoHang>> GetById(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var kho = await _context.KhoHang.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId);

                if (kho == null) return NotFound();
                return Ok(kho);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetById KhoHang Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Tạo kho mới cho user hiện tại
        [HttpPost]
        public async Task<ActionResult<KhoHang>> Create(KhoHang kho)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                kho.UserId = userId;

                _context.KhoHang.Add(kho);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = kho.Id }, kho);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create KhoHang Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Cập nhật kho
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, KhoHang kho)
        {
            if (id != kho.Id) return BadRequest();

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var existing = await _context.KhoHang.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId);
                if (existing == null) return NotFound();

                // Gán lại UserId cho chắc
                kho.UserId = userId;

                // ✅ Cập nhật tất cả field (bao gồm GiaTri)
                _context.Entry(existing).CurrentValues.SetValues(kho);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhoHangExists(id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update KhoHang Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Xóa kho
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var kho = await _context.KhoHang.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId);
                if (kho == null) return NotFound();

                _context.KhoHang.Remove(kho);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete KhoHang Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 🔹 Xuất kho (chỉ user của kho đó mới được xuất)
        [HttpPut("Xuat/{id}")]
        public async Task<IActionResult> XuatKho(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var kho = await _context.KhoHang.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId);
                if (kho == null) return NotFound();

                if (kho.TrangThai == "Đã xuất")
                    return BadRequest("Kho này đã xuất.");

                kho.TrangThai = "Đã xuất";
                kho.NgayXuat = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok(kho);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"XuatKho Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        private bool KhoHangExists(int id)
        {
            return _context.KhoHang.Any(e => e.Id == id);
        }
    }
}
