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
            return await _context.KhoHang.ToListAsync();
        }

        // GET: api/KhoHang/5
        [HttpGet("{id}")]
        public async Task<ActionResult<KhoHang>> GetById(int id)
        {
            var kho = await _context.KhoHang.FindAsync(id);
            if (kho == null) return NotFound();
            return kho;
        }

        // POST: api/KhoHang
        [HttpPost]
        public async Task<ActionResult<KhoHang>> Create(KhoHang kho)
        {
            _context.KhoHang.Add(kho);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = kho.Id }, kho);
        }

        // PUT: api/KhoHang/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, KhoHang kho)
        {
            if (id != kho.Id) return BadRequest();

            _context.Entry(kho).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhoHangExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/KhoHang/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var kho = await _context.KhoHang.FindAsync(id);
            if (kho == null) return NotFound();

            _context.KhoHang.Remove(kho);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/KhoHang/Xuat/5
        [HttpPut("Xuat/{id}")]
        public async Task<IActionResult> XuatKho(int id)
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

        private bool KhoHangExists(int id)
        {
            return _context.KhoHang.Any(e => e.Id == id);
        }
    }
}
