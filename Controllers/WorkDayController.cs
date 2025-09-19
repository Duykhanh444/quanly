using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMApi.Data;
using HRMApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkDayController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorkDayController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📋 GET: api/WorkDay
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetWorkDays()
        {
            var workDays = await _context.WorkDays
                .Include(w => w.NhanVien)
                .ToListAsync();

            var result = workDays.Select(w => new
            {
                w.Id,
                w.Ngay,
                w.SoGio,
                w.NhanVienId,
                NhanVienTen = w.NhanVien?.HoTen,
                TongSoGioDaChamCong = w.NhanVien?.WorkDays.Sum(x => x.SoGio) ?? 0
            });

            return Ok(result);
        }

        // 📋 GET: api/WorkDay/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetWorkDay(int id)
        {
            var workDay = await _context.WorkDays
                .Include(w => w.NhanVien)
                .ThenInclude(n => n.WorkDays)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workDay == null)
                return NotFound();

            var result = new
            {
                workDay.Id,
                workDay.Ngay,
                workDay.SoGio,
                workDay.NhanVienId,
                NhanVienTen = workDay.NhanVien?.HoTen,
                TongSoGioDaChamCong = workDay.NhanVien?.WorkDays.Sum(x => x.SoGio) ?? 0
            };

            return Ok(result);
        }

        // ➕ POST: api/WorkDay
        [HttpPost]
        public async Task<ActionResult<WorkDay>> PostWorkDay([FromBody] WorkDay workDay)
        {
            _context.WorkDays.Add(workDay);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWorkDay), new { id = workDay.Id }, workDay);
        }

        // ✏️ PUT: api/WorkDay/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWorkDay(int id, [FromBody] WorkDay workDay)
        {
            if (id != workDay.Id)
                return BadRequest();

            _context.Entry(workDay).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.WorkDays.Any(w => w.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // 🗑️ DELETE: api/WorkDay/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkDay(int id)
        {
            var workDay = await _context.WorkDays.FindAsync(id);
            if (workDay == null)
                return NotFound();

            _context.WorkDays.Remove(workDay);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
