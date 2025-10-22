using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMApi.Data;
using HRMApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace HRMApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HoaDonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Lấy tất cả hóa đơn của user đăng nhập
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HoaDon>>> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var list = await _context.HoaDons
                .Where(h => h.UserId == userId)
                .Include(h => h.Items) // ✅ Lấy kèm danh sách mặt hàng
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();

            return Ok(list);
        }

        // 🔹 Lấy 1 hóa đơn theo Id
        [HttpGet("{id}")]
        public async Task<ActionResult<HoaDon>> GetById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var hd = await _context.HoaDons
                .Include(h => h.Items) // ✅ Lấy kèm danh sách mặt hàng
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (hd == null)
            {
                return NotFound();
            }
            
            return Ok(hd);
        }

        // 🔹 Thêm hóa đơn mới
        [HttpPost]
        public async Task<ActionResult<HoaDon>> Create(HoaDon hd)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            hd.UserId = userId;
            hd.NgayLap = DateTime.UtcNow; // Dùng UtcNow để đồng bộ thời gian tốt hơn
            hd.MaHoaDon ??= $"HD-{DateTime.Now:yyyyMMddHHmmss}";
            hd.TrangThai ??= "Chưa thanh toán";
            hd.PhuongThuc ??= "Tiền mặt";

            // Tính tổng tiền trên server dựa vào items gửi lên (nếu có)
            hd.TongTien = hd.TinhTongTien();

            _context.HoaDons.Add(hd);
            await _context.SaveChangesAsync();
            
            var createdHd = await _context.HoaDons
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == hd.Id);

            return CreatedAtAction(nameof(GetById), new { id = hd.Id }, createdHd);
        }

        // 🔹 Cập nhật hóa đơn
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, HoaDon hd)
        {
            if (id != hd.Id)
            {
                return BadRequest(new { message = "ID không khớp!" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var existing = await _context.HoaDons
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (existing == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn!" });
            }

            existing.MaHoaDon = hd.MaHoaDon;
            existing.LoaiHoaDon = hd.LoaiHoaDon;
            existing.NgayLap = hd.NgayLap;
            existing.TrangThai = hd.TrangThai;
            existing.PhuongThuc = hd.PhuongThuc;
            
            // 1. Đánh dấu các items cũ để xóa khỏi DB
            _context.HoaDonItems.RemoveRange(existing.Items);
            
            // 2. ✅ DỌN SẠCH danh sách items trong bộ nhớ
            existing.Items.Clear();

            // 3. Thêm các items mới từ request vào danh sách rỗng
            if (hd.Items != null && hd.Items.Any())
            {
                foreach (var item in hd.Items)
                {
                    existing.Items.Add(new HoaDonItem
                    {
                        TenHang = item.TenHang,
                        SoLuong = item.SoLuong,
                        GiaTien = item.GiaTien
                    });
                }
            }

            // 4. ✅ TÍNH LẠI tổng tiền chỉ dựa trên các items mới
            existing.TongTien = existing.TinhTongTien();

            try
            {
                // 5. Lưu tất cả thay đổi vào DB
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.HoaDons.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // 🔹 Xóa hóa đơn
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var hd = await _context.HoaDons
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (hd == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn để xóa!" });
            }

            _context.HoaDons.Remove(hd);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}