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

        // GET: api/KhoHang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KhoHang>>> GetAll()
        {
            try
            {
                return Ok(await _context.KhoHang.ToListAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAll KhoHang Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // GET: api/KhoHang/5
        [HttpGet("{id}")]
        public async Task<ActionResult<KhoHang>> GetById(int id)
        {
            try
            {
                var kho = await _context.KhoHang.FindAsync(id);
                if (kho == null) return NotFound();
                return Ok(kho);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetById KhoHang Exception: {ex}");
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // POST: api/KhoHang
        [HttpPost]
        public async Task<ActionResult<KhoHang>> Create(KhoHang kho)
        {
            try
            {
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

        // PUT: api/KhoHang/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, KhoHang kho)
        {
            if (id != kho.Id) return BadRequest();

            try
            {
                _context.Entry(kho).State = EntityState.Modified;
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

        // DELETE: api/KhoHang/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var kho = await _context.KhoHang.FindAsync(id);
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

        // PUT: api/KhoHang/Xuat/5
        [HttpPut("Xuat/{id}")]
        public async Task<IActionResult> XuatKho(int id)
        {
            try
            {
                var kho = await _context.KhoHang.FindAsync(id);
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
