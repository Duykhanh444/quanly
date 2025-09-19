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
            return await _context.HoaDons
                .Include(h => h.Items)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HoaDon>> GetById(int id)
        {
            var hd = await _context.HoaDons
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hd == null) return NotFound();

            return hd;
        }

        [HttpPost]
        public async Task<ActionResult<HoaDon>> Create(HoaDon hd)
        {
            hd.TongTien = TinhTongTien(hd);
            _context.HoaDons.Add(hd);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = hd.Id }, hd);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, HoaDon hd)
        {
            if (id != hd.Id) return BadRequest();

            hd.TongTien = TinhTongTien(hd);
            _context.Entry(hd).State = EntityState.Modified;

            foreach (var item in hd.Items)
            {
                if (item.Id == 0)
                    _context.Entry(item).State = EntityState.Added;
                else
                    _context.Entry(item).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.HoaDons.Any(e => e.Id == id))
                    return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var hd = await _context.HoaDons
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hd == null) return NotFound();

            _context.HoaDonItems.RemoveRange(hd.Items);
            _context.HoaDons.Remove(hd);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --------------------- ITEM ---------------------
        [HttpPost("{id}/Items")]
        public async Task<ActionResult<HoaDonItem>> AddItem(int id, HoaDonItem item)
        {
            var hd = await _context.HoaDons
                .Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hd == null) return NotFound();

            item.HoaDonId = hd.Id;
            hd.Items.Add(item);
            hd.TongTien = TinhTongTien(hd);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = hd.Id }, item);
        }

        [HttpDelete("Items/{itemId}")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            var item = await _context.HoaDonItems.FindAsync(itemId);
            if (item == null) return NotFound();

            var hd = await _context.HoaDons.Include(h => h.Items)
                .FirstOrDefaultAsync(h => h.Id == item.HoaDonId);

            _context.HoaDonItems.Remove(item);
            if (hd != null)
            {
                hd.TongTien = TinhTongTien(hd);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("Items/{itemId}")]
        public async Task<IActionResult> UpdateItem(int itemId, HoaDonItem item)
        {
            if (itemId != item.Id) return BadRequest();

            var existingItem = await _context.HoaDonItems
                .Include(i => i.HoaDon)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (existingItem == null) return NotFound();

            existingItem.TenHang = item.TenHang;
            existingItem.SoLuong = item.SoLuong;
            existingItem.GiaTien = item.GiaTien;

            if (existingItem.HoaDon != null)
            {
                existingItem.HoaDon.TongTien = TinhTongTien(existingItem.HoaDon);
            }

            await _context.SaveChangesAsync();
            return NoContent();
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

                if (!hoaDonsDaThanhToan.Any())
                    return NoContent();

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
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}
