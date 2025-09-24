using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMApi.Data;
using HRMApi.Models;

namespace HRMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HoaDonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --------------------- HÀM TIỆN ÍCH ---------------------
        private int TinhTongTien(HoaDon hd)
        {
            return hd.Items.Sum(i => i.SoLuong * i.GiaTien);
        }

        // --------------------- HÓA ĐƠN ---------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HoaDon>>> GetAll()
        {
            try
            {
                return Ok(await _context.HoaDons.Include(h => h.Items).ToListAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAll HoaDon Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HoaDon>> GetById(int id)
        {
            try
            {
                var hd = await _context.HoaDons.Include(h => h.Items).FirstOrDefaultAsync(h => h.Id == id);
                if (hd == null) return NotFound();
                return Ok(hd);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetById HoaDon Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<HoaDon>> Create(HoaDon hd)
        {
            try
            {
                hd.TongTien = TinhTongTien(hd);
                _context.HoaDons.Add(hd);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = hd.Id }, hd);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create HoaDon Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, HoaDon hd)
        {
            if (id != hd.Id) return BadRequest();

            try
            {
                hd.TongTien = TinhTongTien(hd);
                _context.Entry(hd).State = EntityState.Modified;

                foreach (var item in hd.Items)
                {
                    if (item.Id == 0)
                        _context.Entry(item).State = EntityState.Added;
                    else
                        _context.Entry(item).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.HoaDons.Any(e => e.Id == id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update HoaDon Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var hd = await _context.HoaDons.Include(h => h.Items).FirstOrDefaultAsync(h => h.Id == id);
                if (hd == null) return NotFound();

                _context.HoaDonItems.RemoveRange(hd.Items);
                _context.HoaDons.Remove(hd);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete HoaDon Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // --------------------- XÓA TẤT CẢ HÓA ĐƠN ĐÃ THANH TOÁN ---------------------
        [HttpDelete("xoa-tat-ca-da-thanh-toan")]
        public async Task<IActionResult> XoaTatCaDaThanhToan()
        {
            try
            {
                var hoaDonsDaThanhToan = await _context.HoaDons
                    .Include(h => h.Items)
                    .Where(hd => hd.TrangThai == "Đã thanh toán")
                    .ToListAsync();

                if (!hoaDonsDaThanhToan.Any()) return NoContent();

                foreach (var hd in hoaDonsDaThanhToan)
                {
                    _context.HoaDonItems.RemoveRange(hd.Items);
                }

                _context.HoaDons.RemoveRange(hoaDonsDaThanhToan);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"XoaTatCaDaThanhToan Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}
